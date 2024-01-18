using System.Collections.Frozen;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Sorter;

public record Nodes(ISet<Node> NodesCollection)
{
    public Nodes(params Node[] nodes) : this(new HashSet<Node>(nodes))
    {
    }

    public ISet<Node> All
    {
        get { return NodesCollection.ToFrozenSet(); }
    }

    public Nodes Add(Node node)
    {
        var newNodes = new HashSet<Node>(NodesCollection) { node };
        return new Nodes(newNodes);
    }

    public Nodes WithAllDependenciesPresentIn(IEnumerable<Node> nodes)
    {
        return new Nodes(All
            .Where(n => n.Dependencies.All.All(d => nodes.Contains(d)))
            .ToHashSet());
    }

    public Nodes RemoveAll(IEnumerable<Node> nodes)
    {
        return new Nodes(All
            .Where(n => !nodes.Contains(n))
            .ToHashSet());
    }

    public virtual bool Equals(Nodes? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return NodesCollection.SetEquals(other.NodesCollection);
    }

    public override int GetHashCode()
    {
        return NodesCollection.CalculateHashCode();
    }

    public override string ToString()
    {
        return $"Nodes{{node={NodesCollection.ToCollectionString()}}}";
    }
}