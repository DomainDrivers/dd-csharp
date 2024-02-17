namespace DomainDrivers.SmartSchedule.Availability;

public record ResourceId(Guid? Id)
{
    public static ResourceId NewOne()
    {
        return new ResourceId(Guid.NewGuid());
    }

    public static ResourceId None() {
        return new ResourceId((Guid?)null);
    }

    public static ResourceId Of(string? id) {
        if (id == null) {
            return None();
        }
        return new ResourceId(Guid.Parse(id));
    }
}