using DomainDrivers.SmartSchedule.Sorter;

namespace DomainDrivers.SmartSchedule.Tests.Sorter;

public class GraphTopologicalSortTest
{
    private static readonly GraphTopologicalSort GraphTopologicalSort = new GraphTopologicalSort();

    [Fact]
    public void TestTopologicalSortWithSimpleDependencies()
    {
        //given
        var node1 = new Node("Node1");
        var node2 = new Node("Node2");
        var node3 = new Node("Node3");
        var node4 = new Node("Node4");
        node2 = node2.DependsOn(node1);
        node3 = node3.DependsOn(node1);
        node4 = node4.DependsOn(node2);

        var nodes = new Nodes(node1, node2, node3, node4);

        //when
        var sortedNodes = GraphTopologicalSort.Sort(nodes);

        //then
        Assert.Equal(3, sortedNodes.All.Count);

        Assert.Equal(1, sortedNodes.All[0].NodesCollection.Count);
        Assert.Contains(node1, sortedNodes.All[0].NodesCollection);

        Assert.Equal(2, sortedNodes.All[1].NodesCollection.Count);
        Assert.Contains(node2, sortedNodes.All[1].NodesCollection);
        Assert.Contains(node3, sortedNodes.All[1].NodesCollection);
        
        Assert.Equal(1, sortedNodes.All[2].NodesCollection.Count);
        Assert.Contains(node4, sortedNodes.All[2].NodesCollection);
    }

    [Fact]
    public void TestTopologicalSortWithLinearDependencies()
    {
        //given
        var node1 = new Node("Node1");
        var node2 = new Node("Node2");
        var node3 = new Node("Node3");
        var node4 = new Node("Node4");
        var node5 = new Node("Node5");
        node1 = node1.DependsOn(node2);
        node2 = node2.DependsOn(node3);
        node3 = node3.DependsOn(node4);
        node4 = node4.DependsOn(node5);

        var nodes = new Nodes(node1, node2, node3, node4, node5);

        //when
        var sortedNodes = GraphTopologicalSort.Sort(nodes);

        //then
        Assert.Equal(5, sortedNodes.All.Count);

        Assert.Equal(1, sortedNodes.All[0].NodesCollection.Count);
        Assert.Contains(node5, sortedNodes.All[0].NodesCollection);

        Assert.Equal(1, sortedNodes.All[1].NodesCollection.Count);
        Assert.Contains(node4, sortedNodes.All[1].NodesCollection);

        Assert.Equal(1, sortedNodes.All[2].NodesCollection.Count);
        Assert.Contains(node3, sortedNodes.All[2].NodesCollection);

        Assert.Equal(1, sortedNodes.All[3].NodesCollection.Count);
        Assert.Contains(node2, sortedNodes.All[3].NodesCollection);

        Assert.Equal(1, sortedNodes.All[4].NodesCollection.Count);
        Assert.Contains(node1, sortedNodes.All[4].NodesCollection);
    }

    [Fact]
    public void TestNodesWithoutDependencies()
    {
        //given
        var node1 = new Node("Node1");
        var node2 = new Node("Node2");
        var nodes = new Nodes(node1, node2);

        //when
        var sortedNodes = GraphTopologicalSort.Sort(nodes);

        //then
        Assert.Equal(1, sortedNodes.All.Count);
    }

    [Fact]
    public void TestCyclicDependency()
    {
        //given
        var node1 = new Node("Node1");
        var node2 = new Node("Node2");
        node2 = node2.DependsOn(node1);
        node1 = node1.DependsOn(node2); // Making it cyclic
        var nodes = new Nodes(node1, node2);

        //when
        var sortedNodes = GraphTopologicalSort.Sort(nodes);

        //then
        Assert.Empty(sortedNodes.All);
    }
}