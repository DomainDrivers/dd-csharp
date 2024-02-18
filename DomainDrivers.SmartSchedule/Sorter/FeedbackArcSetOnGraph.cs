namespace DomainDrivers.SmartSchedule.Sorter;

public class FeedbackArcSetOnGraph<T> where T : class
{
    public IList<Edge> Calculate(IList<Node<T>> initialNodes)
    {
        var adjacencyList = CreateAdjacencyList(initialNodes);
        var v = adjacencyList.Count;
        var feedbackEdges = new List<Edge>();
        var visited = new int[v + 1];
        
        foreach (var i in adjacencyList.Keys)
        {
            var neighbours = adjacencyList[i];
            if (neighbours.Count != 0)
            {
                visited[i] = 1;
                foreach (var j in neighbours)
                {
                    if (visited[j] == 1)
                    {
                        feedbackEdges.Add(new Edge(i, j));
                    }
                    else
                    {
                        visited[j] = 1;
                    }
                }
            }
        }

        return feedbackEdges;
    }

    private static Dictionary<int, IList<int>> CreateAdjacencyList(IList<Node<T>> initialNodes)
    {
        var adjacencyList = new Dictionary<int, IList<int>>();

        for (var i = 1; i <= initialNodes.Count; i++)
        {
            adjacencyList[i] = new List<int>();
        }

        for (var i = 0; i < initialNodes.Count; i++)
        {
            var dependencies = initialNodes[i].Dependencies.All
                .Select(dependency => initialNodes.IndexOf(dependency) + 1)
                .ToList();
            adjacencyList[i + 1] = dependencies;
        }

        return adjacencyList;
    }
}

