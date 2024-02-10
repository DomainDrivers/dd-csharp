using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Planning;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule;

public class SmartScheduleDbContext : DbContext, IPlanningDbContext, IAllocationDbContext
{
    public SmartScheduleDbContext(DbContextOptions<SmartScheduleDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Project> Projects { get; init; } = null!;
    public DbSet<ProjectAllocations> ProjectAllocations { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartScheduleDbContext).Assembly);
    }
}