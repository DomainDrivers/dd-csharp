using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Resource.Device;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Resource.Device;

public class ScheduleDeviceCapabilitiesTest : IntegrationTestWithSharedApp
{
    private readonly DeviceFacade _deviceFacade;
    private readonly CapabilityFinder _capabilityFinder;

    public ScheduleDeviceCapabilitiesTest(IntegrationTestApp testApp) : base(testApp)
    {
        _deviceFacade = Scope.ServiceProvider.GetRequiredService<DeviceFacade>();
        _capabilityFinder = Scope.ServiceProvider.GetRequiredService<CapabilityFinder>();
    }

    [Fact]
    public async Task CanSetupCapabilitiesAccordingToPolicy()
    {
        //given
        var device = await _deviceFacade.CreateDevice("super-bulldozer-3000",
            Capability.Assets("EXCAVATOR", "BULLDOZER"));
        //when
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var allocations = await _deviceFacade.ScheduleCapabilities(device, oneDay);
        
        //then
        var loaded = await _capabilityFinder.FindById(allocations);
        Assert.Equal(allocations.Count, loaded.All.Count);
    }
}