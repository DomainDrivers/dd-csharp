namespace DomainDrivers.SmartSchedule.Shared;

public record TimeSlot(DateTime From, DateTime To)
{
    public static TimeSlot Empty()
    {
        return new TimeSlot(DateTime.UnixEpoch, DateTime.UnixEpoch);
    }

    public static TimeSlot CreateDailyTimeSlotAtUtc(int year, int month, int day)
    {
        return CreateTimeSlotAtUtcOfDuration(year, month, day, TimeSpan.FromDays(1));
    }

    public static TimeSlot CreateTimeSlotAtUtcOfDuration(int year, int month, int day, TimeSpan duration)
    {
        var thisDay = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        return new TimeSlot(thisDay, thisDay.Add(duration));
    }

    public static TimeSlot CreateMonthlyTimeSlotAtUtc(int year, int month)
    {
        var startOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1);
        return new TimeSlot(startOfMonth, endOfMonth);
    }

    public bool OverlapsWith(TimeSlot other)
    {
        return !(From > other.To) && !(To < other.From);
    }

    public bool Within(TimeSlot other)
    {
        return From >= other.From && To <= other.To;
    }

    public IList<TimeSlot> LeftoverAfterRemovingCommonWith(TimeSlot other)
    {
        var result = new List<TimeSlot>();

        if (other == this)
        {
            return new List<TimeSlot>();
        }

        if (!other.OverlapsWith(this))
        {
            return new List<TimeSlot>() { this, other };
        }

        if (this == other)
        {
            return result;
        }

        if (From < other.From)
        {
            result.Add(new TimeSlot(From, other.From));
        }

        if (other.From < From)
        {
            result.Add(new TimeSlot(other.From, From));
        }

        if (To > other.To)
        {
            result.Add(new TimeSlot(other.To, To));
        }

        if (other.To > To)
        {
            result.Add(new TimeSlot(To, other.To));
        }

        return result;
    }

    public TimeSlot CommonPartWith(TimeSlot other)
    {
        if (!OverlapsWith(other))
        {
            return Empty();
        }

        var commonStart = From > other.From ? From : other.From;
        var commonEnd = To < other.To ? To : other.To;
        return new TimeSlot(commonStart, commonEnd);
    }

    public bool IsEmpty
    {
        get { return From == To; }
    }

    public TimeSpan Duration
    {
        get { return To - From; }
    }

    public TimeSlot Stretch(TimeSpan duration)
    {
        return new TimeSlot(From - duration, To + duration);
    }
}