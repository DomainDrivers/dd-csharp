using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public record Stage(string StageName, ISet<Stage> Dependencies, ISet<ResourceName> Resources, TimeSpan Duration)
{
    public Stage OfDuration(TimeSpan duration)
    {
        return new Stage(StageName, Dependencies, Resources, duration);
    }

    public Stage(string name) : this(name, new HashSet<Stage>(), new HashSet<ResourceName>(), TimeSpan.Zero)
    {
    }

    public Stage DependsOn(Stage stage)
    {
        var newDependencies = new HashSet<Stage>(Dependencies) { stage };
        Dependencies.Add(stage);
        return new Stage(StageName, newDependencies, Resources, Duration);
    }

    public string Name
    {
        get { return StageName; }
    }

    public Stage WithChosenResourceCapabilities(params ResourceName[] resources)
    {
        var collect = new HashSet<ResourceName>(resources);
        return new Stage(StageName, Dependencies, collect, Duration);
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