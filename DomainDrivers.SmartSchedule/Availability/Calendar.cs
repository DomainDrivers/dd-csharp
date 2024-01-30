using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Availability;

public record Calendar(ResourceName ResourceId, IDictionary<Owner, IList<TimeSlot>> CalendarEntries)
{
    public static Calendar WithAvailableSlots(ResourceName resourceId, params TimeSlot[] availableSlots)
    {
        return new Calendar(resourceId,
            new Dictionary<Owner, IList<TimeSlot>> { { Owner.None(), new List<TimeSlot>(availableSlots) } });
    }

    public static Calendar Empty(ResourceName resourceId)
    {
        return new Calendar(resourceId, new Dictionary<Owner, IList<TimeSlot>>());
    }

    public IList<TimeSlot> AvailableSlots()
    {
        if (CalendarEntries.TryGetValue(Owner.None(), out var availableSlots))
        {
            return availableSlots;
        }

        return new List<TimeSlot>();
    }

    public virtual bool Equals(Calendar? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ResourceId == other.ResourceId
               && CalendarEntries.DictionaryEqual(other.CalendarEntries,
                   EqualityComparer<IList<TimeSlot>>.Create((x, y) => x!.SequenceEqual(y!)));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ResourceId,
            CalendarEntries.CalculateHashCode(x => x.CalculateHashCode()));
    }
}