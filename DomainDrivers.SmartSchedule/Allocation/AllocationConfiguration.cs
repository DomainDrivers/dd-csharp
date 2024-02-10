using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivers.SmartSchedule.Allocation;

public class ProjectAllocationsEntityTypeConfiguration : IEntityTypeConfiguration<ProjectAllocations>
{
    public void Configure(EntityTypeBuilder<ProjectAllocations> builder)
    {
        builder.ToTable("project_allocations");

        builder.Property<ProjectAllocationsId>("_projectId")
            .HasConversion(
                projectId => projectId.Id,
                projectId => new ProjectAllocationsId(projectId))
            .HasColumnName("project_allocations_id");
        builder.HasKey("_projectId");

        builder.Property(p => p.Allocations)
            .HasColumnName("allocations")
            .HasColumnType("jsonb");

        builder.Property("_demands")
            .HasColumnName("demands")
            .HasColumnType("jsonb");

        builder.OwnsOne<TimeSlot>("_timeSlot", ts =>
        {
            ts.Property(t => t.From).HasColumnName("from_date");
            ts.Property(t => t.To).HasColumnName("to_date");
        });
    }
}