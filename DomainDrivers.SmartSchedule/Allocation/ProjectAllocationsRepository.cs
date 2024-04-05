using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation;

public class ProjectAllocationsRepository
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
}