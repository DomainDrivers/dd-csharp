using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Availability;

public record ResourceTakenOver(
    Guid EventId,
    ResourceId ResourceId,
    ISet<Owner> PreviousOwners,
    TimeSlot Slot,
    DateTime OccurredAt) : IPublishedEvent
{
    public ResourceTakenOver(ResourceId resourceId,
        ISet<Owner> previousOwners,
        TimeSlot slot,
        DateTime occurredAt) : this(Guid.NewGuid(), resourceId, previousOwners, slot, occurredAt) { }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(EventId, ResourceId, PreviousOwners.CalculateHashCode(), Slot, OccurredAt);
    }

    public virtual bool Equals(ResourceTakenOver? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EventId == other.EventId && ResourceId == other.ResourceId &&
               PreviousOwners.SetEquals(other.PreviousOwners)
               && Slot == other.Slot && OccurredAt == other.OccurredAt;
    }
}