namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public record Cost(decimal Value)
{
    public static Cost Of(int integer)
    {
        return new Cost(integer);
    }
}