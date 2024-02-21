using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public class AllocatableCapability
{
    public AllocatableCapabilityId Id { get; } = AllocatableCapabilityId.NewOne();
    
    public Capability Capability { get; }
    
    public AllocatableResourceId ResourceId { get; }
    
    public TimeSlot TimeSlot { get; }

    public AllocatableCapability(AllocatableResourceId resourceId, Capability capability, TimeSlot timeSlot)
    {
        ResourceId = resourceId;
        Capability = capability;
        //https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities#by-design-restrictions
        TimeSlot = new TimeSlot(timeSlot.From, timeSlot.To);
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private AllocatableCapability(){}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
    public bool CanPerform(Capability capability) {
        return Capability == capability;
    }
}