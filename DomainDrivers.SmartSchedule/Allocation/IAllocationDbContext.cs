using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation;

public interface IAllocationDbContext
{
    public DbSet<ProjectAllocations> ProjectAllocations { get; }
}