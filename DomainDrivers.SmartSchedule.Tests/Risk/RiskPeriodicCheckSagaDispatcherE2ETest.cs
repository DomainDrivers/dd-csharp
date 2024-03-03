using System.Linq.Expressions;
using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Resource.Employee;
using DomainDrivers.SmartSchedule.Risk;
using DomainDrivers.SmartSchedule.Shared;
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
                .Replace(ServiceDescriptor.Scoped<TimeProvider>(_ => Substitute.For<TimeProvider>()));
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
            new ProjectAllocationsDemandsScheduled(projectId, Demands.Of(javaOneDayDemand),
                _clock.GetUtcNow().DateTime), CancellationToken.None);

        //when
        await _riskSagaDispatcher.Handle(
            new CapabilitiesAllocated(Guid.NewGuid(), projectId, Demands.None(), _clock.GetUtcNow().DateTime),
            CancellationToken.None);

        //then
        _riskPushNotification
            .Received(1)
            .NotifyDemandsSatisfied(projectId);
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
            new ProjectAllocationsDemandsScheduled(projectId, Demands.Of(javaOneDayDemand),
                _clock.GetUtcNow().DateTime), CancellationToken.None);
        //and
        await _riskSagaDispatcher.Handle(
            new CapabilitiesAllocated(Guid.NewGuid(), projectId, Demands.None(), _clock.GetUtcNow().DateTime),
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

    [Fact]
    public async Task WeeklyCheckDoesNothingWhenNotCloseToDeadlineAndDemandsNotSatisfied()
    {
        //given
        var projectId = ProjectAllocationsId.NewOne();
        var java = Capability.Skill("JAVA-MID-JUNIOR");
        var javaOneDayDemand = new Demand(java, OneDayLong);
        //and
        await _riskSagaDispatcher.Handle(
            new ProjectAllocationsDemandsScheduled(projectId, Demands.Of(javaOneDayDemand),
                _clock.GetUtcNow().DateTime), CancellationToken.None);
        //and
        await _riskSagaDispatcher.Handle(
            new ProjectAllocationScheduled(projectId, ProjectDates, _clock.GetUtcNow().DateTime),
            CancellationToken.None);

        //when
        ItIsDaysBeforeDeadline(100);
        await _riskSagaDispatcher.HandleWeeklyCheck();

        //then
        Assert.Empty(_riskPushNotification.ReceivedCalls());
    }

    [Fact]
    public async Task WeeklyCheckDoesNothingWhenCloseToDeadlineAndDemandsSatisfied()
    {
        //given   
        var projectId = ProjectAllocationsId.NewOne();
        var java = Capability.Skill("JAVA-MID-JUNIOR-UNIQUE");
        var javaOneDayDemand = new Demand(java, OneDayLong);
        await _riskSagaDispatcher.Handle(
            new ProjectAllocationsDemandsScheduled(projectId, Demands.Of(javaOneDayDemand),
                _clock.GetUtcNow().DateTime), CancellationToken.None);
        //and
        await _riskSagaDispatcher.Handle(
            new EarningsRecalculated(projectId, Earnings.Of(10), _clock.GetUtcNow().DateTime), CancellationToken.None);
        //and
        await _riskSagaDispatcher.Handle(
            new CapabilitiesAllocated(Guid.NewGuid(), projectId, Demands.None(), _clock.GetUtcNow().DateTime),
            CancellationToken.None);
        //and
        await _riskSagaDispatcher.Handle(
            new ProjectAllocationScheduled(projectId, ProjectDates, _clock.GetUtcNow().DateTime),
            CancellationToken.None);

        //when
        ItIsDaysBeforeDeadline(100);
        _riskPushNotification.ClearReceivedCalls();
        await _riskSagaDispatcher.HandleWeeklyCheck();

        //then
        Assert.Empty(_riskPushNotification.ReceivedCalls());
    }

    [Fact]
    public async Task FindReplacementsWhenDeadlineClose()
    {
        //given
        var projectId = ProjectAllocationsId.NewOne();
        var java = Capability.Skill("JAVA-MID-JUNIOR");
        var javaOneDayDemand = new Demand(java, OneDayLong);
        await _riskSagaDispatcher.Handle(
            new ProjectAllocationsDemandsScheduled(projectId, Demands.Of(javaOneDayDemand),
                _clock.GetUtcNow().DateTime), CancellationToken.None);
        //and
        await _riskSagaDispatcher.Handle(
            new EarningsRecalculated(projectId, Earnings.Of(10), _clock.GetUtcNow().DateTime), CancellationToken.None);
        //and
        await _riskSagaDispatcher.Handle(
            new ProjectAllocationScheduled(projectId, ProjectDates, _clock.GetUtcNow().DateTime),
            CancellationToken.None);
        //and
        var employee = await ThereIsEmployeeWithSkills(new HashSet<Capability>() { java }, OneDayLong);

        //when
        _riskPushNotification.ClearReceivedCalls();
        ItIsDaysBeforeDeadline(20);
        await _riskSagaDispatcher.HandleWeeklyCheck();

        //then
        _riskPushNotification
            .Received(1)
            .NotifyAboutAvailability(Arg.Is(projectId),
                Arg.Is(EmployeeWasSuggestedForDemand(javaOneDayDemand, employee)));
    }

    [Fact]
    public async Task SuggestResourcesFromDifferentProjects()
    {
        //given
        var highValueProject = ProjectAllocationsId.NewOne();
        var lowValueProject = ProjectAllocationsId.NewOne();
        //and
        var java = Capability.Skill("JAVA-MID-JUNIOR-SUPER-UNIQUE");
        var javaOneDayDemand = new Demand(java, OneDayLong);
        //and
        await _allocationFacade.ScheduleProjectAllocationDemands(highValueProject, Demands.Of(javaOneDayDemand));
        await _cashFlowFacade.AddIncomeAndCost(highValueProject, Income.Of(10000), Cost.Of(10));
        await _allocationFacade.ScheduleProjectAllocationDemands(lowValueProject, Demands.Of(javaOneDayDemand));
        await _cashFlowFacade.AddIncomeAndCost(lowValueProject, Income.Of(100), Cost.Of(10));
        //and
        var employee = await ThereIsEmployeeWithSkills(new HashSet<Capability>() { java }, OneDayLong);
        await _allocationFacade.AllocateToProject(lowValueProject, employee, OneDayLong);
        //and
        await _riskSagaDispatcher.Handle(
            new ProjectAllocationScheduled(highValueProject, ProjectDates, _clock.GetUtcNow().DateTime),
            CancellationToken.None);

        //when
        _riskPushNotification.ClearReceivedCalls();
        await _allocationFacade.EditProjectDates(highValueProject, ProjectDates);
        await _allocationFacade.EditProjectDates(lowValueProject, ProjectDates);
        ItIsDaysBeforeDeadline(1);
        await _riskSagaDispatcher.HandleWeeklyCheck();

        //then
        _riskPushNotification
            .Received(1)
            .NotifyProfitableRelocationFound(highValueProject, employee);
    }

    private static Expression<Predicate<IDictionary<Demand, AllocatableCapabilitiesSummary>>>
        EmployeeWasSuggestedForDemand(Demand demand, AllocatableCapabilityId allocatableCapabilityId)
    {
        return suggestions =>
            suggestions[demand].All.Any(suggestion => suggestion.Id == allocatableCapabilityId);
    }

    private async Task<AllocatableCapabilityId> ThereIsEmployeeWithSkills(ISet<Capability> skills, TimeSlot inSlot)
    {
        var staszek = await _employeeFacade.AddEmployee("Staszek", "Staszkowski", Seniority.MID, skills,
            Capability.Permissions());
        var allocatableCapabilityIds = await _employeeFacade.ScheduleCapabilities(staszek, inSlot);
        return Assert.Single(allocatableCapabilityIds);
    }

    private void ItIsDaysBeforeDeadline(int days)
    {
        _clock.GetUtcNow().Returns(ProjectDates.To.AddDays(-days));
    }
}