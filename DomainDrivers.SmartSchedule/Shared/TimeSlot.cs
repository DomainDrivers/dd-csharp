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

    public bool Within(TimeSlot other)
    {
        return From >= other.From && To <= other.To;
    }
}