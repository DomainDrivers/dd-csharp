using DomainDrivers.SmartSchedule.Planning.Parallelization;

namespace DomainDrivers.SmartSchedule.Sorter;

public record Node(string Name, Nodes Dependencies, Stage? Content)
{
    public Node(string name) : this(name, new Nodes(new HashSet<Node>()), null)
    {
    }

    public Node(string name, Stage? content) : this(name, new Nodes(new HashSet<Node>()), content)
    {
    }

    public Node DependsOn(Node node)
    {
        return new Node(Name, Dependencies.Add(node), Content);
    }

    public virtual bool Equals(Node? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}