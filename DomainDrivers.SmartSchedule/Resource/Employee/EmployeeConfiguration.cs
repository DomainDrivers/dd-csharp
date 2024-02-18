using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivers.SmartSchedule.Resource.Employee;

public static class EmployeeConfiguration
{
    public static IServiceCollection AddEmployee(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IEmployeeDbContext>(
            sp => sp.GetRequiredService<SmartScheduleDbContext>());
        serviceCollection.AddTransient<EmployeeRepository>();
        serviceCollection.AddTransient<EmployeeFacade>();
        return serviceCollection;
    }
}

public class EmployeeEntityTypeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.Property<EmployeeId>(x => x.Id)
            .HasConversion(
                projectId => projectId.Id,
                projectId => new EmployeeId(projectId))
            .HasColumnName("employee_id");
        builder.HasKey(x => x.Id);

        builder.Property("_version")
            .HasColumnName("version")
            .IsConcurrencyToken();

        builder.Property(p => p.Name)
            .HasColumnName("name");

        builder.Property(p => p.LastName)
            .HasColumnName("last_name");

        builder.Property(p => p.Seniority)
            .HasConversion(
                v => v.ToString(), // Convert enum to string
                v => (Seniority)Enum.Parse(typeof(Seniority), v)) // Convert string back to enum
            .HasColumnName("seniority")
            .HasColumnType("varchar");

        builder.Property(x => x.Capabilities)
            .HasColumnName("capabilities")
            .HasColumnType("jsonb");
    }
}