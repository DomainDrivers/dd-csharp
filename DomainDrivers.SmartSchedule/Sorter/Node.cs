
namespace DomainDrivers.SmartSchedule.Sorter;

public record Node<T>(string Name, Nodes<T> Dependencies, T? Content)
{
    public Node(string name) : this(name, new Nodes<T>(new HashSet<Node<T>>()), default)
    {
    }

    public Node(string name, T? content) : this(name, new Nodes<T>(new HashSet<Node<T>>()), content)
    {
    }

    public Node<T> DependsOn(Node<T> node)
    {
        return new Node<T>(Name, Dependencies.Add(node), Content);
    }

    public virtual bool Equals(Node<T>? other)
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