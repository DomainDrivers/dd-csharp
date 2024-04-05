using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation;

public class ProjectAllocationsRepository : IProjectAllocationsRepository
{
    private readonly IAllocationDbContext _allocationDbContext;

    public ProjectAllocationsRepository(IAllocationDbContext allocationDbContext)
    {
        _allocationDbContext = allocationDbContext;
    }

    public async Task<IList<ProjectAllocations>> FindAllContainingDate(DateTime when)
    {
        return await _allocationDbContext.ProjectAllocations.FromSql(
                $"SELECT * FROM project_allocations WHERE from_date <= {when} AND to_date > {when}")
            .ToListAsync();
    }

    public async Task<ProjectAllocations?> FindById(ProjectAllocationsId projectId)
    {
        return await _allocationDbContext.ProjectAllocations
            .SingleOrDefaultAsync(x => x.ProjectId == projectId);
    }

    public async Task<ProjectAllocations> GetById(ProjectAllocationsId projectId)
    {
        return await _allocationDbContext.ProjectAllocations
            .SingleAsync(x => x.ProjectId == projectId);
    }

    public async Task<ProjectAllocations> Add(ProjectAllocations project)
    {
        return (await _allocationDbContext.ProjectAllocations.AddAsync(project)).Entity;
    }

    public Task<ProjectAllocations> Update(ProjectAllocations project)
    {
        return Task.FromResult(_allocationDbContext.ProjectAllocations.Update(project).Entity);
    }

    public async Task<IList<ProjectAllocations>> FindAllById(ISet<ProjectAllocationsId> projectIds)
    {
        //ToArray cast is needed for query to be compiled properly
        return await _allocationDbContext.ProjectAllocations
            .Where(x => projectIds.ToArray().Contains(x.ProjectId))
            .ToListAsync();
    }

    public async Task<IList<ProjectAllocations>> FindAll()
    {
        return await _allocationDbContext.ProjectAllocations.ToListAsync();
    }
}