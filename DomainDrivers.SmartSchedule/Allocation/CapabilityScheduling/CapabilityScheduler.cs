using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public class CapabilityScheduler
{
    private readonly IAvailabilityFacade _availabilityFacade;
    private readonly AllocatableCapabilityRepository _allocatableResourceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CapabilityScheduler(IAvailabilityFacade availabilityFacade,
        AllocatableCapabilityRepository allocatableResourceRepository, IUnitOfWork unitOfWork)
    {
        _availabilityFacade = availabilityFacade;
        _allocatableResourceRepository = allocatableResourceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<AllocatableCapabilityId>> ScheduleResourceCapabilitiesForPeriod(
        AllocatableResourceId resourceId, IList<CapabilitySelector> capabilities, TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var allocatableResourceIds = await CreateAllocatableResources(resourceId, capabilities, timeSlot);

            foreach (var resource in allocatableResourceIds)
            {
                await _availabilityFacade.CreateResourceSlots(resource.ToAvailabilityResourceId(), timeSlot);
            }

            return allocatableResourceIds;
        });
    }

    public async Task<IList<AllocatableCapabilityId>> ScheduleMultipleResourcesForPeriod(
        ISet<AllocatableResourceId> resources, Capability capability, TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var allocatableCapability =
                resources
                    .Select(resource =>
                        new AllocatableCapability(resource, CapabilitySelector.CanJustPerform(capability),
                            timeSlot))
                    .ToList();
            await _allocatableResourceRepository.SaveAll(allocatableCapability);

            foreach (var resource in allocatableCapability)
            {
                await _availabilityFacade.CreateResourceSlots(resource.Id.ToAvailabilityResourceId(), timeSlot);
            }

            return allocatableCapability
                .Select(x => x.Id)
                .ToList();
        });
    }

    private async Task<IList<AllocatableCapabilityId>> CreateAllocatableResources(AllocatableResourceId resourceId,
        IList<CapabilitySelector> capabilities, TimeSlot timeSlot)
    {
        var allocatableResources = capabilities
            .Select(capability => new AllocatableCapability(resourceId, capability, timeSlot))
            .ToList();
        await _allocatableResourceRepository.SaveAll(allocatableResources);
        return allocatableResources
            .Select(x => x.Id)
            .ToList();
    }

    public async Task<AllocatableCapabilityId?> FindResourceCapabilities(AllocatableResourceId resourceId,
        Capability capability, TimeSlot period)
    {
        var result = await _allocatableResourceRepository
            .FindByResourceIdAndCapabilityAndTimeSlot(resourceId.Id, capability.Name, capability.Type, period.From,
                period.To);

        if (result == null)
        {
            return null;
        }

        return result.Id;
    }

    public async Task<AllocatableCapabilityId?> FindResourceCapabilities(AllocatableResourceId allocatableResourceId,
        ISet<Capability> capabilities, TimeSlot timeSlot)
    {
        return (await _allocatableResourceRepository
                .FindByResourceIdAndTimeSlot(allocatableResourceId.Id, timeSlot.From, timeSlot.To))
            .Where(ac => ac.CanPerform(capabilities))
            .Select(allocatableCapability => allocatableCapability.Id)
            .FirstOrDefault();
    }
}