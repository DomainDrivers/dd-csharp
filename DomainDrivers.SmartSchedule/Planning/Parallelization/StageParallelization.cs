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
}