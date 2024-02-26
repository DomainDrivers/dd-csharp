using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Resource.Device;

public class DeviceFacade
{
    private readonly DeviceRepository _deviceRepository;
    private readonly ScheduleDeviceCapabilities _scheduleDeviceCapabilities;
    private readonly IUnitOfWork _unitOfWork;

    public DeviceFacade(DeviceRepository deviceRepository, ScheduleDeviceCapabilities scheduleDeviceCapabilities,
        IUnitOfWork unitOfWork)
    {
        _deviceRepository = deviceRepository;
        _scheduleDeviceCapabilities = scheduleDeviceCapabilities;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviceSummary> FindDevice(DeviceId deviceId)
    {
        return await _deviceRepository.FindSummary(deviceId);
    }

    public async Task<IList<Capability>> FindAllCapabilities()
    {
        return await _deviceRepository.FindAllCapabilities();
    }
    
    public async Task<DeviceId> CreateDevice(string model, ISet<Capability> assets)
    {
        return await _unitOfWork.InTransaction(async () =>
        {
            var deviceId = DeviceId.NewOne();
            var device = new Device(deviceId, model, assets);
            await _deviceRepository.Add(device);
            return deviceId;
        });
    }

    public async Task<IList<AllocatableCapabilityId>> ScheduleCapabilities(DeviceId deviceId, TimeSlot oneDay)
    {
        return await _scheduleDeviceCapabilities.SetupDeviceCapabilities(deviceId, oneDay);
    }
}