using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Planning;

public class PlanningFacadeIntegrationTest : IntegrationTestWithSharedApp
{
    private readonly PlanningFacade _projectFacade;
    private readonly IEventsPublisher _eventsPublisher;

    public PlanningFacadeIntegrationTest(IntegrationTestApp testFixture) : base(testFixture)
    {
        _projectFacade = Scope.ServiceProvider.GetRequiredService<PlanningFacade>();
        _eventsPublisher = Scope.ServiceProvider.GetRequiredService<IEventsPublisher>();
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
}