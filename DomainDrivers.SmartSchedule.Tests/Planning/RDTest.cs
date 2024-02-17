using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Tests.Planning.Scheduling.Assertions.ScheduleAssert;

namespace DomainDrivers.SmartSchedule.Tests.Planning;

public class RDTest : IntegrationTest
{
    static readonly TimeSlot January = new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"),
        DateTime.Parse("2020-01-31T00:00:00.00Z"));

    static readonly TimeSlot February = new TimeSlot(DateTime.Parse("2020-02-01T00:00:00.00Z"),
        DateTime.Parse("2020-02-28T00:00:00.00Z"));

    static readonly TimeSlot March = new TimeSlot(DateTime.Parse("2020-03-01T00:00:00.00Z"),
        DateTime.Parse("2020-03-31T00:00:00.00Z"));

    static readonly TimeSlot Q1 = new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"),
        DateTime.Parse("2020-03-31T00:00:00.00Z"));

    static readonly TimeSlot Jan1_4 = new TimeSlot(DateTime.Parse("2020-01-01T00:00:00.00Z"),
        DateTime.Parse("2020-01-04T00:00:00.00Z"));

    static readonly TimeSlot Feb2_16 = new TimeSlot(DateTime.Parse("2020-02-01T00:00:00.00Z"),
        DateTime.Parse("2020-02-16T00:00:00.00Z"));

    static readonly TimeSlot Mar1_6 = new TimeSlot(DateTime.Parse("2020-03-01T00:00:00.00Z"),
        DateTime.Parse("2020-03-06T00:00:00.00Z"));

    private readonly PlanningFacade _projectFacade;
    private readonly AvailabilityFacade _availabilityFacade;

    public RDTest(IntegrationTestApp testApp) : base(testApp)
    {
        _projectFacade = Scope.ServiceProvider.GetRequiredService<PlanningFacade>();
        _availabilityFacade = Scope.ServiceProvider.GetRequiredService<AvailabilityFacade>();
    }

    [Fact]
    public async Task ResearchAndDevelopmentProjectProcess()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("waterfall");
        //and
        var r1 = ResourceId.NewOne();
        var javaAvailableInJanuary = await ResourceAvailableForCapabilityInPeriod(r1, Capability.Skill("JAVA"), January);
        var r2 = ResourceId.NewOne();
        var phpAvailableInFebruary = await ResourceAvailableForCapabilityInPeriod(r2, Capability.Skill("PHP"), February);
        var r3 = ResourceId.NewOne();
        var csharpAvailableInMarch = await ResourceAvailableForCapabilityInPeriod(r3, Capability.Skill("CSHARP"), March);
        var allResources = new HashSet<ResourceId> { r1, r2, r3 };

        //when
        await _projectFacade.DefineResourcesWithinDates(projectId, allResources, January);

        //then
        VerifyThatResourcesAreMissing(projectId,
            new HashSet<ResourceId> { phpAvailableInFebruary, csharpAvailableInMarch });

        //when
        await _projectFacade.DefineResourcesWithinDates(projectId, allResources, February);

        //then
        VerifyThatResourcesAreMissing(projectId,
            new HashSet<ResourceId> { javaAvailableInJanuary, csharpAvailableInMarch });

        //when
        await _projectFacade.DefineResourcesWithinDates(projectId, allResources, Q1);

        //then
        VerifyThatNoResourcesAreMissing(projectId);

        //when
        await _projectFacade.AdjustStagesToResourceAvailability(projectId, Q1,
            new Stage("Stage1")
                .OfDuration(TimeSpan.FromDays(3))
                .WithChosenResourceCapabilities(r1),
            new Stage("Stage2")
                .OfDuration(TimeSpan.FromDays(15))
                .WithChosenResourceCapabilities(r2),
            new Stage("Stage3")
                .OfDuration(TimeSpan.FromDays(5))
                .WithChosenResourceCapabilities(r3));

        //then
        var loaded = await _projectFacade.Load(projectId);
        var schedule = (await _projectFacade.Load(projectId)).Schedule;

        AssertThat(schedule)
            .HasStage("Stage1").WithSlot(Jan1_4)
            .And()
            .HasStage("Stage2").WithSlot(Feb2_16)
            .And()
            .HasStage("Stage3").WithSlot(Mar1_6);
        ProjectIsNotParallelized(loaded);
    }

    private async Task<ResourceId> ResourceAvailableForCapabilityInPeriod(ResourceId resource, Capability capability,
        TimeSlot slot)
    {
        await _availabilityFacade.CreateResourceSlots(resource, slot);
        return ResourceId.NewOne();
    }

    private void ProjectIsNotParallelized(ProjectCard loaded)
    {
        Assert.Equal(ParallelStagesList.Empty(), loaded.ParallelizedStages);
    }

    private void VerifyThatNoResourcesAreMissing(ProjectId projectId)
    {
    }

    private void VerifyThatResourcesAreMissing(ProjectId projectId, HashSet<ResourceId> missingResources)
    {
    }
}