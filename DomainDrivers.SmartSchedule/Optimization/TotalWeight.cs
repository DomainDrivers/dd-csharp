using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Optimization;

public record TotalWeight(IList<IWeightDimension> ComponentsList)
{
    public static TotalWeight Zero()
    {
        return new TotalWeight(new List<IWeightDimension>());
    }

    public static TotalWeight Of(params IWeightDimension[] components)
    {
        return new TotalWeight(components.ToList());
    }

    public IList<IWeightDimension> Components()
    {
        return new List<IWeightDimension>(ComponentsList);
    }

    public virtual bool Equals(TotalWeight? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ComponentsList.SequenceEqual(other.ComponentsList);
    }

    public override int GetHashCode()
    {
        return ComponentsList.CalculateHashCode();
    }
}