namespace DomainDrivers.SmartSchedule.Availability;

public record ResourceAvailabilityId(Guid? Id)
{
    public static ResourceAvailabilityId None() {
        return new ResourceAvailabilityId((Guid?)null);
    }

    public static ResourceAvailabilityId NewOne()
    {
        return new ResourceAvailabilityId(Guid.NewGuid());
    }

    public static ResourceAvailabilityId Of(string? id) {
        if (id == null) {
            return None();
        }
        return new ResourceAvailabilityId(Guid.Parse(id));
    }
}