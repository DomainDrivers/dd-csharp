using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;
using NSubstitute;

namespace DomainDrivers.SmartSchedule.Tests.Planning;

public static class PlanningTestConfiguration
{
    public static PlanningFacade PlanningFacadeWithInMemoryDb(IEventsPublisher eventsPublisher)
    {
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        var projectRepository = new InMemoryProjectRepository();
        var planChosenResources = new PlanChosenResources(projectRepository, Substitute.For<IAvailabilityFacade>(),
            eventsPublisher, clock);
        return new PlanningFacade(projectRepository, new StageParallelization(), planChosenResources, eventsPublisher,
            clock);
    }
}

public class InMemoryProjectRepository : IProjectRepository
{
    private readonly Dictionary<ProjectId, Project> _projects = new Dictionary<ProjectId, Project>();

    public Task<Project> GetById(ProjectId projectId)
    {
        return Task.FromResult(_projects[projectId]);
    }

    public Task<Project> Save(Project project)
    {
        _projects[project.Id] = project;
        return Task.FromResult(project);
    }

    public Task<IList<Project>> FindAllByIdIn(ISet<ProjectId> projectIds)
    {
        var projects = _projects
            .Where(x => projectIds.Contains(x.Key))
            .Select(x => x.Value)
            .ToList();
        return Task.FromResult<IList<Project>>(projects);
    }

    public Task<IList<Project>> FindAll()
    {
        return Task.FromResult<IList<Project>>(_projects.Values.ToList());
    }
}