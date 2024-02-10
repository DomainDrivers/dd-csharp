namespace DomainDrivers.SmartSchedule.Allocation;

public record ProjectAllocationsId(Guid Id)
{
    public static ProjectAllocationsId NewOne()
    {
        return new ProjectAllocationsId(Guid.NewGuid());
    }
}