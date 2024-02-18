using DomainDrivers.SmartSchedule.Resource.Device;
using DomainDrivers.SmartSchedule.Resource.Employee;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Resource;

public class ResourceFacade
{
    private readonly EmployeeFacade _employeeFacade;
    private readonly DeviceFacade _deviceFacade;

    public ResourceFacade(EmployeeFacade employeeFacade, DeviceFacade deviceFacade)
    {
        _employeeFacade = employeeFacade;
        _deviceFacade = deviceFacade;
    }
    
    public async Task<IList<Capability>> FindAllCapabilities() 
    {
        var employeeCapabilities = await _employeeFacade.FindAllCapabilities();
        var deviceCapabilities = await _deviceFacade.FindAllCapabilities();
        return deviceCapabilities.Concat(employeeCapabilities).ToList();
    }
}