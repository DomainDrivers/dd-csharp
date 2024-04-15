using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Availability.Segment;

public static class Segments
{
    public const int DefaultSegmentDurationInMinutes = 60;

    public static IList<TimeSlot> Split(TimeSlot timeSlot, SegmentInMinutes unit)
    {
        var normalizedSlot = NormalizeToSegmentBoundaries(timeSlot, unit);
        return SlotToSegments.Apply(normalizedSlot, unit);
    }

    public static TimeSlot NormalizeToSegmentBoundaries(TimeSlot timeSlot, SegmentInMinutes unit)
    {
        return SlotToNormalizedSlot.Apply(timeSlot, unit);
    }
}