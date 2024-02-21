using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public class CapabilityScheduler
{
    private readonly AvailabilityFacade _availabilityFacade;
    private readonly AllocatableCapabilityRepository _allocatableResourceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CapabilityScheduler(AvailabilityFacade availabilityFacade,
        AllocatableCapabilityRepository allocatableResourceRepository,
        IUnitOfWork unitOfWork)
    {
        _availabilityFacade = availabilityFacade;
        _allocatableResourceRepository = allocatableResourceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<AllocatableCapabilityId>> ScheduleResourceCapabilitiesForPeriod(
        AllocatableResourceId resourceId, IList<Capability> capabilities, TimeSlot timeSlot)
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

    public async Task<List<AllocatableCapabilityId>> ScheduleMultipleResourcesForPeriod(
        ISet<AllocatableResourceId> resources, Capability capability, TimeSlot timeSlot)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var allocatableCapability =
                resources
                    .Select(resource => new AllocatableCapability(resource, capability, timeSlot))
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
        IList<Capability> capabilities, TimeSlot timeSlot)
    {
        var allocatableResources = capabilities
            .Select(capability => new AllocatableCapability(resourceId, capability, timeSlot))
            .ToList();
        await _allocatableResourceRepository.SaveAll(allocatableResources);
        return allocatableResources
            .Select(x => x.Id)
            .ToList();
    }
}