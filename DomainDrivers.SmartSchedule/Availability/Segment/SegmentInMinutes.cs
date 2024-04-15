namespace DomainDrivers.SmartSchedule.Availability.Segment;

public record SegmentInMinutes(int Value)
{
    public static SegmentInMinutes Of(int minutes, int slotDurationInMinutes)
    {
        if (minutes <= 0)
        {
            throw new ArgumentException("SegmentInMinutesDuration must be positive");
        }

        if (minutes < slotDurationInMinutes)
        {
            throw new ArgumentException($"SegmentInMinutesDuration must be at least {slotDurationInMinutes} minutes");
        }

        if (minutes % slotDurationInMinutes != 0)
        {
            throw new ArgumentException($"SegmentInMinutesDuration must be a multiple of {slotDurationInMinutes} minutes");
        }

        return new SegmentInMinutes(minutes);
    }

    public static SegmentInMinutes Of(int minutes)
    {
        return Of(minutes, Segments.DefaultSegmentDurationInMinutes);
    }

    public static SegmentInMinutes DefaultSegment()
    {
        return Of(Segments.DefaultSegmentDurationInMinutes);
    }
}