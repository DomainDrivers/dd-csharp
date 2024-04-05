using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Planning;

public class ProjectRepository : IProjectRepository
{
    private readonly IPlanningDbContext _planningDbContext;

    public ProjectRepository(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;
    }

    public async Task<Project> GetById(ProjectId projectId)
    {
        return await _planningDbContext.Projects
            .SingleAsync(x => x.Id == projectId);
    }

    public async Task<Project> Add(Project project)
    {
        return (await _planningDbContext.Projects.AddAsync(project)).Entity;
    }

    public Task<Project> Update(Project project)
    {
        return Task.FromResult(_planningDbContext.Projects.Update(project).Entity);
    }

    public async Task<IList<Project>> FindAllByIdIn(ISet<ProjectId> projectIds)
    {
        //ToArray cast is needed for query to be compiled properly
        return await _planningDbContext.Projects
            .Where(x => projectIds.ToArray().Contains(x.Id))
            .ToListAsync();
    }

    public async Task<IList<Project>> FindAll()
    {
        return await _planningDbContext.Projects.ToListAsync();
    }
}