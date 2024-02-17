using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Availability;

public record Calendars(IDictionary<ResourceId, Calendar> CalendarsDictionary)
{
    public static Calendars Of(params Calendar[] calendars)
    {
        var collect = calendars
            .ToDictionary(calendar => calendar.ResourceId, calendar => calendar);
        return new Calendars(collect);
    }

    public Calendar Get(ResourceId resourceId)
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