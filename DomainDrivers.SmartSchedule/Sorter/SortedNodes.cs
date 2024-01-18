using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Sorter;

public record SortedNodes(IList<Nodes> All)
{
    public static SortedNodes Empty()
    {
        return new SortedNodes(new List<Nodes>());
    }

    public SortedNodes Add(Nodes newNodes)
    {
        var result = new List<Nodes>(All) { newNodes };
        return new SortedNodes(result);
    }

    public virtual bool Equals(SortedNodes? other)
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
