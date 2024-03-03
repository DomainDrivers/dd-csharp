using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public record Demands(IList<Demand> All)
{
    public static Demands None()
    {
        return new Demands(new List<Demand>());
    }

    public static Demands Of(params Demand[] demands)
    {
        return new Demands(demands);
    }
    
    public static Demands AllInSameTimeSlot(TimeSlot slot, params Capability[] capabilities)
    {
        return new Demands(capabilities.Select(c => new Demand(c, slot)).ToList());
    }

    public Demands MissingDemands(Allocations allocations)
    {
        return new Demands(All.Where(d => !SatisfiedBy(d, allocations)).ToList());
    }

    private static bool SatisfiedBy(Demand d, Allocations allocations)
    {
        return allocations.All.Any(ar => ar.Capability.CanPerform(d.Capability) && d.Slot.Within(ar.TimeSlot));
    }
    
    public Demands WithNew(Demands newDemands) {
        var all = new List<Demand>(All);
        all.AddRange(newDemands.All);
        return new Demands(all);
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