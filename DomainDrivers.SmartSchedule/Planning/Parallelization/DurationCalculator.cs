namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public static class DurationCalculator
{
    public static TimeSpan Calculate(IList<Stage> stages)
    {
        var parallelizedStages = new StageParallelization().Of(new HashSet<Stage>(stages));
        var durations = stages.ToDictionary(identity => identity, stage => stage.Duration);
        return parallelizedStages
            .AllSorted()
            .Aggregate(TimeSpan.Zero, (current, parallelStages) =>
                current.Add(parallelStages.Stages
                    .Select(stage => durations[stage])
                    .DefaultIfEmpty(TimeSpan.Zero)
                    .Max()));
    }
}