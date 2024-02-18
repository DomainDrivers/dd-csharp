using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Sorter;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public class StageParallelization
{
    public ParallelStagesList Of(ISet<Stage> stages)
    {
        var nodes = new StagesToNodes().Calculate(new List<Stage>(stages));
        var sortedNodes = new GraphTopologicalSort<Stage>().Sort(nodes);
        return new SortedNodesToParallelizedStages().Calculate(sortedNodes);
    }

    public RemovalSuggestion WhatToRemove(ISet<Stage> stages)
    {
        var nodes = new StagesToNodes().Calculate(new List<Stage>(stages));
        var result = new FeedbackArcSetOnGraph<Stage>().Calculate(new List<Node<Stage>>(nodes.NodesCollection));
        return new RemovalSuggestion(result);
    }
}

public record RemovalSuggestion(IList<Edge> Edges)
{
    public override int GetHashCode()
    {
        return Edges.CalculateHashCode();
    }

    public virtual bool Equals(RemovalSuggestion? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Edges.SequenceEqual(other.Edges);
    }

    public override string ToString()
    {
        return Edges.ToCollectionString();
    }
}