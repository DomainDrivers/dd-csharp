namespace DomainDrivers.SmartSchedule.Availability.Segment;

public record SegmentInMinutes(int Value)
{
    public static SegmentInMinutes Of(int minutes)
    {
        if (minutes <= 0)
        {
            throw new ArgumentException("SegmentInMinutesDuration must be positive");
        }

        if (minutes % Segments.DefaultSegmentDurationInMinutes != 0)
        {
            throw new ArgumentException($"SegmentInMinutesDuration must be a multiple of {Segments.DefaultSegmentDurationInMinutes}");
        }

        return new SegmentInMinutes(minutes);
    }

    public static SegmentInMinutes DefaultSegment()
    {
        return Of(Segments.DefaultSegmentDurationInMinutes);
    }
}