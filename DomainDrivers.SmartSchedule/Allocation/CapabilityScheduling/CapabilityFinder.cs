﻿using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public interface ICapabilityFinder
{
    Task<AllocatableCapabilitiesSummary> FindAvailableCapabilities(Capability capability,
        TimeSlot timeSlot);

    Task<AllocatableCapabilitiesSummary> FindCapabilities(Capability capability, TimeSlot timeSlot);

    Task<AllocatableCapabilitiesSummary> FindById(IList<AllocatableCapabilityId> allocatableCapabilityIds);

    Task<AllocatableCapabilitySummary?> FindById(AllocatableCapabilityId allocatableCapabilityId);
}

public class CapabilityFinder : ICapabilityFinder
{
    private readonly IAvailabilityFacade _availabilityFacade;
    private readonly AllocatableCapabilityRepository _allocatableResourceRepository;

    public CapabilityFinder(IAvailabilityFacade availabilityFacade,
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

    public async Task<AllocatableCapabilitySummary?> FindById(AllocatableCapabilityId allocatableCapabilityId)
    {
        var allocatableCapability = await _allocatableResourceRepository.FindById(allocatableCapabilityId);
        
        if (allocatableCapability == null)
        {
            return null;
        }

        return CreateSummary(allocatableCapability);
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
        return new AllocatableCapabilitiesSummary(from
            .Select(allocatableCapability => CreateSummary(allocatableCapability)).ToList());
    }

    private AllocatableCapabilitySummary CreateSummary(AllocatableCapability allocatableCapability)
    {
        return new AllocatableCapabilitySummary(allocatableCapability.Id, allocatableCapability.ResourceId,
            allocatableCapability.Capabilities, allocatableCapability.TimeSlot);
    }
}