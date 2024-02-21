using DomainDrivers.SmartSchedule.Availability;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public record AllocatableCapabilityId(Guid? Id)
{
    public static AllocatableCapabilityId NewOne()
    {
        return new AllocatableCapabilityId(Guid.NewGuid());
    }
    
    public static AllocatableCapabilityId None()
    {
        return new AllocatableCapabilityId((Guid?)null);
    }
    
    public ResourceId ToAvailabilityResourceId()
    {
        return ResourceId.Of(Id.ToString());
    }
    
    public static AllocatableCapabilityId From(ResourceId resourceId) {
        return new AllocatableCapabilityId(resourceId.Id!.Value);
    }
}