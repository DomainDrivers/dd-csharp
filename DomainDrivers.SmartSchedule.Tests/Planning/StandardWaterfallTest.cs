using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Shared.Capability;
using static DomainDrivers.SmartSchedule.Planning.Demand;
using static DomainDrivers.SmartSchedule.Tests.Planning.Scheduling.Assertions.ScheduleAssert;

namespace DomainDrivers.SmartSchedule.Tests.Planning;

public class StandardWaterfallTest : IntegrationTestWithSharedApp
{
    static readonly DateTime Jan1 = DateTime.Parse("2020-01-01T00:00:00.00Z");
    static readonly ResourceId Resource1 = ResourceId.NewOne();
    static readonly ResourceId Resource2 = ResourceId.NewOne();
    static readonly ResourceId Resource4 = ResourceId.NewOne();

    static readonly TimeSlot Jan1_2 =
        new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"), DateTime.Parse("2020-01-02T00:00:00Z"));

    static readonly TimeSlot Jan2_5 =
        new TimeSlot(DateTime.Parse("2020-01-02T00:00:00.00Z"), DateTime.Parse("2020-01-05T00:00:00Z"));

    static readonly TimeSlot Jan2_12 =
        new TimeSlot(DateTime.Parse("2020-01-02T00:00:00.00Z"), DateTime.Parse("2020-01-12T00:00:00Z"));

    private readonly PlanningFacade _projectFacade;

    public StandardWaterfallTest(IntegrationTestApp testApp) : base(testApp)
    {
        _projectFacade = Scope.ServiceProvider.GetRequiredService<PlanningFacade>();
    }

    [Fact(Skip = "not implemented yet")]
    public async Task WaterfallProjectProcess()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("waterfall");

        //when
        await _projectFacade.DefineProjectStages(projectId,
            new Stage("stage1"),
            new Stage("stage2"),
            new Stage("stage3")
        );

        //then
        var projectCard = await _projectFacade.Load(projectId);
        Assert.Equal("stage1, stage2, stage3", projectCard.ParallelizedStages.Print());

        //when
        var demandsPerStage = new DemandsPerStage(new Dictionary<string, Demands>
        {
            { "stage1", Demands.Of(DemandFor(Skill("java"))) }
        });
        await _projectFacade.DefineDemandsPerStage(projectId, demandsPerStage);

        //then
        VerifyRiskDuringPlanning(projectId);

        //when
        await _projectFacade.DefineProjectStages(projectId,
            new Stage("stage1")
                .WithChosenResourceCapabilities(Resource1),
            new Stage("stage2")
                .WithChosenResourceCapabilities(Resource2, Resource1),
            new Stage("stage3")
                .WithChosenResourceCapabilities(Resource4));

        //then
        projectCard = await _projectFacade.Load(projectId);
        Assert.Contains(projectCard.ParallelizedStages.Print(),
            new[] { "stage1 | stage2, stage3", "stage2, stage3 | stage1" });

        //when
        await _projectFacade.DefineProjectStages(projectId,
            new Stage("stage1")
                .OfDuration(TimeSpan.FromDays(1))
                .WithChosenResourceCapabilities(Resource1),
            new Stage("stage2")
                .OfDuration(TimeSpan.FromDays(3))
                .WithChosenResourceCapabilities(Resource2, Resource1),
            new Stage("stage3")
                .OfDuration(TimeSpan.FromDays(10))
                .WithChosenResourceCapabilities(Resource4));
        //and
        await _projectFacade.DefineStartDate(projectId, Jan1);

        //then
        var schedule = (await _projectFacade.Load(projectId)).Schedule;
        AssertThat(schedule)
            // .HasStage("stage1").WithSlot(Jan1_2)
            // .And()
            .HasStage("stage2").WithSlot(Jan2_5)
            .And()
            .HasStage("stage3").WithSlot(Jan2_12);
    }

    private void VerifyRiskDuringPlanning(ProjectId projectId)
    {
    }
}