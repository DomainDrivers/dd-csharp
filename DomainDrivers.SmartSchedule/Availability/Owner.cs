namespace DomainDrivers.SmartSchedule.Availability;

public record Owner(Guid? OwnerId)
{
    public bool ByNone
    {
        get { return this == None(); }
    }

    public static Owner None()
    {
        return new Owner((Guid?)null);
    }

    public static Owner NewOne()
    {
        return new Owner(Guid.NewGuid());
    }

    public static Owner Of(Guid id)
    {
        return new Owner(id);
    }
}