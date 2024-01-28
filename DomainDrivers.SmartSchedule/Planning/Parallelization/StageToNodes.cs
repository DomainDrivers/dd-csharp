using DomainDrivers.SmartSchedule.Sorter;

namespace DomainDrivers.SmartSchedule.Planning.Parallelization;

public static class StagesToNodes
{
    public static Nodes<Stage> Calculate(IList<Stage> stages)
    {
        IDictionary<string, Node<Stage>> result = stages.ToDictionary(stage => stage.Name, stage => new Node<Stage>(stage.Name, stage));

        for (var i = 0; i < stages.Count; i++)
        {
            var stage = stages[i];
            result = ExplicitDependencies(stage, result);
            result = SharedResources(stage, stages.Skip(i + 1).ToList(), result);
        }

        return new Nodes<Stage>(new HashSet<Node<Stage>>(result.Values));
    }

    private static IDictionary<string, Node<Stage>> SharedResources(Stage stage, IList<Stage> with, IDictionary<string, Node<Stage>> result)
    {
        foreach (var other in with)
        {
            if (stage.Name != other.Name)
            {
                if (stage.Resources.Intersect(other.Resources).Any())
                {
                    if (other.Resources.Count > stage.Resources.Count)
                    {
                        var node = result[stage.Name];
                        node = node.DependsOn(result[other.Name]);
                        result[stage.Name] = node;
                    }
                    else
                    {
                        var node = result[other.Name];
                        node = node.DependsOn(result[stage.Name]);
                        result[other.Name] = node;
                    }
                }
            }
        }
        return result;
    }

    private static IDictionary<string, Node<Stage>> ExplicitDependencies(Stage stage, IDictionary<string, Node<Stage>> result)
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
