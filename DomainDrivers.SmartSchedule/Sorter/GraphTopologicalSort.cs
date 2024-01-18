namespace DomainDrivers.SmartSchedule.Sorter;

public class GraphTopologicalSort
{
    public SortedNodes Sort(Nodes nodes)
    {
        return CreateSortedNodesRecursively(nodes, SortedNodes.Empty());
    }

    private SortedNodes CreateSortedNodesRecursively(Nodes remainingNodes, SortedNodes accumulatedSortedNodes)
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