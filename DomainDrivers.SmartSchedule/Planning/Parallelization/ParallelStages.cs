using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public record ParallelStages(ISet<Stage> Stages)
{
    public string Print()
    {
        return string.Join(", ", Stages.Select(stage => stage.StageName).OrderBy(name => name));
    }

    public virtual bool Equals(ParallelStages? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Stages.SetEquals(other.Stages);
    }

    public override int GetHashCode()
    {
        return Stages.CalculateHashCode();
    }
}