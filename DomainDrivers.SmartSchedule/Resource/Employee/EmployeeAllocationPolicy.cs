using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Resource.Employee;

public interface IEmployeeAllocationPolicy
{
    IList<CapabilitySelector> SimultaneousCapabilitiesOf(EmployeeSummary employee);

    public static IEmployeeAllocationPolicy DefaultPolicy()
    {
        return new DefaultPolicy();
    }

    public static IEmployeeAllocationPolicy PermissionsInMultipleProjects(int howMany)
    {
        return new PermissionsInMultipleProjectsPolicy(howMany);
    }

    public static IEmployeeAllocationPolicy OneOfSkills()
    {
        return new OneOfSkillsPolicy();
    }

    public static CompositePolicy Simultaneous(params IEmployeeAllocationPolicy[] policies)
    {
        return new CompositePolicy(policies.ToList());
    }
}

file class DefaultPolicy : IEmployeeAllocationPolicy
{
    public IList<CapabilitySelector> SimultaneousCapabilitiesOf(EmployeeSummary employee)
    {
        var all = new HashSet<Capability>();
        all.UnionWith(employee.Skills);
        all.UnionWith(employee.Permissions);
        return new List<CapabilitySelector> { CapabilitySelector.CanPerformOneOf(all) };
    }
}

file class PermissionsInMultipleProjectsPolicy : IEmployeeAllocationPolicy
{
    private readonly int _howMany;

    public PermissionsInMultipleProjectsPolicy(int howMany)
    {
        _howMany = howMany;
    }

    public IList<CapabilitySelector> SimultaneousCapabilitiesOf(EmployeeSummary employee)
    {
        return employee.Permissions
            .SelectMany(permission => Enumerable.Range(0, _howMany).Select(_ => permission))
            .Select(CapabilitySelector.CanJustPerform)
            .ToList();
    }
}

file class OneOfSkillsPolicy : IEmployeeAllocationPolicy
{
    public IList<CapabilitySelector> SimultaneousCapabilitiesOf(EmployeeSummary employee)
    {
        return new List<CapabilitySelector> { CapabilitySelector.CanPerformOneOf(employee.Skills) };
    }
}

public class CompositePolicy : IEmployeeAllocationPolicy
{
    private readonly IList<IEmployeeAllocationPolicy> _policies;

    public CompositePolicy(IList<IEmployeeAllocationPolicy> policies)
    {
        _policies = policies;
    }

    public IList<CapabilitySelector> SimultaneousCapabilitiesOf(EmployeeSummary employee)
    {
        return _policies
            .SelectMany(p => p.SimultaneousCapabilitiesOf(employee))
            .ToList();
    }
}