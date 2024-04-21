using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Scheduling;

//those classes will be part of another module - possibly "availability"
public record Calendars(IDictionary<ResourceName, Calendar> CalendarsDictionary)
{
    public static Calendars Of(params Calendar[] calendars)
    {
        var collect = calendars
            .ToDictionary(calendar => calendar.ResourceId, calendar => calendar);
        return new Calendars(collect);
    }

    public Calendar Get(ResourceName resourceId)
    {
        if (CalendarsDictionary.TryGetValue(resourceId, out var calendar))
        {
            return calendar;
        }

        return Calendar.Empty(resourceId);
    }

    public virtual bool Equals(Calendars? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return CalendarsDictionary.DictionaryEqual(other.CalendarsDictionary);
    }

    public override int GetHashCode()
    {
        return CalendarsDictionary.CalculateHashCode();
    }
}

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

public record Owner(Guid? OwnerId)
{
    public static Owner None()
    {
        return new Owner((Guid?)null);
    }
}