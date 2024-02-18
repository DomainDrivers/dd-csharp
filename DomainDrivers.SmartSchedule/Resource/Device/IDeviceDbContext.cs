using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Resource.Device;

public interface IDeviceDbContext
{
    public DbSet<Device> Devices { get; }
}