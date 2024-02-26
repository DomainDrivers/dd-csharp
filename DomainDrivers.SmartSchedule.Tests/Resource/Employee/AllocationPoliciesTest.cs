using DomainDrivers.SmartSchedule.Resource.Employee;
using DomainDrivers.SmartSchedule.Shared;
using NUnit.Framework.Legacy;
using static DomainDrivers.SmartSchedule.Shared.Capability;
using static DomainDrivers.SmartSchedule.Resource.Employee.IEmployeeAllocationPolicy;

namespace DomainDrivers.SmartSchedule.Tests.Resource.Employee;

public class AllocationPoliciesTest
{
    [Fact]
    public void DefaultPolicyShouldReturnJustOneSkillAtOnce()
    {
        //given
        var employee = new EmployeeSummary(EmployeeId.NewOne(), "resourceName", "lastName", Seniority.LEAD,
            Capability.Skills("JAVA"), Capability.Permissions("ADMIN"));

        //when
        var capabilities = IEmployeeAllocationPolicy.DefaultPolicy().SimultaneousCapabilitiesOf(employee);

        //then
        Assert.Single(capabilities);
        CollectionAssert.AreEquivalent(new HashSet<Capability>() { Permission("ADMIN"), Skill("JAVA") },
            capabilities[0].Capabilities);
    }

    [Fact]
    public void PermissionsCanBeSharedBetweenProjects()
    {
        //given
        var policy = PermissionsInMultipleProjects(3);
        var employee = new EmployeeSummary(EmployeeId.NewOne(), "resourceName", "lastName", Seniority.LEAD,
            Capability.Skills("JAVA"), Capability.Permissions("ADMIN"));

        //when
        var capabilities = policy.SimultaneousCapabilitiesOf(employee);

        //then
        Assert.Equal(3, capabilities.Count);

        CollectionAssert.AreEquivalent(new List<Capability>() { Permission("ADMIN"), Permission("ADMIN"), Permission("ADMIN") },
            capabilities.SelectMany(cap => cap.Capabilities));
    }

    [Fact]
    public void CanCreateCompositePolicy()
    {
        //given
        var policy = Simultaneous(PermissionsInMultipleProjects(3), IEmployeeAllocationPolicy.OneOfSkills());
        var employee = new EmployeeSummary(EmployeeId.NewOne(), "resourceName", "lastName", Seniority.LEAD,
            Capability.Skills("JAVA", "PYTHON"), Capability.Permissions("ADMIN"));

        //when
        var capabilities = policy.SimultaneousCapabilitiesOf(employee);

        //then
        Assert.Equal(4, capabilities.Count);
        CollectionAssert.AreEquivalent(
            new List<CapabilitySelector>()
            {
                CapabilitySelector.CanPerformOneOf(Capability.Skills("JAVA", "PYTHON")),
                CapabilitySelector.CanJustPerform(Permission("ADMIN")),
                CapabilitySelector.CanJustPerform(Permission("ADMIN")),
                CapabilitySelector.CanJustPerform(Permission("ADMIN"))
            }, capabilities);
    }
}