using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public record Demands(IList<Demand> All)
{
    public Demands MissingDemands(Allocations allocations)
    {
        return new Demands(All.Where(d => !SatisfiedBy(d, allocations)).ToList());
    }

    private static bool SatisfiedBy(Demand d, Allocations allocations)
    {
        return allocations.All.Any(ar => ar.Capability == d.Capability && d.Slot.Within(ar.TimeSlot));
    }

    public virtual bool Equals(Demands? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return All.SequenceEqual(other.All);
    }

    public override int GetHashCode()
    {
        return All.CalculateHashCode();
    }
}