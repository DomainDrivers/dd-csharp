using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Optimization;

public record TotalCapacity(IList<ICapacityDimension> CapacitiesList)
{
    public static TotalCapacity Of(params ICapacityDimension[] capacities)
    {
        return new TotalCapacity(capacities.ToList());
    }

    public static TotalCapacity Of(IList<ICapacityDimension> capacities)
    {
        return new TotalCapacity(capacities);
    }

    public static TotalCapacity Zero()
    {
        return new TotalCapacity(new List<ICapacityDimension>());
    }

    public int Size
    {
        get { return CapacitiesList.Count; }
    }

    public IList<ICapacityDimension> Capacities()
    {
        return new List<ICapacityDimension>(CapacitiesList);
    }

    public TotalCapacity Add(IList<ICapacityDimension> capacities)
    {
        var newCapacities = new List<ICapacityDimension>(CapacitiesList);
        newCapacities.AddRange(capacities);
        return new TotalCapacity(newCapacities);
    }

    public virtual bool Equals(TotalCapacity? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return CapacitiesList.SequenceEqual(other.CapacitiesList);
    }

    public override int GetHashCode()
    {
        return CapacitiesList.CalculateHashCode();
    }
}