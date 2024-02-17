using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Planning.Scheduling;
using DomainDrivers.SmartSchedule.Shared;
using NUnit.Framework.Legacy;
using Demands = DomainDrivers.SmartSchedule.Planning.Demands;
using static DomainDrivers.SmartSchedule.Planning.Demand;
using static DomainDrivers.SmartSchedule.Shared.Capability;

namespace DomainDrivers.SmartSchedule.Tests.Planning;

public class PlanningFacadeTest : IntegrationTest
{
    private readonly PlanningFacade _projectFacade;

    public PlanningFacadeTest(IntegrationTestApp testApp) : base(testApp)
    {
        _projectFacade = Scope.ServiceProvider.GetRequiredService<PlanningFacade>();
    }

    [Fact]
    public async Task CanCreateProjectAndLoadProjectCard()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("project", new Stage("Stage1"));

        //when
        var loaded = await _projectFacade.Load(projectId);

        //then
        Assert.Equal(projectId, loaded.ProjectId);
        Assert.Equal("project", loaded.Name);
        Assert.Equal("Stage1", loaded.ParallelizedStages.Print());
    }

    [Fact]
    public async Task CanLoadMultipleProjects()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("project", new Stage("Stage1"));
        var projectId2 = await _projectFacade.AddNewProject("project2", new Stage("Stage2"));

        //when
        var loaded = _projectFacade.LoadAll(new HashSet<ProjectId> { projectId, projectId2 });

        //then
        CollectionAssert.AreEquivalent(new[] { projectId, projectId2 }, loaded.Select(c => c.ProjectId));
    }

    [Fact]
    public async Task CanCreateAndSaveMoreComplexParallelization()
    {
        //given
        var stage1 = new Stage("Stage1");
        var stage2 = new Stage("Stage2");
        var stage3 = new Stage("Stage3");
        stage2 = stage2.DependsOn(stage1);
        stage3 = stage3.DependsOn(stage2);

        //and
        var projectId = await _projectFacade.AddNewProject("project", stage1, stage2, stage3);

        //when
        var loaded = await _projectFacade.Load(projectId);

        //then
        Assert.Equal("Stage1 | Stage2 | Stage3", loaded.ParallelizedStages.Print());
    }

    [Fact]
    public async Task CanPlanDemands()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("project", new Stage("Stage1"));

        //when
        var demandForJava = Demands.Of(DemandFor(Skill("JAVA")));
        await _projectFacade.AddDemands(projectId, demandForJava);

        //then
        var loaded = await _projectFacade.Load(projectId);
        Assert.Equal(demandForJava, loaded.Demands);
    }

    [Fact]
    public async Task CanPlanNewDemands()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("project", new Stage("Stage1"));

        //when
        var java = DemandFor(Skill("JAVA"));
        var csharp = DemandFor(Skill("C#"));
        await _projectFacade.AddDemands(projectId, Demands.Of(java));
        await _projectFacade.AddDemands(projectId, Demands.Of(csharp));

        //then
        var loaded = await _projectFacade.Load(projectId);
        Assert.Equal(Demands.Of(java, csharp), loaded.Demands);
    }

    [Fact]
    public async Task CanPlanDemandsPerStage()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("project", new Stage("Stage1"));

        //when
        var java = Demands.Of(DemandFor(Skill("JAVA")));
        var demandsPerStage = new DemandsPerStage(new Dictionary<string, Demands> { { "Stage1", java } });
        await _projectFacade.DefineDemandsPerStage(projectId, demandsPerStage);

        //then
        var loaded = await _projectFacade.Load(projectId);
        Assert.Equal(demandsPerStage, loaded.DemandsPerStage);
        Assert.Equal(java, loaded.Demands);
    }

    [Fact]
    public async Task CanPlanNeededResourcesInTime()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("project");

        //when
        var neededResources = new HashSet<ResourceId> { ResourceId.NewOne() };
        var firstHalfOfTheYear = new TimeSlot(DateTime.Parse("2021-01-01T00:00:00.00Z"),
            DateTime.Parse("2021-06-01T00:00:00.00Z"));
        await _projectFacade.DefineResourcesWithinDates(projectId, neededResources, firstHalfOfTheYear);

        //then
        var loaded = await _projectFacade.Load(projectId);
        Assert.Equal(new ChosenResources(neededResources, firstHalfOfTheYear), loaded.NeededResources);
    }

    [Fact]
    public async Task CanRedefineStages()
    {
        //given
        var projectId = await _projectFacade.AddNewProject("project", new Stage("Stage1"));

        //when
        await _projectFacade.DefineProjectStages(projectId, new Stage("Stage2"));

        //then
        var loaded = await _projectFacade.Load(projectId);
        Assert.Equal("Stage2", loaded.ParallelizedStages.Print());
    }

    [Fact]
    public async Task CanCalculateScheduleAfterPassingPossibleStart()
    {
        //given
        var stage1 = new Stage("Stage1").OfDuration(TimeSpan.FromDays(2));
        var stage2 = new Stage("Stage2").OfDuration(TimeSpan.FromDays(5));
        var stage3 = new Stage("Stage3").OfDuration(TimeSpan.FromDays(7));

        //and
        var projectId = await _projectFacade.AddNewProject("project", stage1, stage2, stage3);

        //when
        await _projectFacade.DefineStartDate(projectId, DateTime.Parse("2021-01-01T00:00:00.00Z"));

        //then
        var expectedSchedule = new Dictionary<string, TimeSlot>
        {
            {
                "Stage1",
                new TimeSlot(DateTime.Parse("2021-01-01T00:00:00.00Z"), DateTime.Parse("2021-01-03T00:00:00.00Z"))
            },
            {
                "Stage2",
                new TimeSlot(DateTime.Parse("2021-01-01T00:00:00.00Z"), DateTime.Parse("2021-01-06T00:00:00.00Z"))
            },
            {
                "Stage3",
                new TimeSlot(DateTime.Parse("2021-01-01T00:00:00.00Z"), DateTime.Parse("2021-01-08T00:00:00.00Z"))
            }
        };
        var loaded = await _projectFacade.Load(projectId);
        CollectionAssert.AreEquivalent(expectedSchedule, loaded.Schedule.Dates);
    }

    [Fact]
    public async Task CanManuallyAddSchedule()
    {
        //given
        var stage1 = new Stage("Stage1").OfDuration(TimeSpan.FromDays(2));
        var stage2 = new Stage("Stage2").OfDuration(TimeSpan.FromDays(5));
        var stage3 = new Stage("Stage3").OfDuration(TimeSpan.FromDays(7));

        //and
        var projectId = await _projectFacade.AddNewProject("project", stage1, stage2, stage3);

        //when
        var dates = new Dictionary<string, TimeSlot>
        {
            {
                "Stage1",
                new TimeSlot(DateTime.Parse("2021-01-01T00:00:00.00Z"), DateTime.Parse("2021-01-03T00:00:00.00Z"))
            },
            {
                "Stage2",
                new TimeSlot(DateTime.Parse("2021-01-03T00:00:00.00Z"), DateTime.Parse("2021-01-08T00:00:00.00Z"))
            },
            {
                "Stage3",
                new TimeSlot(DateTime.Parse("2021-01-08T00:00:00.00Z"), DateTime.Parse("2021-01-15T00:00:00.00Z"))
            }
        };
        await _projectFacade.DefineManualSchedule(projectId, new Schedule(dates));

        //then
        var loaded = await _projectFacade.Load(projectId);
        CollectionAssert.AreEquivalent(dates, loaded.Schedule.Dates);
    }
}