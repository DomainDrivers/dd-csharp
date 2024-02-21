using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public record AllocatableCapabilitySummary(AllocatableCapabilityId Id, AllocatableResourceId AllocatableResourceId,
    Capability Capability, TimeSlot TimeSlot);