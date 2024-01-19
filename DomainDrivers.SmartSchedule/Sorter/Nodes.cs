using System.Collections.Frozen;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Sorter;

public record Nodes<T>(ISet<Node<T>> NodesCollection)
{
    public Nodes(params Node<T>[] nodes) : this(new HashSet<Node<T>>(nodes))
    {
    }

    public ISet<Node<T>> All
    {
        get { return NodesCollection.ToFrozenSet(); }
    }

    public Nodes<T> Add(Node<T> node)
    {
        var newNodes = new HashSet<Node<T>>(NodesCollection) { node };
        return new Nodes<T>(newNodes);
    }

    public Nodes<T> WithAllDependenciesPresentIn(IEnumerable<Node<T>> nodes)
    {
        return new Nodes<T>(All
            .Where(n => n.Dependencies.All.All(d => nodes.Contains(d)))
            .ToHashSet());
    }

    public Nodes<T> RemoveAll(IEnumerable<Node<T>> nodes)
    {
        return new Nodes<T>(All
            .Where(n => !nodes.Contains(n))
            .ToHashSet());
    }

    public virtual bool Equals(Nodes<T>? other)
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