using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public record Demands(IList<Demand> All)
{
    public static Demands None()
    {
        return new Demands(new List<Demand>());
    }

    public static Demands Of(params Demand[] demands)
    {
        return new Demands(demands.ToList());
    }

    public Demands Add(Demands demands)
    {
        return new Demands(All.Concat(demands.All).ToList());
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