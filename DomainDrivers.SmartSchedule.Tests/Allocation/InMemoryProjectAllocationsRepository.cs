using DomainDrivers.SmartSchedule.Allocation;

namespace DomainDrivers.SmartSchedule.Tests.Allocation;

public class InMemoryProjectAllocationsRepository : IProjectAllocationsRepository
{
    private readonly Dictionary<ProjectAllocationsId, ProjectAllocations> _projects = new();

    public Task<IList<ProjectAllocations>> FindAllContainingDate(DateTime when)
    {
        return Task.FromResult<IList<ProjectAllocations>>(_projects.Values.Where(x => x.TimeSlot != null).ToList());
    }

    public Task<ProjectAllocations?> FindById(ProjectAllocationsId projectId)
    {
        return Task.FromResult(_projects.GetValueOrDefault(projectId));
    }

    public Task<ProjectAllocations> GetById(ProjectAllocationsId projectId)
    {
        return Task.FromResult(_projects[projectId]);
    }

    public Task<ProjectAllocations> Add(ProjectAllocations project)
    {
        _projects.Add(project.ProjectId, project);
        return Task.FromResult(project);
    }

    public Task<ProjectAllocations> Update(ProjectAllocations project)
    {
        _projects[project.ProjectId] = project;
        return Task.FromResult(project);
    }

    public Task<IList<ProjectAllocations>> FindAllById(ISet<ProjectAllocationsId> projectIds)
    {
        var projects = _projects
            .Where(x => projectIds.Contains(x.Key))
            .Select(x => x.Value)
            .ToList();
        return Task.FromResult<IList<ProjectAllocations>>(projects);
    }

    public Task<IList<ProjectAllocations>> FindAll()
    {
        return Task.FromResult<IList<ProjectAllocations>>(_projects.Values.ToList());
    }
}