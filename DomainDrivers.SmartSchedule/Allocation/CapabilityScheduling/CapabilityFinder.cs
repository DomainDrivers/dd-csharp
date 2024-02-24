using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public class CapabilityFinder
{
    private readonly AvailabilityFacade _availabilityFacade;
    private readonly AllocatableCapabilityRepository _allocatableResourceRepository;

    public CapabilityFinder(AvailabilityFacade availabilityFacade,
        AllocatableCapabilityRepository allocatableResourceRepository)
    {
        _availabilityFacade = availabilityFacade;
        _allocatableResourceRepository = allocatableResourceRepository;
    }

    public async Task<AllocatableCapabilitiesSummary> FindAvailableCapabilities(Capability capability,
        TimeSlot timeSlot)
    {
        var findAllocatableCapability =
            await _allocatableResourceRepository.FindByCapabilityWithin(capability.Name, capability.Type, timeSlot.From,
                timeSlot.To);
        var found = await FilterAvailabilityInTimeSlot(findAllocatableCapability, timeSlot);
        return CreateSummary(found);
    }

    public async Task<AllocatableCapabilitiesSummary> FindCapabilities(Capability capability, TimeSlot timeSlot)
    {
        var found = await _allocatableResourceRepository.FindByCapabilityWithin(capability.Name, capability.Type,
            timeSlot.From, timeSlot.To);
        return CreateSummary(found);
    }

    public async Task<AllocatableCapabilitiesSummary> FindById(IList<AllocatableCapabilityId> allocatableCapabilityIds)
    {
        var allByIdIn = await _allocatableResourceRepository.FindAllById(allocatableCapabilityIds);
        return CreateSummary(allByIdIn);
    }

    private async Task<IList<AllocatableCapability>> FilterAvailabilityInTimeSlot(
        IList<AllocatableCapability> findAllocatableCapability, TimeSlot timeSlot)
    {
        var resourceIds =
            findAllocatableCapability
                .Select(ac => ac.Id.ToAvailabilityResourceId())
                .ToHashSet();
        var calendars = await _availabilityFacade.LoadCalendars(resourceIds, timeSlot);
        return findAllocatableCapability
            .Where(ac => calendars.CalendarsDictionary[ac.Id.ToAvailabilityResourceId()].AvailableSlots()
                .Contains(timeSlot))
            .ToList();
    }

    private AllocatableCapabilitiesSummary CreateSummary(IList<AllocatableCapability> from)
    {
        return new AllocatableCapabilitiesSummary(
            from.Select(allocatableCapability => new AllocatableCapabilitySummary(allocatableCapability.Id,
                    allocatableCapability.ResourceId, allocatableCapability.Capabilities, allocatableCapability.TimeSlot))
                .ToList());
    }
}