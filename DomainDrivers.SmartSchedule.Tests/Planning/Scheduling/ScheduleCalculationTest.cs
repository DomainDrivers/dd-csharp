using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Planning.Scheduling;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Tests.Planning.Scheduling.Assertions.ScheduleAssert;

namespace DomainDrivers.SmartSchedule.Tests.Planning.Scheduling;

public class ScheduleCalculationTest
{
    private static readonly DateTime Jan1 = DateTime.Parse("2020-01-01T00:00:00.00Z");

    private static readonly TimeSlot Jan10_20 = new TimeSlot(DateTime.Parse("2020-01-10T00:00:00.00Z"),
        DateTime.Parse("2020-01-20T00:00:00.00Z"));

    private static readonly TimeSlot Jan1_1 = new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"),
        DateTime.Parse("2020-01-02T00:00:00.00Z"));

    private static readonly TimeSlot Jan3_10 = new TimeSlot(DateTime.Parse("2020-01-03T00:00:00.00Z"),
        DateTime.Parse("2020-01-10T00:00:00.00Z"));

    private static readonly TimeSlot Jan1_20 = new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"),
        DateTime.Parse("2020-01-20T00:00:00.00Z"));

    private static readonly TimeSlot Jan11_21 = new TimeSlot(DateTime.Parse("2020-01-11T00:00:00.00Z"),
        DateTime.Parse("2020-01-21T00:00:00.00Z"));

    private static readonly TimeSlot Jan1_4 = new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"),
        DateTime.Parse("2020-01-04T00:00:00.00Z"));

    private static readonly TimeSlot Jan4_14 = new TimeSlot(DateTime.Parse("2020-01-04T00:00:00.00Z"),
        DateTime.Parse("2020-01-14T00:00:00.00Z"));

    private static readonly TimeSlot Jan14_16 = new TimeSlot(DateTime.Parse("2020-01-14T00:00:00.00Z"),
        DateTime.Parse("2020-01-16T00:00:00.00Z"));

    private static readonly TimeSlot Jan1_5 = new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"),
        DateTime.Parse("2020-01-05T00:00:00.00Z"));

    private static readonly TimeSlot Dec29_Jan1 = new TimeSlot(DateTime.Parse("2019-12-29T00:00:00.00Z"),
        DateTime.Parse("2020-01-01T00:00:00.00Z"));

    private static readonly TimeSlot Jan1_11 = new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"),
        DateTime.Parse("2020-01-11T00:00:00.00Z"));

    private static readonly TimeSlot Jan5_7 = new TimeSlot(DateTime.Parse("2020-01-05T00:00:00.00Z"),
        DateTime.Parse("2020-01-07T00:00:00.00Z"));

    private static readonly TimeSlot Jan3_6 = new TimeSlot(DateTime.Parse("2020-01-03T00:00:00.00Z"),
        DateTime.Parse("2020-01-06T00:00:00.00Z"));

    [Fact]
    public void CanCalculateScheduleBasedOnTheStartDay()
    {
        //given
        var stage1 = new Stage("Stage1").OfDuration(TimeSpan.FromDays(3));
        var stage2 = new Stage("Stage2").OfDuration(TimeSpan.FromDays(10));
        var stage3 = new Stage("Stage3").OfDuration(TimeSpan.FromDays(2));
        //and
        var parallelStages = ParallelStagesList.Of(
            ParallelStages.Of(stage1),
            ParallelStages.Of(stage2),
            ParallelStages.Of(stage3));

        //when
        var schedule = Schedule.BasedOnStartDay(Jan1, parallelStages);

        //then
        AssertThat(schedule)
            .HasStage("Stage1").WithSlot(Jan1_4)
            .And()
            .HasStage("Stage2").WithSlot(Jan4_14)
            .And()
            .HasStage("Stage3").WithSlot(Jan14_16);
    }

    [Fact]
    public void ScheduleCanAdjustToDatesOfOneReferenceStage()
    {
        //given
        var stage = new Stage("S1").OfDuration(TimeSpan.FromDays(3));
        var anotherStage = new Stage("S2").OfDuration(TimeSpan.FromDays(10));
        var yetAnotherStage = new Stage("S3").OfDuration(TimeSpan.FromDays(2));
        var referenceStage = new Stage("S4-Reference").OfDuration(TimeSpan.FromDays(4));
        //and
        var parallelStages = ParallelStagesList.Of(
            ParallelStages.Of(stage),
            ParallelStages.Of(referenceStage, anotherStage),
            ParallelStages.Of(yetAnotherStage));

        //when
        var schedule = Schedule.BasedOnReferenceStageTimeSlot(referenceStage, Jan1_5, parallelStages);

        //then
        AssertThat(schedule)
            .HasStage("S1").WithSlot(Dec29_Jan1).IsBefore("S4-Reference")
            .And()
            .HasStage("S2").WithSlot(Jan1_11).StartsTogetherWith("S4-Reference")
            .And()
            .HasStage("S3").WithSlot(Jan5_7).IsAfter("S4-Reference")
            .And()
            .HasStage("S4-Reference").WithSlot(Jan1_5);
    }

    [Fact]
    public void NoScheduleIsCalculatedIfReferenceStageToAdjustToDoesNotExists()
    {
        //given
        var stage1 = new Stage("Stage1").OfDuration(TimeSpan.FromDays(3));
        var stage2 = new Stage("Stage2").OfDuration(TimeSpan.FromDays(10));
        var stage3 = new Stage("Stage3").OfDuration(TimeSpan.FromDays(2));
        var stage4 = new Stage("Stage4").OfDuration(TimeSpan.FromDays(4));
        //and
        var parallelStages = ParallelStagesList.Of(
            ParallelStages.Of(stage1),
            ParallelStages.Of(stage2, stage4),
            ParallelStages.Of(stage3));

        //when
        var schedule = Schedule.BasedOnReferenceStageTimeSlot(new Stage("Stage5"), Jan1_5, parallelStages);

        //then
        AssertThat(schedule).IsEmpty();
    }

    [Fact]
    public void CanAdjustScheduleToAvailabilityOfNeededResources()
    {
        //given
        var r1 = ResourceId.NewOne();
        var r2 = ResourceId.NewOne();
        var r3 = ResourceId.NewOne();
        //and
        var stage1 = new Stage("Stage1")
            .OfDuration(TimeSpan.FromDays(3))
            .WithChosenResourceCapabilities(r1);
        var stage2 = new Stage("Stage2")
            .OfDuration(TimeSpan.FromDays(10))
            .WithChosenResourceCapabilities(r2, r3);
        //and
        var cal1 = Calendar.WithAvailableSlots(r1, Jan1_1, Jan3_10);
        var cal2 = Calendar.WithAvailableSlots(r2, Jan1_20);
        var cal3 = Calendar.WithAvailableSlots(r3, Jan11_21);

        //when
        var schedule =
            Schedule.BasedOnChosenResourcesAvailability(Calendars.Of(cal1, cal2, cal3),
                new List<Stage> { stage1, stage2 });

        //then
        AssertThat(schedule)
            .HasStage("Stage1").WithSlot(Jan3_6)
            .And()
            .HasStage("Stage2").WithSlot(Jan10_20);
    }
}