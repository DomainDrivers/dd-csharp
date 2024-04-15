using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Resource.Employee;
using DomainDrivers.SmartSchedule.Risk;
using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Tests.Planning;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using static DomainDrivers.SmartSchedule.Resource.Employee.Seniority;
using static DomainDrivers.SmartSchedule.Shared.Capability;

namespace DomainDrivers.SmartSchedule.Tests.Risk;

public class VerifyEnoughDemandsDuringPlanningTestApp : IntegrationTestAppBase
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
            services
                .Replace(ServiceDescriptor.Singleton<IRiskPushNotification>(_ => Substitute.For<IRiskPushNotification>()))
                .Replace(ServiceDescriptor.Scoped<IProjectRepository, InMemoryProjectRepository>())
        );
        base.ConfigureWebHost(builder);
    }
}

public class VerifyEnoughDemandsDuringPlanningTest : IntegrationTest, IClassFixture<VerifyEnoughDemandsDuringPlanningTestApp>
{
    private readonly IRiskPushNotification _riskPushNotification;
    private readonly EmployeeFacade _employeeFacade;
    private readonly PlanningFacade _planningFacade;

    public VerifyEnoughDemandsDuringPlanningTest(VerifyEnoughDemandsDuringPlanningTestApp testApp) : base(testApp)
    {
        _riskPushNotification = Scope.ServiceProvider.GetRequiredService<IRiskPushNotification>();
        _employeeFacade = Scope.ServiceProvider.GetRequiredService<EmployeeFacade>();
        _planningFacade = Scope.ServiceProvider.GetRequiredService<PlanningFacade>();
    }
    
    [Fact]
    public async Task DoesNothingWhenEnoughResources()
    {
        //given
        await _employeeFacade.AddEmployee("resourceName", "lastName", SENIOR, Capability.Skills("JAVA5", "PYTHON"), Capability.Permissions());
        await _employeeFacade.AddEmployee("resourceName", "lastName", SENIOR, Capability.Skills("C#", "RUST"), Capability.Permissions());
        //and
        var projectId = await _planningFacade.AddNewProject("java5");

        //when
        await _planningFacade.AddDemands(projectId, Demands.Of(new Demand(Skill("JAVA5"))));

        //then
        _riskPushNotification
            .DidNotReceive()
            .NotifyAboutPossibleRiskDuringPlanning(projectId, Demands.Of(Demand.DemandFor(Skill("JAVA"))));
    }

    [Fact]
    public async Task NotifiesWhenNotEnoughResources()
    {
        //given
        await _employeeFacade.AddEmployee("resourceName", "lastName", SENIOR, Capability.Skills("JAVA"), Capability.Permissions());
        await _employeeFacade.AddEmployee("resourceName", "lastName", SENIOR, Capability.Skills("C"), Capability.Permissions());
        //and
        var java = await _planningFacade.AddNewProject("java");
        var c = await _planningFacade.AddNewProject("C");
        //and
        await _planningFacade.AddDemands(java, Demands.Of(new Demand(Skill("JAVA"))));
        await _planningFacade.AddDemands(c, Demands.Of(new Demand(Skill("RUST"))));

        //when
        var rust = await _planningFacade.AddNewProject("rust");
        await _planningFacade.AddDemands(rust, Demands.Of(new Demand(Skill("RUST"))));

        //then
        _riskPushNotification
            .Received(1)
            .NotifyAboutPossibleRiskDuringPlanning(rust, Demands.Of(Demand.DemandFor(Skill("RUST"))));
    }
}