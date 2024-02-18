using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Resource.Employee;

public interface IEmployeeDbContext
{
    public DbSet<Employee> Employees { get; }
}