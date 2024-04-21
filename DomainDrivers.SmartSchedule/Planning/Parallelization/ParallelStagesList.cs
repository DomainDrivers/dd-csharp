using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public record ParallelStagesList(IList<ParallelStages> All)
{
    public string Print()
    {
        return string.Join(" | ", All.Select(parallelStages => parallelStages.Print()));
    }

    public virtual bool Equals(ParallelStagesList? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return All.SequenceEqual(other.All);
    }

    public override int GetHashCode()
    {
        return All.CalculateHashCode();
    }
}