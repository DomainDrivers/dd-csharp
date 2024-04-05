using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public class AllocationFacade
{
    private readonly IProjectAllocationsRepository _projectAllocationsRepository;
    private readonly IAvailabilityFacade _availabilityFacade;
    private readonly ICapabilityFinder _capabilityFinder;
    private readonly IEventsPublisher _eventsPublisher;
    private readonly TimeProvider _timeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AllocationFacade(IProjectAllocationsRepository projectAllocationsRepository, IAvailabilityFacade availabilityFacade,
        ICapabilityFinder capabilityFinder, IEventsPublisher eventsPublisher, TimeProvider timeProvider, IUnitOfWork unitOfWork)
    {
        _projectAllocationsRepository = projectAllocationsRepository;
        _availabilityFacade = availabilityFacade;
        _capabilityFinder = capabilityFinder;
        _eventsPublisher = eventsPublisher;
        _timeProvider = timeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectAllocationsId> CreateAllocation(TimeSlot timeSlot, Demands scheduledDemands)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var projectId = ProjectAllocationsId.NewOne();
            var projectAllocations = new ProjectAllocations(projectId, Allocations.None(), scheduledDemands, timeSlot);
            await _projectAllocationsRepository.Add(projectAllocations);
            await _eventsPublisher.Publish(new ProjectAllocationScheduled(projectId, timeSlot,
                _timeProvider.GetUtcNow().DateTime));
            return projectId;
        });
    }

    public async Task<ProjectsAllocationsSummary> FindAllProjectsAllocations(ISet<ProjectAllocationsId> projectIds)
    {
        return ProjectsAllocationsSummary.Of(await _projectAllocationsRepository.FindAllById(projectIds));
    }

    public async Task<ProjectsAllocationsSummary> FindAllProjectsAllocations()
    {
        return ProjectsAllocationsSummary.Of(await _projectAllocationsRepository.FindAll());
    }

    public async Task<Guid?> AllocateToProject(ProjectAllocationsId projectId,
        AllocatableCapabilityId allocatableCapabilityId, TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction<Guid?>(async () =>
        {
            //yes, one transaction crossing 2 modules.
            var capability = await _capabilityFinder.FindById(allocatableCapabilityId);
            if (capability == null)
            {
                return null;
            }

            if (!await _availabilityFacade.Block(allocatableCapabilityId.ToAvailabilityResourceId(), timeSlot,
                    Owner.Of(projectId.Id)))
            {
                return null;
            }

            var @event = await Allocate(projectId, allocatableCapabilityId, capability.Capabilities, timeSlot);
            if (@event == null)
            {
                return null;
            }

            return @event.AllocatedCapabilityId;
        });
    }

    private async Task<CapabilitiesAllocated?> Allocate(ProjectAllocationsId projectId,
        AllocatableCapabilityId allocatableCapabilityId, CapabilitySelector capability, TimeSlot timeSlot)
    {
        var allocations = await _projectAllocationsRepository.GetById(projectId);
        var @event = allocations.Allocate(allocatableCapabilityId, capability, timeSlot,
            _timeProvider.GetUtcNow().DateTime);
        await _projectAllocationsRepository.Update(allocations);
        return @event;
    }

    public async Task<bool> ReleaseFromProject(ProjectAllocationsId projectId,
        AllocatableCapabilityId allocatableCapabilityId, TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            //can release not scheduled capability - at least for now. Hence no check to capabilityFinder
            await _availabilityFacade.Release(allocatableCapabilityId.ToAvailabilityResourceId(), timeSlot,
                Owner.Of(projectId.Id));
            var allocations = await _projectAllocationsRepository.GetById(projectId);
            var @event = allocations.Release(allocatableCapabilityId, timeSlot, _timeProvider.GetUtcNow().DateTime);
            await _projectAllocationsRepository.Update(allocations);
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
            return await Allocate(projectId, toAllocate.Id, toAllocate.Capabilities, timeSlot) != null;
        });
    }

    private AllocatableCapabilitySummary FindChosenAllocatableCapability(AllocatableCapabilitiesSummary proposedCapabilities,
        ResourceId chosen)
    {
        return proposedCapabilities.All
            .First(summary => summary.Id.ToAvailabilityResourceId() == chosen);
    }

    public async Task EditProjectDates(ProjectAllocationsId projectId, TimeSlot fromTo)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var projectAllocations = await _projectAllocationsRepository.GetById(projectId);
            var projectDatesSet = projectAllocations.DefineSlot(fromTo, _timeProvider.GetUtcNow().DateTime);
            if (projectDatesSet != null)
            {
                await _eventsPublisher.Publish(projectDatesSet);
            }

            await _projectAllocationsRepository.Update(projectAllocations);
        });
    }

    public async Task ScheduleProjectAllocationDemands(ProjectAllocationsId projectId, Demands demands)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var projectAllocations = await _projectAllocationsRepository.FindById(projectId);
            if (projectAllocations == null)
            {
                projectAllocations = ProjectAllocations.Empty(projectId);
                await _projectAllocationsRepository.Add(projectAllocations);
            }
            else
            {
                await _projectAllocationsRepository.Update(projectAllocations);
            }

            var @event = projectAllocations.AddDemands(demands, _timeProvider.GetUtcNow().DateTime);
            //event could be stored in a local store
            //always remember about transactional boundaries
        });
    }
}