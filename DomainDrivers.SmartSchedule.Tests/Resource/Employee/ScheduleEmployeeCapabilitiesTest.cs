using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Resource.Employee;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Resource.Employee;

public class ScheduleEmployeeCapabilitiesTest : IntegrationTestWithSharedApp
{
    private readonly CapabilityFinder _capabilityFinder;
    private readonly EmployeeFacade _employeeFacade;

    public ScheduleEmployeeCapabilitiesTest(IntegrationTestApp testApp) : base(testApp)
    {
        _capabilityFinder = Scope.ServiceProvider.GetRequiredService<CapabilityFinder>();
        _employeeFacade = Scope.ServiceProvider.GetRequiredService<EmployeeFacade>();
    }

    [Fact]
    public async Task CanSetupCapabilitiesAccordingToPolicy()
    {
        var employee = await _employeeFacade.AddEmployee("resourceName", "lastName", Seniority.LEAD,
            Capability.Skills("JAVA, PYTHON"),
            Capability.Permissions("ADMIN"));
        //when
        var oneDay = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
        var allocations = await _employeeFacade.ScheduleCapabilities(employee, oneDay);

        //then
        var loaded = await _capabilityFinder.FindById(allocations);
        Assert.Equal(allocations.Count, loaded.All.Count);
    }
}