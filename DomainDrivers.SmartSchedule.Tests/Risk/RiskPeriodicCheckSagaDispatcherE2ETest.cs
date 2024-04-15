using System.Linq.Expressions;
using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Resource.Employee;
using DomainDrivers.SmartSchedule.Risk;
using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Tests.Planning;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Demand = DomainDrivers.SmartSchedule.Allocation.Demand;
using Demands = DomainDrivers.SmartSchedule.Allocation.Demands;

namespace DomainDrivers.SmartSchedule.Tests.Risk;

public class RiskPeriodicCheckSagaDispatcherE2ETestApp : IntegrationTestAppBase
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services
                .Replace(ServiceDescriptor.Scoped<IRiskPushNotification>(_ => Substitute.For<IRiskPushNotification>()))
                .Replace(ServiceDescriptor.Scoped<TimeProvider>(_ => Substitute.For<TimeProvider>()))
                .Replace(ServiceDescriptor.Singleton<IProjectRepository, InMemoryProjectRepository>());;
        });
        base.ConfigureWebHost(builder);
    }
}

public class RiskPeriodicCheckSagaDispatcherE2ETest : IntegrationTest, IClassFixture<RiskPeriodicCheckSagaDispatcherE2ETestApp>
{
    private static readonly TimeSlot OneDayLong = TimeSlot.CreateDailyTimeSlotAtUtc(2021, 1, 1);
    private static readonly TimeSlot ProjectDates = new TimeSlot(DateTime.UtcNow, DateTime.UtcNow.AddDays(20));
    private readonly EmployeeFacade _employeeFacade;
    private readonly AllocationFacade _allocationFacade;
    private readonly RiskPeriodicCheckSagaDispatcher _riskSagaDispatcher;
    private readonly IRiskPushNotification _riskPushNotification;
    private readonly CashFlowFacade _cashFlowFacade;
    private readonly TimeProvider _clock;

    public RiskPeriodicCheckSagaDispatcherE2ETest(RiskPeriodicCheckSagaDispatcherE2ETestApp testApp) : base(testApp)
    {
        _employeeFacade = Scope.ServiceProvider.GetRequiredService<EmployeeFacade>();
        _allocationFacade = Scope.ServiceProvider.GetRequiredService<AllocationFacade>();
        _riskSagaDispatcher = Scope.ServiceProvider.GetRequiredService<RiskPeriodicCheckSagaDispatcher>();
        _riskPushNotification = Scope.ServiceProvider.GetRequiredService<IRiskPushNotification>();
        _cashFlowFacade = Scope.ServiceProvider.GetRequiredService<CashFlowFacade>();
        _clock = Scope.ServiceProvider.GetRequiredService<TimeProvider>();
    }

    [Fact]
    public async Task InformsAboutDemandSatisfied()
    {
        //given
        var projectId = ProjectAllocationsId.NewOne();
        var java = Capability.Skill("JAVA-MID-JUNIOR");
        var javaOneDayDemand = new Demand(java, OneDayLong);
        //and
        await _riskSagaDispatcher.Handle(
            NotSatisfiedDemands.ForOneProject(projectId, Demands.Of(javaOneDayDemand),
                    _clock.GetUtcNow().DateTime), CancellationToken.None);

        //when
        await _riskSagaDispatcher.Handle(
            NotSatisfiedDemands.AllSatisfied( projectId,  _clock.GetUtcNow().DateTime),
            CancellationToken.None);

        //then
        _riskPushNotification
            .Received(1)
            .NotifyDemandsSatisfied(projectId);
    }
    
    [Fact]
    public async Task InformsAboutDemandSatisfiedForAllProjects() 
    {
        //given
        var projectId = ProjectAllocationsId.NewOne();
        var projectId2 = ProjectAllocationsId.NewOne();
        //and
        var noMissingDemands =
            new Dictionary<ProjectAllocationsId, Demands>()
            {
                { projectId, Demands.None() },
                { projectId2, Demands.None() }
            };
           
        //when
        await _riskSagaDispatcher.Handle(new NotSatisfiedDemands(noMissingDemands,  _clock.GetUtcNow().DateTime), CancellationToken.None);

        //then
        _riskPushNotification
            .Received(1)
            .NotifyDemandsSatisfied(projectId);
        _riskPushNotification
            .Received(1)
            .NotifyDemandsSatisfied(projectId2);
    }

    [Fact]
    public async Task InformsAboutPotentialRiskWhenResourceTakenOver()
    {
        //given
        var projectId = ProjectAllocationsId.NewOne();
        var java = Capability.Skill("JAVA-MID-JUNIOR");
        var javaOneDayDemand = new Demand(java, OneDayLong);
        //and
        await _riskSagaDispatcher.Handle(
            NotSatisfiedDemands.ForOneProject(projectId, Demands.Of(javaOneDayDemand),
                _clock.GetUtcNow().DateTime), CancellationToken.None);
        //and
        await _riskSagaDispatcher.Handle(
            NotSatisfiedDemands.AllSatisfied(projectId, _clock.GetUtcNow().DateTime),
            CancellationToken.None);
        //and
        await _riskSagaDispatcher.Handle(
            new ProjectAllocationScheduled(projectId, ProjectDates, _clock.GetUtcNow().DateTime),
            CancellationToken.None);

        //when
        _riskPushNotification.ClearReceivedCalls();
        ItIsDaysBeforeDeadline(100);
        await _riskSagaDispatcher.Handle(new ResourceTakenOver(ResourceId.NewOne(),
                new HashSet<Owner>() { Owner.Of(projectId.Id) }, OneDayLong, _clock.GetUtcNow().DateTime),
            CancellationToken.None);

        //then
        _riskPushNotification
            .Received(1)
            .NotifyAboutPossibleRisk(projectId);
    }

    [Fact]
    public async Task DoesNothingWhenResourceTakenOverFromFromUnknownProject()
    {
        //given
        var unknown = ProjectId.NewOne();

        //when
        await _riskSagaDispatcher.Handle(
            new ResourceTakenOver(ResourceId.NewOne(), new HashSet<Owner>() { Owner.Of(unknown.Id) }, OneDayLong,
                _clock.GetUtcNow().DateTime), CancellationToken.None);

        //then
        Assert.Empty(_riskPushNotification.ReceivedCalls());
    }
    
    private static Expression<Predicate<IDictionary<Demand, AllocatableCapabilitiesSummary>>>
        EmployeeWasSuggestedForDemand(Demand demand, EmployeeId employee)
    {
        return suggestions =>
            suggestions[demand].All.Any(suggestion => suggestion.AllocatableResourceId == employee.ToAllocatableResourceId());
    }

    private void ItIsDaysBeforeDeadline(int days)
    {
        _clock.GetUtcNow().Returns(ProjectDates.To.AddDays(-days));
    }
}