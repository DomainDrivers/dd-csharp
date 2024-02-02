using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public record ParallelStagesList(IList<ParallelStages> All)
{
    public static ParallelStagesList Empty()
    {
        return new ParallelStagesList(new List<ParallelStages>());
    }
    
    public static ParallelStagesList Of(params ParallelStages[] stages)
    {
        return new ParallelStagesList(stages.ToList());
    }

    public string Print()
    {
        return string.Join(" | ", All.Select(parallelStages => parallelStages.Print()));
    }

    public ParallelStagesList Add(ParallelStages newParallelStages)
    {
        var result = new List<ParallelStages>(All) { newParallelStages };
        return new ParallelStagesList(result);
    }

    public IList<ParallelStages> AllSorted(IComparer<ParallelStages> comparing)
    {
        return All
            .Order(comparing)
            .ToList();
    }

    public IList<ParallelStages> AllSorted()
    {
        return AllSorted(Comparer<ParallelStages>.Create((x, y) => x.Print().CompareTo(y.Print())));
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