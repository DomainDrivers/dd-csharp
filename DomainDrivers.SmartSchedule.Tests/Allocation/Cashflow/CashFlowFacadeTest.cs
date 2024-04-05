using System.Linq.Expressions;
using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Shared;
using NSubstitute;

namespace DomainDrivers.SmartSchedule.Tests.Allocation.Cashflow;

public class CashFlowFacadeTest
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private readonly CashFlowFacade _cashFlowFacade;
    private readonly IEventsPublisher _eventsPublisher;

    public CashFlowFacadeTest()
    {
        _eventsPublisher = Substitute.For<IEventsPublisher>();
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(Now));
        _cashFlowFacade = CashFlowTestConfiguration.CashFlowFacade(_eventsPublisher,timeProvider);
    }

    [Fact]
    public async Task CanSaveCashFlow()
    {
        //given
        var projectId = ProjectAllocationsId.NewOne();

        //when
        await _cashFlowFacade.AddIncomeAndCost(projectId, Income.Of(100), Cost.Of(50));

        //then
        Assert.Equal(Earnings.Of(50), await _cashFlowFacade.Find(projectId));
    }

    [Fact]
    public async Task UpdatingCashFlowEmitsAnEvent()
    {
        //given
        var projectId = ProjectAllocationsId.NewOne();
        var income = Income.Of(100);
        var cost = Cost.Of(50);

        //when
        await _cashFlowFacade.AddIncomeAndCost(projectId, income, cost);

        //then
        await _eventsPublisher.Received(1)
            .Publish(Arg.Is(IsEarningsRecalculatedEvent(projectId, Earnings.Of(50))));
    }

    private static Expression<Predicate<EarningsRecalculated>> IsEarningsRecalculatedEvent(
        ProjectAllocationsId projectId, Earnings earnings)
    {
        return @event => @event.ProjectId == projectId
            && @event.Earnings == earnings
            && @event.OccurredAt != default;
    }
}