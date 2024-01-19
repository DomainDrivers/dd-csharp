namespace DomainDrivers.SmartSchedule.Sorter;

public class GraphTopologicalSort<T>
{
    public SortedNodes<T> Sort(Nodes<T> nodes)
    {
        return CreateSortedNodesRecursively(nodes, SortedNodes<T>.Empty());
    }

    private SortedNodes<T> CreateSortedNodesRecursively(Nodes<T> remainingNodes, SortedNodes<T> accumulatedSortedNodes)
    {
        var alreadyProcessedNodes = accumulatedSortedNodes.All
            .SelectMany(n => n.All)
            .ToList();
        var nodesWithoutDependencies = remainingNodes.WithAllDependenciesPresentIn(alreadyProcessedNodes);

        if (nodesWithoutDependencies.All.Count == 0)
        {
            return accumulatedSortedNodes;
        }

        var newSortedNodes = accumulatedSortedNodes.Add(nodesWithoutDependencies);
        remainingNodes = remainingNodes.RemoveAll(nodesWithoutDependencies.All);
        return CreateSortedNodesRecursively(remainingNodes, newSortedNodes);
    }
}