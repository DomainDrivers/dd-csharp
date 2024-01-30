using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Planning.Parallelization;

public class ParallelizationTest
{
    private static readonly StageParallelization StageParallelization = new StageParallelization();
    private static readonly ResourceName Leon = new ResourceName("Leon");
    private static readonly ResourceName Eryk = new ResourceName("Eric");
    private static readonly ResourceName Slawek = new ResourceName("Sławek");
    private static readonly ResourceName Kuba = new ResourceName("Kuba");

    [Fact]
    public void EverythingCanBeDoneInParallelWhenThereAreNoDependencies()
    {
        //given
        var stage1 = new Stage("Stage1");
        var stage2 = new Stage("Stage2");

        //when
        var sortedStages = StageParallelization.Of(new HashSet<Stage>() { stage1, stage2 });

        //then
        Assert.Equal(1, sortedStages.All.Count);
    }

    [Fact]
    public void TestSimpleDependencies()
    {
        //given
        var stage1 = new Stage("Stage1");
        var stage2 = new Stage("Stage2");
        var stage3 = new Stage("Stage3");
        var stage4 = new Stage("Stage4");
        stage2 = stage2.DependsOn(stage1);
        stage3 = stage3.DependsOn(stage1);
        stage4 = stage4.DependsOn(stage2);

        //when
        var sortedStages = StageParallelization.Of(new HashSet<Stage> { stage1, stage2, stage3, stage4 });

        //then
        Assert.Equal("Stage1 | Stage2, Stage3 | Stage4", sortedStages.Print());
    }

    [Fact]
    public void CantBeDoneWhenThereIsACycle()
    {
        //given
        var stage1 = new Stage("Stage1");
        var stage2 = new Stage("Stage2");
        stage2 = stage2.DependsOn(stage1);
        stage1 = stage1.DependsOn(stage2); // making it cyclic

        //when
        var sortedStages = StageParallelization.Of(new HashSet<Stage> { stage1, stage2 });

        //then
        Assert.Empty(sortedStages.All);
    }

    [Fact]
    public void TakesIntoAccountSharedResources()
    {
        //given
        var stage1 = new Stage("Stage1")
            .WithChosenResourceCapabilities(Leon);
        var stage2 = new Stage("Stage2")
            .WithChosenResourceCapabilities(Eryk, Leon);
        var stage3 = new Stage("Stage3")
            .WithChosenResourceCapabilities(Slawek);
        var stage4 = new Stage("Stage4")
            .WithChosenResourceCapabilities(Slawek, Kuba);

        //when
        var parallelStages = StageParallelization.Of(new HashSet<Stage> { stage1, stage2, stage3, stage4 });

        //then 
        Assert.Contains(parallelStages.Print(), new[]
        {
            "Stage1, Stage3 | Stage2, Stage4",
            "Stage2, Stage4 | Stage1, Stage3"
        });
    }
}