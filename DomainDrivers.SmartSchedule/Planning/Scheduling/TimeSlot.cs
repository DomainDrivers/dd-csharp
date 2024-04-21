namespace DomainDrivers.SmartSchedule.Planning.Scheduling;

public record TimeSlot(DateTime From, DateTime To)
{
    public static TimeSlot Empty()
    {
        return new TimeSlot(DateTime.UnixEpoch, DateTime.UnixEpoch);
    }

    public bool OverlapsWith(TimeSlot other)
    {
        return !(From > other.To) && !(To < other.From);
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