using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation;

public class AllocationFacade
{
    private readonly IAllocationDbContext _allocationDbContext;
    private readonly TimeProvider _timeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AllocationFacade(IAllocationDbContext allocationDbContext, TimeProvider timeProvider, IUnitOfWork unitOfWork)
    {
        _allocationDbContext = allocationDbContext;
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

    public async Task<Guid?> AllocateToProject(ProjectAllocationsId projectId, ResourceId resourceId,
        Capability capability, TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction<Guid?>(async () =>
        {
            var allocations = await _allocationDbContext.ProjectAllocations.SingleAsync(x => x.ProjectId == projectId);
            var @event = allocations.Allocate(resourceId, capability, timeSlot, _timeProvider.GetUtcNow().DateTime);
            if (@event == null)
            {
                return null;
            }

            return @event.AllocatedCapabilityId;
        });
    }
    
    public async Task<bool> ReleaseFromProject(ProjectAllocationsId projectId, Guid allocatableCapabilityId, TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var allocations = await _allocationDbContext.ProjectAllocations.SingleAsync(x => x.ProjectId == projectId);
            var @event = allocations.Release(allocatableCapabilityId, timeSlot, _timeProvider.GetUtcNow().DateTime);
            return @event != null;
        });
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