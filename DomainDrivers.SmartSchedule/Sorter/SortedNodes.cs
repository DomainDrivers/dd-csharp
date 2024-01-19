using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Sorter;

public record SortedNodes<T>(IList<Nodes<T>> All)
{
    public static SortedNodes<T> Empty()
    {
        return new SortedNodes<T>(new List<Nodes<T>>());
    }

    public SortedNodes<T> Add(Nodes<T> newNodes)
    {
        var result = new List<Nodes<T>>(All) { newNodes };
        return new SortedNodes<T>(result);
    }

    public virtual bool Equals(SortedNodes<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return All.SequenceEqual(other.All);
    }

    public override int GetHashCode()
    {
        return All.CalculateHashCode();
    }

    public override string ToString()
    {
        return $"SortedNodes: {All.ToCollectionString()}";
    }
}
