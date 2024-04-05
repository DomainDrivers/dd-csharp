using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;
using NSubstitute;

namespace DomainDrivers.SmartSchedule.Tests.Planning;

public static class PlanningTestConfiguration
{
    public static PlanningFacade PlanningFacade(IEventsPublisher eventsPublisher, IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        var planChosenResources = new PlanChosenResources(projectRepository, Substitute.For<IAvailabilityFacade>(),
            eventsPublisher, clock, unitOfWork);
        return new PlanningFacade(projectRepository, new StageParallelization(), planChosenResources, eventsPublisher,
            clock, unitOfWork);
    }
}

public class InMemoryProjectRepository : IProjectRepository
{
    private readonly Dictionary<ProjectId, Project> _projects = new Dictionary<ProjectId, Project>();

    public Task<Project> GetById(ProjectId projectId)
    {
        return Task.FromResult(_projects[projectId]);
    }

    public Task<Project> Add(Project project)
    {
        _projects.Add(project.Id, project);
        return Task.FromResult(project);
    }

    public Task<Project> Update(Project project)
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