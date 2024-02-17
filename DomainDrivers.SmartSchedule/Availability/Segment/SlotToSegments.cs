using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Availability.Segment;

public static class SlotToSegments
{
    public static IList<TimeSlot> Apply(TimeSlot timeSlot, SegmentInMinutes duration)
    {
        var minimalSegment = new TimeSlot(timeSlot.From, timeSlot.From.AddMinutes(duration.Value));

        if (timeSlot.Within(minimalSegment))
        {
            return new List<TimeSlot> { minimalSegment };
        }

        var segmentInMinutesDuration = duration.Value;
        var numberOfSegments = CalculateNumberOfSegments(timeSlot, segmentInMinutesDuration);

        var segments = new List<TimeSlot>();

        for (long i = 0; i < numberOfSegments; i++)
        {
            var currentStart = timeSlot.From.AddMinutes(i * segmentInMinutesDuration);
            segments.Add(new TimeSlot(currentStart,
                CalculateEnd(segmentInMinutesDuration, currentStart, timeSlot.To)));
        }

        return segments;
    }

    private static long CalculateNumberOfSegments(TimeSlot timeSlot, int segmentInMinutesDuration)
    {
        return (long)Math.Ceiling((timeSlot.To - timeSlot.From).TotalMinutes / segmentInMinutesDuration);
    }

    private static DateTime CalculateEnd(int segmentInMinutesDuration, DateTime currentStart, DateTime initialEnd)
    {
        var segmentEnd = currentStart.AddMinutes(segmentInMinutesDuration);

        if (initialEnd < segmentEnd)
        {
            return initialEnd;
        }

        return segmentEnd;
    }
}