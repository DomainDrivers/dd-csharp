using DomainDrivers.SmartSchedule.Sorter;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public class StagesToNodes
{
    public Nodes Calculate(IList<Stage> stages)
    {
        IDictionary<string, Node> result = stages.ToDictionary(stage => stage.Name, stage => new Node(stage.Name, stage));

        for (var i = 0; i < stages.Count; i++)
        {
            var stage = stages[i];
            result = ExplicitDependencies(stage, result);
        }

        return new Nodes(new HashSet<Node>(result.Values));
    }

    private IDictionary<string, Node> ExplicitDependencies(Stage stage, IDictionary<string, Node> result)
    {
        var nodeWithExplicitDeps = result[stage.Name];
        foreach (var explicitDependency in stage.Dependencies)
        {
            nodeWithExplicitDeps = nodeWithExplicitDeps.DependsOn(result[explicitDependency.Name]);
        }
        result[stage.Name] = nodeWithExplicitDeps;
        return result;
    }
}
