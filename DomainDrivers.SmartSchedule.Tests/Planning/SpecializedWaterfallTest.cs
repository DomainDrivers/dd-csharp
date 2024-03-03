using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Tests.Planning.Scheduling.Assertions.ScheduleAssert;

namespace DomainDrivers.SmartSchedule.Tests.Planning;

public class SpecializedWaterfallTest : IntegrationTestWithSharedApp
{
    static readonly TimeSlot Jan1_2 =
        new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"), DateTime.Parse("2020-01-02T00:00:00Z"));

    static readonly TimeSlot Jan1_4 =
        new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"), DateTime.Parse("2020-01-04T00:00:00Z"));

    static readonly TimeSlot Jan1_5 =
        new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"), DateTime.Parse("2020-01-05T00:00:00Z"));

    static readonly TimeSlot Jan1_6 =
        new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"), DateTime.Parse("2020-01-06T00:00:00Z"));

    static readonly TimeSlot Jan4_8 =
        new TimeSlot(DateTime.Parse("2020-01-04T00:00:00.00Z"), DateTime.Parse("2020-01-08T00:00:00Z"));

    private readonly PlanningFacade _projectFacade;

    public SpecializedWaterfallTest(IntegrationTestApp testApp) : base(testApp)
    {
        _projectFacade = Scope.ServiceProvider.GetRequiredService<PlanningFacade>();
    }

    [Fact(Skip = "not implemented yet")]
    public async Task SpecializedWaterfallProjectProcess()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("waterfall");

        //and
        var criticalStageDuration = TimeSpan.FromDays(5);
        var stage1Duration = TimeSpan.FromDays(1);
        var stageBeforeCritical = new Stage("stage1").OfDuration(stage1Duration);
        var criticalStage = new Stage("stage2").OfDuration(criticalStageDuration);
        var stageAfterCritical = new Stage("stage3").OfDuration(TimeSpan.FromDays(3));
        await _projectFacade.DefineProjectStages(projectId, stageBeforeCritical, criticalStage, stageAfterCritical);

        //and
        var criticalResourceName = ResourceId.NewOne();
        var criticalCapabilityAvailability =
            ResourceAvailableForCapabilityInPeriod(criticalResourceName, Capability.Skill("JAVA"), Jan1_6);

        //when
        await _projectFacade.PlanCriticalStageWithResource(projectId, criticalStage, criticalResourceName, Jan4_8);

        //then
        VerifyResourcesNotAvailable(projectId, criticalCapabilityAvailability, Jan4_8);

        //when
        await _projectFacade.PlanCriticalStageWithResource(projectId, criticalStage, criticalResourceName, Jan1_6);

        //then
        AssertResourcesAvailable(projectId, criticalCapabilityAvailability);

        //and
        var schedule = (await _projectFacade.Load(projectId)).Schedule;
        AssertThat(schedule)
            .HasStage("stage1").WithSlot(Jan1_2)
            .And()
            .HasStage("stage2").WithSlot(Jan1_6)
            .And()
            .HasStage("stage3").WithSlot(Jan1_4);
    }

    private void AssertResourcesAvailable(ProjectId projectId, ResourceId resource)
    {
    }

    private void VerifyResourcesNotAvailable(ProjectId projectId, ResourceId resource,
        TimeSlot requestedButNotAvailable)
    {
    }

    private ResourceId ResourceAvailableForCapabilityInPeriod(ResourceId resource, Capability capability,
        TimeSlot slot)
    {
        return null!;
    }
}