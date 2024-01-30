using DomainDrivers.SmartSchedule.Planning.Parallelization;

namespace DomainDrivers.SmartSchedule.Tests.Planning.Parallelization;

public class DurationCalculatorTest
{
    [Fact]
    public void LongestStageIsTakenIntoAccount()
    {
        //given
        var stage1 = new Stage("Stage1").OfDuration(TimeSpan.Zero);
        var stage2 = new Stage("Stage2").OfDuration(TimeSpan.FromDays(3));
        var stage3 = new Stage("Stage3").OfDuration(TimeSpan.FromDays(2));
        var stage4 = new Stage("Stage4").OfDuration(TimeSpan.FromDays(5));

        //when
        var duration = DurationCalculator.Calculate(new List<Stage> { stage1, stage2, stage3, stage4 });

        //then
        Assert.Equal(5, duration.Days);
    }

    [Fact]
    public void SumIsTakenIntoAccountWhenNothingIsParallel()
    {
        //given
        var stage1 = new Stage("Stage1").OfDuration(TimeSpan.FromHours(10));
        var stage2 = new Stage("Stage2").OfDuration(TimeSpan.FromHours(24));
        var stage3 = new Stage("Stage3").OfDuration(TimeSpan.FromDays(2));
        var stage4 = new Stage("Stage4").OfDuration(TimeSpan.FromDays(1));
        stage4.DependsOn(stage3);
        stage3.DependsOn(stage2);
        stage2.DependsOn(stage1);

        //when
        var duration = DurationCalculator.Calculate(new List<Stage> { stage1, stage2, stage3, stage4 });

        //then
        Assert.Equal(106, duration.TotalHours);
    }
}