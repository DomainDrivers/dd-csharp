namespace DomainDrivers.SmartSchedule.Allocation;

public record ResourceId(Guid Id)
{
    public static ResourceId NewOne()
    {
        return new ResourceId(Guid.NewGuid());
    }
}