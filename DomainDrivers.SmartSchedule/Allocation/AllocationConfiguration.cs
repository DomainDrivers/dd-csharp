using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivers.SmartSchedule.Allocation;

public static class AllocationConfiguration
{
    public static IServiceCollection AddAllocation(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAllocationDbContext>(
            sp => sp.GetRequiredService<SmartScheduleDbContext>());
        serviceCollection.AddTransient<AllocationFacade>();
        return serviceCollection;
    }
}

public class ProjectAllocationsEntityTypeConfiguration : IEntityTypeConfiguration<ProjectAllocations>
{
    public void Configure(EntityTypeBuilder<ProjectAllocations> builder)
    {
        builder.ToTable("project_allocations");

        builder.Property<ProjectAllocationsId>(p => p.ProjectId)
            .HasConversion(
                projectId => projectId.Id,
                projectId => new ProjectAllocationsId(projectId))
            .HasColumnName("project_allocations_id");
        builder.HasKey(p => p.ProjectId);

        builder.Property(p => p.Allocations)
            .HasColumnName("allocations")
            .HasColumnType("jsonb");

        builder.Property(p => p.Demands)
            .HasColumnName("demands")
            .HasColumnType("jsonb");

        builder.OwnsOne<TimeSlot>(p => p.TimeSlot, ts =>
        {
            ts.Property(t => t.From).HasColumnName("from_date");
            ts.Property(t => t.To).HasColumnName("to_date");
        });
    }
}