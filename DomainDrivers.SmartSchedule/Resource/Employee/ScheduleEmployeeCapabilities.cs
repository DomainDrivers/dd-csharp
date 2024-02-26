using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Resource.Employee;

public class ScheduleEmployeeCapabilities
{
    private readonly EmployeeRepository _employeeRepository;
    private readonly CapabilityScheduler _capabilityScheduler;

    public ScheduleEmployeeCapabilities(EmployeeRepository employeeRepository, CapabilityScheduler capabilityScheduler)
    {
        _employeeRepository = employeeRepository;
        _capabilityScheduler = capabilityScheduler;
    }

    public async Task<IList<AllocatableCapabilityId>> SetupEmployeeCapabilities(EmployeeId employeeId,
        TimeSlot timeSlot)
    {
        var summary = await _employeeRepository.FindSummary(employeeId);
        var policy = FindAllocationPolicy(summary);
        var capabilities = policy.SimultaneousCapabilitiesOf(summary);
        return await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(employeeId.ToAllocatableResourceId(),
            capabilities, timeSlot);
    }

    private IEmployeeAllocationPolicy FindAllocationPolicy(EmployeeSummary employee)
    {
        if (employee.Seniority == Seniority.LEAD)
        {
            return IEmployeeAllocationPolicy.Simultaneous(IEmployeeAllocationPolicy.OneOfSkills(),
                IEmployeeAllocationPolicy.PermissionsInMultipleProjects(3));
        }

        return IEmployeeAllocationPolicy.DefaultPolicy();
    }
}