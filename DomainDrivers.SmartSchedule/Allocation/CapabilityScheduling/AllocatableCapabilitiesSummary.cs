using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public record AllocatableCapabilitiesSummary(IList<AllocatableCapabilitySummary> All)
{
    public override int GetHashCode()
    {
        return All.CalculateHashCode();
    }
    
    public virtual bool Equals(AllocatableCapabilitiesSummary? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return All.SequenceEqual(other.All);
    }
}