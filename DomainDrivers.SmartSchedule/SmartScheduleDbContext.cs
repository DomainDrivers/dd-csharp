using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Resource.Device;
using DomainDrivers.SmartSchedule.Resource.Employee;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule;

public class SmartScheduleDbContext : DbContext, IPlanningDbContext, IAllocationDbContext,
    ICashflowDbContext, IEmployeeDbContext, IDeviceDbContext
{
    public SmartScheduleDbContext(DbContextOptions<SmartScheduleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; init; } = null!;
    public DbSet<ProjectAllocations> ProjectAllocations { get; set; } = null!;
    public DbSet<Cashflow> Cashflows { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartScheduleDbContext).Assembly);
    }
}