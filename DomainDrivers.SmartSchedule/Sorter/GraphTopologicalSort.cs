namespace DomainDrivers.SmartSchedule.Sorter;

public static class GraphTopologicalSort<T>
{
    private static readonly Func<Nodes<T>, SortedNodes<T>, SortedNodes<T>> CreateSortedNodesRecursively =
        (remainingNodes, accumulatedNodes) =>
            IntermediateSortedNodesCreator<T>.CreateSortedNodesRecursively(remainingNodes, accumulatedNodes);

    public static SortedNodes<T> Sort(Nodes<T> nodes)
    {
        return CreateSortedNodesRecursively.Invoke(nodes, SortedNodes<T>.Empty());
    }
}

public static class IntermediateSortedNodesCreator<T>
{
    public static SortedNodes<T> CreateSortedNodesRecursively(Nodes<T> remainingNodes,
        SortedNodes<T> accumulatedSortedNodes)
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