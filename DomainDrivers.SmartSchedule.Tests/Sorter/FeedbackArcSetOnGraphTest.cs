using DomainDrivers.SmartSchedule.Sorter;
using NUnit.Framework.Legacy;
using Assert = Xunit.Assert;

namespace DomainDrivers.SmartSchedule.Tests.Sorter;

public class FeedbackArcSetOnGraphTest
{
    [Fact]
    public void CanFindMinimumNumberOfEdgesToRemoveToMakeTheGraphAcyclic()
    {
        //given
        var node1 = new Node<string>("1");
        var node2 = new Node<string>("2");
        var node3 = new Node<string>("3");
        var node4 = new Node<string>("4");
        node1 = node1.DependsOn(node2);
        node2 = node2.DependsOn(node3);
        node4 = node4.DependsOn(node3);
        node1 = node1.DependsOn(node4);
        node3 = node3.DependsOn(node1);
        
        //when
        var toRemove = FeedbackArcSetOnGraph.Calculate(new List<Node<string>> {node1, node2, node3, node4});
        
        //then
        CollectionAssert.AreEquivalent(new[] { new Edge(3, 1), new Edge(4, 3) }, toRemove);
    }
    
    [Fact]
    public void WhenGraphIsAcyclicThereIsNothingToRemove()
    {
        //given
        var node1 = new Node<string>("1");
        var node2 = new Node<string>("2");
        var node3 = new Node<string>("3");
        var node4 = new Node<string>("4");
        node1 = node1.DependsOn(node2);
        node2 = node2.DependsOn(node3);
        node3 = node3.DependsOn(node4);
        
        //when
        var toRemove = FeedbackArcSetOnGraph.Calculate(new List<Node<string>> {node1, node2, node3, node4});

        //then
        Assert.Empty(toRemove);
    }
}