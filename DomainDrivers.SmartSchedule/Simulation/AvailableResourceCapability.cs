using DomainDrivers.SmartSchedule.Optimization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Simulation;

public record AvailableResourceCapability(Guid ResourceId, CapabilitySelector CapabilitySelector, TimeSlot TimeSlot)
    : ICapacityDimension
{
    public AvailableResourceCapability(Guid resourceId, Capability capability, TimeSlot timeSlot) : this(resourceId,
        CapabilitySelector.CanJustPerform(capability), timeSlot)
    {
    }

    public bool Performs(Capability capability)
    {
        return CapabilitySelector.CanPerform(new HashSet<Capability>() { capability });
    }
}