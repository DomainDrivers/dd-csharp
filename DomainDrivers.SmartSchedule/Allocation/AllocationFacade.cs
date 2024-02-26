using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation;

public class AllocationFacade
{
    private readonly IAllocationDbContext _allocationDbContext;
    private readonly AvailabilityFacade _availabilityFacade;
    private readonly CapabilityFinder _capabilityFinder;
    private readonly TimeProvider _timeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AllocationFacade(IAllocationDbContext allocationDbContext, AvailabilityFacade availabilityFacade,
        CapabilityFinder capabilityFinder, TimeProvider timeProvider, IUnitOfWork unitOfWork)
    {
        _allocationDbContext = allocationDbContext;
        _availabilityFacade = availabilityFacade;
        _capabilityFinder = capabilityFinder;
        _timeProvider = timeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectAllocationsId> CreateAllocation(TimeSlot timeSlot, Demands scheduledDemands)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var projectId = ProjectAllocationsId.NewOne();
            var projectAllocations = new ProjectAllocations(projectId, Allocations.None(), scheduledDemands, timeSlot);
            await _allocationDbContext.ProjectAllocations.AddAsync(projectAllocations);
            return projectId;
        });
    }

    public async Task<ProjectsAllocationsSummary> FindAllProjectsAllocations(ISet<ProjectAllocationsId> projectIds)
    {
        //ToArray cast is needed for query to be compiled properly
        return ProjectsAllocationsSummary.Of(await _allocationDbContext.ProjectAllocations
            .Where(x => projectIds.ToArray().Contains(x.ProjectId))
            .ToListAsync());
    }

    public async Task<ProjectsAllocationsSummary> FindAllProjectsAllocations()
    {
        return ProjectsAllocationsSummary.Of(await _allocationDbContext.ProjectAllocations.ToListAsync());
    }

    public async Task<Guid?> AllocateToProject(ProjectAllocationsId projectId, AllocatableCapabilityId allocatableCapabilityId,
        Capability capability, TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction<Guid?>(async () =>
        {
            //yes, one transaction crossing 2 modules.
            if (!await _capabilityFinder.IsPresent(allocatableCapabilityId))
            {
                return null;
            }

            if (!await _availabilityFacade.Block(allocatableCapabilityId.ToAvailabilityResourceId(), timeSlot,
                    Owner.Of(projectId.Id)))
            {
                return null;
            }

            var @event = await Allocate(projectId, allocatableCapabilityId, capability, timeSlot);
            if (@event == null)
            {
                return null;
            }

            return @event.AllocatedCapabilityId;
        });
    }

    private async Task<CapabilitiesAllocated?> Allocate(ProjectAllocationsId projectId,
        AllocatableCapabilityId allocatableCapabilityId, Capability capability, TimeSlot timeSlot)
    {
        var allocations = await _allocationDbContext.ProjectAllocations.SingleAsync(x => x.ProjectId == projectId);
        var @event = allocations.Allocate(allocatableCapabilityId, capability, timeSlot, _timeProvider.GetUtcNow().DateTime);
        return @event;
    }

    public async Task<bool> ReleaseFromProject(ProjectAllocationsId projectId, AllocatableCapabilityId allocatableCapabilityId, TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            //can release not scheduled capability - at least for now. Hence no check to capabilityFinder
            await _availabilityFacade.Release(allocatableCapabilityId.ToAvailabilityResourceId(), timeSlot,
                Owner.Of(projectId.Id));
            var allocations = await _allocationDbContext.ProjectAllocations.SingleAsync(x => x.ProjectId == projectId);
            var @event = allocations.Release(allocatableCapabilityId, timeSlot, _timeProvider.GetUtcNow().DateTime);
            return @event != null;
        });
    }

    public async Task<bool> AllocateCapabilityToProjectForPeriod(ProjectAllocationsId projectId, Capability capability,
        TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var proposedCapabilities = await _capabilityFinder.FindCapabilities(capability, timeSlot);

            if (proposedCapabilities.All.Count == 0)
            {
                return false;
            }

            var availabilityResourceIds = proposedCapabilities.All
                .Select(resource => resource.Id.ToAvailabilityResourceId())
                .ToHashSet();
            var chosen =
                await _availabilityFacade.BlockRandomAvailable(availabilityResourceIds, timeSlot,
                    Owner.Of(projectId.Id));

            if (chosen == null)
            {
                return false;
            }

            var toAllocate = FindChosenAllocatableCapability(proposedCapabilities, chosen);
            return await Allocate(projectId, toAllocate, capability, timeSlot) != null;
        });
    }

    private AllocatableCapabilityId FindChosenAllocatableCapability(AllocatableCapabilitiesSummary proposedCapabilities,
        ResourceId chosen)
    {
        return proposedCapabilities.All
            .Select(x => x.Id)
            .First(id => id.ToAvailabilityResourceId() == chosen);
    }

    public async Task EditProjectDates(ProjectAllocationsId projectId, TimeSlot fromTo)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var projectAllocations =
                await _allocationDbContext.ProjectAllocations.SingleAsync(x => x.ProjectId == projectId);
            projectAllocations.DefineSlot(fromTo, _timeProvider.GetUtcNow().DateTime);
        });
    }
    
    public async Task ScheduleProjectAllocationDemands(ProjectAllocationsId projectId, Demands demands)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var projectAllocations = await _allocationDbContext.ProjectAllocations.FindAsync(projectId);
            if (projectAllocations == null)
            {
                projectAllocations = ProjectAllocations.Empty(projectId);
                await _allocationDbContext.ProjectAllocations.AddAsync(projectAllocations);
            }

            projectAllocations.AddDemands(demands, _timeProvider.GetUtcNow().DateTime);
        });
    }
}