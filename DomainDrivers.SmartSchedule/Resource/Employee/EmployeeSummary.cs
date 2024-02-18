using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Resource.Employee;

public record EmployeeSummary(EmployeeId Id, string Name, string LastName, Seniority Seniority, ISet<Capability> Skills, ISet<Capability> Permissions)
{
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, LastName, Seniority, Skills.CalculateHashCode(), Permissions.CalculateHashCode());
    }
    
    public virtual bool Equals(EmployeeSummary? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Name == other.Name && LastName == other.LastName && Seniority == other.Seniority &&
               Skills.SetEquals(other.Skills) && Permissions.SetEquals(other.Permissions);
    }
}