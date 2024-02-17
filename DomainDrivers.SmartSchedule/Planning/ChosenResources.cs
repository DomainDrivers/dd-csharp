using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public record ChosenResources(ISet<ResourceId> Resources, TimeSlot TimeSlot)
{
    public static ChosenResources None()
    {
        return new ChosenResources(new HashSet<ResourceId>(), TimeSlot.Empty());
    }

    public virtual bool Equals(ChosenResources? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Resources.SetEquals(other.Resources)
               && TimeSlot == other.TimeSlot;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Resources.CalculateHashCode(), TimeSlot);
    }
}