using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public record ParallelStages(ISet<Stage> Stages)
{
    public string Print()
    {
        return string.Join(", ", Stages.Select(stage => stage.StageName).OrderBy(name => name));
    }

    public static ParallelStages Of(params Stage[] stages)
    {
        return new ParallelStages(stages.ToHashSet());
    }

    public TimeSpan Duration
    {
        get
        {
            return Stages
                .Select(x => x.Duration)
                .DefaultIfEmpty(TimeSpan.Zero)
                .Max();
        }
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