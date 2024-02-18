using DomainDrivers.SmartSchedule.Planning.Parallelization;

namespace DomainDrivers.SmartSchedule.Tests.Planning.Parallelization;

public class DependencyRemovalSuggesting
{
    private static readonly StageParallelization StageParallelization = new StageParallelization();

    [Fact]
    public void SuggestingBreaksTheCycleInSchedule()
    {
        //given
        var stage1 = new Stage("Stage1");
        var stage2 = new Stage("Stage2");
        var stage3 = new Stage("Stage3");
        var stage4 = new Stage("Stage4");
        stage1 = stage1.DependsOn(stage2);
        stage2 = stage2.DependsOn(stage3);
        stage4 = stage4.DependsOn(stage3);
        stage1 = stage1.DependsOn(stage4);
        stage3 = stage3.DependsOn(stage1);

        //when
        var suggestion =
            StageParallelization.WhatToRemove(new HashSet<Stage>() { stage1, stage2, stage3, stage4 });

        //then
        Assert.Equal("[(3 -> 1), (4 -> 3)]", suggestion.ToString());
    }
}