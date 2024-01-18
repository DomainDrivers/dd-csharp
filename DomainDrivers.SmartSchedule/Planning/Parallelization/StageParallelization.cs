namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public class StageParallelization
{
    public ParallelStagesList Of(ISet<Stage> stages)
    {
        return CreateSortedNodesRecursively(stages, ParallelStagesList.Empty());
    }
    
    private ParallelStagesList CreateSortedNodesRecursively(ISet<Stage> remainingNodes, ParallelStagesList accumulatedSortedNodes)
    {
        var alreadyProcessedNodes = accumulatedSortedNodes.All
            .SelectMany(n => n.Stages)
            .ToList();
        var nodesWithoutDependencies = WithAllDependenciesPresentIn(remainingNodes, alreadyProcessedNodes);

        if (nodesWithoutDependencies.Count == 0)
        {
            return accumulatedSortedNodes;
        }

        var newSortedNodes = accumulatedSortedNodes.Add(new ParallelStages(nodesWithoutDependencies));
        var newRemainingNodes = new HashSet<Stage>(remainingNodes);
        newRemainingNodes.ExceptWith(nodesWithoutDependencies);
        return CreateSortedNodesRecursively(newRemainingNodes, newSortedNodes);
    }

    private ISet<Stage> WithAllDependenciesPresentIn(ISet<Stage> toCheck, IEnumerable<Stage> presentIn)
    {
        return toCheck
            .Where(n => n.Dependencies.All(d => presentIn.Contains(d)))
            .ToHashSet();
    }
}