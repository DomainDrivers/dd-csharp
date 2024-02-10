namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public record Earnings(decimal Value)
{
    public static Earnings Of(int integer)
    {
        return new Earnings(integer);
    }

    public bool GreaterThan(Earnings value)
    {
        return Value > value.Value;
    }
}