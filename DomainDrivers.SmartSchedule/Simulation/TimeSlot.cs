namespace DomainDrivers.SmartSchedule.Simulation;

public record TimeSlot(DateTime From, DateTime To)
{
    public static TimeSlot CreateDailyTimeSlotAtUtc(int year, int month, int day)
    {
        var thisDay = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        return new TimeSlot(thisDay, thisDay.AddDays(1));
    }

    public bool Within(TimeSlot other)
    {
        return From >= other.From && To <= other.To;
    }
}