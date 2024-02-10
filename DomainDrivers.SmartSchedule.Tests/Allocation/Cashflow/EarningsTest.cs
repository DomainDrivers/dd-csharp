using DomainDrivers.SmartSchedule.Allocation.Cashflow;

namespace DomainDrivers.SmartSchedule.Tests.Allocation.Cashflow;

public class EarningsTest
{
    [Fact]
    public void IncomeMinusCostTest()
    {
        Assert.Equal(Earnings.Of(9), Income.Of(10.0m).Minus(Cost.Of(1)));
        Assert.Equal(Earnings.Of(8), Income.Of(10.0m).Minus(Cost.Of(2)));
        Assert.Equal(Earnings.Of(7), Income.Of(10.0m).Minus(Cost.Of(3)));
        Assert.Equal(Earnings.Of(-70), Income.Of(100).Minus(Cost.Of(170)));
    }
    
    [Fact]
    public void GreaterThanTest()
    {
        //expect
        Assert.True(Earnings.Of(10).GreaterThan(Earnings.Of(9)));
        Assert.True(Earnings.Of(10).GreaterThan(Earnings.Of(0)));
        Assert.True(Earnings.Of(10).GreaterThan(Earnings.Of(-1)));
        Assert.False(Earnings.Of(10).GreaterThan(Earnings.Of(10)));
        Assert.False(Earnings.Of(10).GreaterThan(Earnings.Of(11)));
    }
}