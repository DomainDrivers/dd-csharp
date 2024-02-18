using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Resource.Employee;

public class Employee
{
    public EmployeeId Id { get; private set; } = EmployeeId.NewOne();
    
    private int _version;
    
    public string Name { get; private set; }
    
    public string LastName { get; private set; }
    
    public Seniority Seniority { get; private set; }
    
    public ISet<Capability> Capabilities { get; private set; }

    public Employee(EmployeeId id, string name, string lastName, Seniority seniority, ISet<Capability> capabilities)
    {
        Id = id;
        Name = name;
        LastName = lastName;
        Seniority = seniority;
        Capabilities = capabilities;
    }
}