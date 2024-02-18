namespace DomainDrivers.SmartSchedule.Resource.Employee;

public record EmployeeId(Guid Id)
{
    public static EmployeeId NewOne()
    {
        return new EmployeeId(Guid.NewGuid());
    }
}