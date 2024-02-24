using System.Text.Json.Serialization;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

[method: JsonConstructor]
public record AllocatedCapability(AllocatableCapabilityId AllocatedCapabilityId, Capability Capability, TimeSlot TimeSlot)
{
    public virtual bool Equals(AllocatedCapability? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return AllocatedCapabilityId == other.AllocatedCapabilityId && Capability == other.Capability && TimeSlot == other.TimeSlot;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AllocatedCapabilityId, Capability, TimeSlot);
    }
}