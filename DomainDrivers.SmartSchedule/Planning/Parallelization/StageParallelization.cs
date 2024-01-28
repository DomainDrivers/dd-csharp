using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Sorter;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public class StageParallelization
{
    private static readonly Func<IList<Stage>, Nodes<Stage>> CreateNodes = stages => StagesToNodes.Calculate(stages);

    private static readonly Func<Nodes<Stage>, SortedNodes<Stage>> GraphSort = nodes =>
        GraphTopologicalSort<Stage>.Sort(nodes);

    private static readonly Func<SortedNodes<Stage>, ParallelStagesList> Parallelize = nodes =>
        SortedNodesToParallelizedStages.Calculate(nodes);

    private static readonly Func<IList<Stage>, ParallelStagesList> Workflow = CreateNodes.AndThen(GraphSort).AndThen(Parallelize);
       
    public ParallelStagesList Of(ISet<Stage> stages)
    {
        return Workflow.Invoke(new List<Stage>(stages));
    }
}