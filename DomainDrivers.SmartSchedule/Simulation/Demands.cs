using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Simulation;

public record Demands(IList<Demand> All)
{
    public static Demands Of(params Demand[] demands)
    {
        return new Demands(demands.ToList());
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