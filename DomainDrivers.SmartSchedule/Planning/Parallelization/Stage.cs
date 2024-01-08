namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public record Stage(string StageName, ISet<Stage> Dependencies, ISet<ResourceName> Resources, TimeSpan Duration)
{
    public Stage(string name) : this(name, new HashSet<Stage>(), new HashSet<ResourceName>(), TimeSpan.Zero)
    {
    }

    public Stage DependsOn(Stage stage)
    {
        Dependencies.Add(stage);
        return this;
    }

    public string Name
    {
        get { return StageName; }
    }

    public virtual bool Equals(Stage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return StageName == other.StageName;
    }

    public override int GetHashCode()
    {
        return StageName.GetHashCode();
    }
}

public record ResourceName(string Name);