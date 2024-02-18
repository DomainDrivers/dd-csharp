using DomainDrivers.SmartSchedule.Availability;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public record Stage(string StageName, ISet<Stage> Dependencies, ISet<ResourceId> Resources, TimeSpan Duration)
{
    public Stage OfDuration(TimeSpan duration)
    {
        return new Stage(StageName, Dependencies, Resources, duration);
    }

    public Stage(string name) : this(name, new HashSet<Stage>(), new HashSet<ResourceId>(), TimeSpan.Zero)
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

    public Stage WithChosenResourceCapabilities(params ResourceId[] resources)
    {
        var collect = new HashSet<ResourceId>(resources);
        return new Stage(StageName, Dependencies, collect, Duration);
    }

    public virtual bool Equals(Stage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return StageName == other.StageName
               && Dependencies.SetEquals(other.Dependencies)
               && Resources.SetEquals(other.Resources)
               && Duration == other.Duration;
    }
    
    public override int GetHashCode()
    {
        return StageName.GetHashCode();
    }

    public override string ToString()
    {
        return StageName;
    }
}