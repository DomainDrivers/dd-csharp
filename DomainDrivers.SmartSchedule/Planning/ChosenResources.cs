using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public record ChosenResources(ISet<ResourceName> Resources, TimeSlot? TimeSlot)
{
    public static ChosenResources None()
    {
        return new ChosenResources(new HashSet<ResourceName>(), null);
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