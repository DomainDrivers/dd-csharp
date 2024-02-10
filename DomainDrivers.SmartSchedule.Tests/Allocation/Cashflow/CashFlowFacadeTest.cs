using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;

namespace DomainDrivers.SmartSchedule.Tests.Allocation.Cashflow;

public class CashFlowFacadeTest : IntegrationTest
{
    private readonly CashFlowFacade _cashFlowFacade;

    public CashFlowFacadeTest(IntegrationTestApp testApp) : base(testApp)
    {
        _cashFlowFacade = Scope.ServiceProvider.GetRequiredService<CashFlowFacade>();
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
}