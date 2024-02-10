namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public record Income(decimal Value)
{
    public static Income Of(decimal @decimal)
    {
        return new Income(@decimal);
    }

    public static Income Of(int integer)
    {
        return new Income(integer);
    }

    public Earnings Minus(Cost estimatedCosts)
    {
        return new Earnings(Value - estimatedCosts.Value);
    }
}