using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Planning;

public interface IPlanningDbContext
{
    public DbSet<Project> Projects { get; }
}