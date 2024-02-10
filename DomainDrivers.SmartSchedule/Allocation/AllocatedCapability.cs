using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public record AllocatedCapability(Guid AllocatedCapabilityId, Guid ResourceId, Capability Capability, TimeSlot TimeSlot)
{
    public AllocatedCapability(Guid resourceId, Capability capability, TimeSlot forSlot) : this(Guid.NewGuid(),
        resourceId, capability, forSlot)
    {
    }

    public virtual bool Equals(AllocatedCapability? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ResourceId == other.ResourceId && Capability == other.Capability && TimeSlot == other.TimeSlot;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ResourceId, Capability, TimeSlot);
    }
}