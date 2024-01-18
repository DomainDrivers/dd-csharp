using DomainDrivers.SmartSchedule.Sorter;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public class SortedNodesToParallelizedStages
{
    public ParallelStagesList Calculate(SortedNodes sortedNodes)
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