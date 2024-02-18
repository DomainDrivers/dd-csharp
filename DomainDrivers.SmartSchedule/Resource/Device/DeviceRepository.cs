using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Resource.Device;

public class DeviceRepository
{
    private readonly IDeviceDbContext _deviceDbContext;

    public DeviceRepository(IDeviceDbContext deviceDbContext)
    {
        _deviceDbContext = deviceDbContext;
    }
    
    public async Task<DeviceSummary> FindSummary(DeviceId deviceId)
    {
        var device = await _deviceDbContext.Devices.SingleAsync(x => x.Id == deviceId);
        var assets = device.Capabilities;
        return new DeviceSummary(deviceId, device.Model, assets);
    }

    public async Task<IList<Capability>> FindAllCapabilities()
    {
        return (await _deviceDbContext.Devices.ToListAsync())
            .SelectMany(device => device.Capabilities)
            .ToList();
    }

    public async Task Add(Device device)
    {
        await _deviceDbContext.Devices.AddAsync(device);
    }
}