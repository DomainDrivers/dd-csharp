namespace DomainDrivers.SmartSchedule.Shared;

public record TimeSlot(DateTime From, DateTime To)
{
    public static TimeSlot CreateDailyTimeSlotAtUtc(int year, int month, int day)
    {
        var thisDay = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        return new TimeSlot(thisDay, thisDay.AddDays(1));
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
}