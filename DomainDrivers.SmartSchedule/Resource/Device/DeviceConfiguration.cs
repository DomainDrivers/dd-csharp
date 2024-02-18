using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivers.SmartSchedule.Resource.Device;

public static class DeviceConfiguration
{
    public static IServiceCollection AddDevice(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDeviceDbContext>(
            sp => sp.GetRequiredService<SmartScheduleDbContext>());
        serviceCollection.AddTransient<DeviceRepository>();
        serviceCollection.AddTransient<DeviceFacade>();
        return serviceCollection;
    }
}

public class DeviceEntityTypeConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");

        builder.Property<DeviceId>(x => x.Id)
            .HasConversion(
                projectId => projectId.Id,
                projectId => new DeviceId(projectId))
            .HasColumnName("device_id");
        builder.HasKey(x => x.Id);

        builder.Property("_version")
            .HasColumnName("version")
            .IsConcurrencyToken();

        builder.Property(p => p.Model)
            .HasColumnName("model");

        builder.Property(x => x.Capabilities)
            .HasColumnName("capabilities")
            .HasColumnType("jsonb");
    }
}