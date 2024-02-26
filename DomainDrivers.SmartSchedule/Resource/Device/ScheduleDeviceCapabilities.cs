using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Shared.CapabilitySelector;

namespace DomainDrivers.SmartSchedule.Resource.Device;

public class ScheduleDeviceCapabilities
{
    private readonly DeviceRepository _deviceRepository;
    private readonly CapabilityScheduler _capabilityScheduler;

    public ScheduleDeviceCapabilities(DeviceRepository deviceRepository, CapabilityScheduler capabilityScheduler)
    {
        _deviceRepository = deviceRepository;
        _capabilityScheduler = capabilityScheduler;
    }

    public async Task<IList<AllocatableCapabilityId>> SetupDeviceCapabilities(DeviceId deviceId, TimeSlot timeSlot)
    {
        var summary = await _deviceRepository.FindSummary(deviceId);
        return await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(deviceId.ToAllocatableResourceId(),
            new List<CapabilitySelector>() { CanPerformAllAtTheTime(summary.Assets) }, timeSlot);
    }
}