using DomainDrivers.SmartSchedule.Resource.Employee;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Resource.Employee.Seniority;

namespace DomainDrivers.SmartSchedule.Tests.Resource.Employee;

public class CreatingEmployeeTest : IntegrationTest
{
    private readonly EmployeeFacade _employeeFacade;

    public CreatingEmployeeTest(IntegrationTestApp testApp) : base(testApp)
    {
        _employeeFacade = Scope.ServiceProvider.GetRequiredService<EmployeeFacade>();
    }

    [Fact]
    public async Task CanCreateAndLoadEmployee()
    {
        //given
        var employee =
            await _employeeFacade.AddEmployee("resourceName", "lastName", SENIOR, Capability.Skills("JAVA, PYTHON"),
                Capability.Permissions("ADMIN, COURT"));

        //when
        var loaded = await _employeeFacade.FindEmployee(employee);
        
        //then
        Assert.Equal(Capability.Skills("JAVA, PYTHON"), loaded.Skills);
        Assert.Equal(Capability.Permissions("ADMIN, COURT"), loaded.Permissions);
        Assert.Equal("resourceName", loaded.Name);
        Assert.Equal("lastName", loaded.LastName);
        Assert.Equal(SENIOR, loaded.Seniority);
    }

    [Fact]
    public async Task CanFindAllCapabilities()
    {
        //given
        await _employeeFacade.AddEmployee("staszek", "lastName", SENIOR, Capability.Skills("JAVA12", "PYTHON21"),
            Capability.Permissions("ADMIN1", "COURT1"));
        await _employeeFacade.AddEmployee("leon", "lastName", SENIOR, Capability.Skills("JAVA12", "PYTHON21"),
            Capability.Permissions("ADMIN2", "COURT2"));
        await _employeeFacade.AddEmployee("sławek", "lastName", SENIOR, Capability.Skills("JAVA12", "PYTHON21"),
            Capability.Permissions("ADMIN3", "COURT3"));

        //when
        var loaded = await _employeeFacade.FindAllCapabilities();

        //then
        Assert.Contains(Capability.Permission("ADMIN1"), loaded);
        Assert.Contains(Capability.Permission("ADMIN2"), loaded);
        Assert.Contains(Capability.Permission("ADMIN3"), loaded);
        Assert.Contains(Capability.Permission("COURT1"), loaded);
        Assert.Contains(Capability.Permission("COURT2"), loaded);
        Assert.Contains(Capability.Permission("COURT3"), loaded);
        Assert.Contains(Capability.Skill("JAVA12"), loaded);
        Assert.Contains(Capability.Skill("JAVA12"), loaded);
        Assert.Contains(Capability.Skill("JAVA12"), loaded);
        Assert.Contains(Capability.Skill("PYTHON21"), loaded);
        Assert.Contains(Capability.Skill("PYTHON21"), loaded);
        Assert.Contains(Capability.Skill("PYTHON21"), loaded);
    }
}