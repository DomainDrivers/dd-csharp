using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public static class CapabilityPlanningConfiguration
{
    public static IServiceCollection AddCapabilityPlanning(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICapabilitySchedulingDbContext>(
            sp => sp.GetRequiredService<SmartScheduleDbContext>());
        serviceCollection.AddTransient<AllocatableCapabilityRepository>();
        serviceCollection.AddTransient<ICapabilityFinder, CapabilityFinder>();
        serviceCollection.AddTransient<CapabilityScheduler>();
        return serviceCollection;
    }
}

public class AllocatableCapabilityEntityTypeConfiguration : IEntityTypeConfiguration<AllocatableCapability>
{
    public void Configure(EntityTypeBuilder<AllocatableCapability> builder)
    {
        builder.ToTable("allocatable_capabilities");

        builder.Property<AllocatableCapabilityId>(x => x.Id)
            .HasConversion(
                allocatableCapabilityId => allocatableCapabilityId.Id,
                allocatableCapabilityId => new AllocatableCapabilityId(allocatableCapabilityId!.Value))
            .HasColumnName("id");
        builder.HasKey(x => x.Id);

        builder.Property(p => p.Capabilities)
            .HasColumnName("possible_capabilities")
            .HasColumnType("jsonb");

        builder.Property<AllocatableResourceId>(x => x.ResourceId)
            .HasConversion(
                resourceId => resourceId.Id,
                resourceId => new AllocatableResourceId(resourceId))
            .HasColumnName("resource_id");

        builder.Ignore(x => x.TimeSlot);
        builder.OwnsOne<TimeSlot>(x => x.TimeSlot, ts =>
        {
            ts.Property(t => t.From).HasColumnName("from_date");
            ts.Property(t => t.To).HasColumnName("to_date");
        });
    }
}