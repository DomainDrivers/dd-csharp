using DomainDrivers.SmartSchedule.Sorter;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public static class SortedNodesToParallelizedStages
{
    public static ParallelStagesList Calculate(SortedNodes<Stage> sortedNodes)
    {
        var parallelized = sortedNodes.All
            .Select(nodes => new ParallelStages(nodes.NodesCollection
                .Where(node => node.Content != null)
                .Select(node => node.Content!)
                .ToHashSet()))
            .ToList();
        return new ParallelStagesList(parallelized);
    }
}