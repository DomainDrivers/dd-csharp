using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Tests.Planning.Scheduling.Assertions.ScheduleAssert;

namespace DomainDrivers.SmartSchedule.Tests.Planning;

public class TimeCriticalWaterfallTest : IntegrationTest
{
    static readonly TimeSlot Jan1_5 = new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"),
        DateTime.Parse("2020-01-05T00:00:00.00Z"));

    static readonly TimeSlot Jan1_3 =
        new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"), DateTime.Parse("2020-01-03T00:00:00Z"));

    static readonly TimeSlot Jan1_4 =
        new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"), DateTime.Parse("2020-01-04T00:00:00Z"));

    private readonly PlanningFacade _projectFacade;

    public TimeCriticalWaterfallTest(IntegrationTestApp testApp) : base(testApp)
    {
        _projectFacade = Scope.ServiceProvider.GetRequiredService<PlanningFacade>();
    }

    [Fact(Skip = "not implemented yet")]
    public async Task TimeCriticalWaterfallProjectProcess()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("waterfall");

        //and
        var stageBeforeCritical = new Stage("stage1")
            .OfDuration(TimeSpan.FromDays(2));
        var criticalStage = new Stage("stage2")
            .OfDuration(Jan1_5.Duration);
        var stageAfterCritical = new Stage("stage3")
            .OfDuration(TimeSpan.FromDays(3));
        await _projectFacade.DefineProjectStages(projectId, stageBeforeCritical, criticalStage, stageAfterCritical);

        //when
        await _projectFacade.PlanCriticalStage(projectId, criticalStage, Jan1_5);

        //then
        var schedule = (await _projectFacade.Load(projectId)).Schedule;
        AssertThat(schedule)
            .HasStage("stage1").WithSlot(Jan1_3)
            .And()
            .HasStage("stage2").WithSlot(Jan1_5)
            .And()
            .HasStage("stage3").WithSlot(Jan1_4);
    }
}