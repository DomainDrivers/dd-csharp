namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public record AllocatableResourceId(Guid Id)
{
    public static AllocatableResourceId NewOne()
    {
        return new AllocatableResourceId(Guid.NewGuid());
    }
}