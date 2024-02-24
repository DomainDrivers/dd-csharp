using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public class AllocatableCapability
{
    public AllocatableCapabilityId Id { get; } = AllocatableCapabilityId.NewOne();

    public CapabilitySelector Capabilities { get; }

    public AllocatableResourceId ResourceId { get; }

    public TimeSlot TimeSlot { get; }

    public AllocatableCapability(AllocatableResourceId resourceId, CapabilitySelector capabilities, TimeSlot timeSlot)
    {
        ResourceId = resourceId;
        Capabilities = capabilities;
        //https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities#by-design-restrictions
        TimeSlot = new TimeSlot(timeSlot.From, timeSlot.To);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private AllocatableCapability(){}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public bool CanPerform(ISet<Capability> capabilities)
    {
        return Capabilities.CanPerform(capabilities);
    }
}