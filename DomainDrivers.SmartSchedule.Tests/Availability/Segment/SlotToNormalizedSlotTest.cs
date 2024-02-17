using DomainDrivers.SmartSchedule.Availability.Segment;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Availability.Segment;

public class SlotToNormalizedSlotTest
{
    [Fact]
    public void HasNoEffectWhenSlotAlreadyNormalized()
    {
        //given
        var start = DateTime.Parse("2023-09-09T00:00:00Z");
        var end = DateTime.Parse("2023-09-09T01:00:00Z");
        var timeSlot = new TimeSlot(start, end);
        var oneHour = SegmentInMinutes.Of(60);

        //when
        var normalized = SlotToNormalizedSlot.Apply(timeSlot, oneHour);

        //then
        Assert.Equal(timeSlot, normalized);
    }

    [Fact]
    public void NormalizedToTheHour()
    {
        //given
        var start = DateTime.Parse("2023-09-09T00:10:00Z");
        var end = DateTime.Parse("2023-09-09T00:59:00Z");
        var timeSlot = new TimeSlot(start, end);
        var oneHour = SegmentInMinutes.Of(60);

        //when
        var normalized = SlotToNormalizedSlot.Apply(timeSlot, oneHour);

        //then
        Assert.Equal(DateTime.Parse("2023-09-09T00:00:00Z"), normalized.From);
        Assert.Equal(DateTime.Parse("2023-09-09T01:00:00Z"), normalized.To);
    }

    [Fact]
    public void NormalizedWhenShortSlotOverlappingTwoSegments()
    {
        //given
        var start = DateTime.Parse("2023-09-09T00:29:00Z");
        var end = DateTime.Parse("2023-09-09T00:31:00Z");
        var timeSlot = new TimeSlot(start, end);
        var oneHour = SegmentInMinutes.Of(60);

        //when
        var normalized = SlotToNormalizedSlot.Apply(timeSlot, oneHour);

        //then
        Assert.Equal(DateTime.Parse("2023-09-09T00:00:00Z"), normalized.From);
        Assert.Equal(DateTime.Parse("2023-09-09T01:00:00Z"), normalized.To);
    }

    [Fact]
    public void NoNormalizationWhenSlotStartsAtSegmentStart()
    {
        //given
        var start = DateTime.Parse("2023-09-09T00:15:00Z");
        var end = DateTime.Parse("2023-09-09T00:30:00Z");
        var timeSlot = new TimeSlot(start, end);
        var start2 = DateTime.Parse("2023-09-09T00:30:00Z");
        var end2 = DateTime.Parse("2023-09-09T00:45:00Z");
        var timeSlot2 = new TimeSlot(start2, end2);
        var fifteenMinutes = SegmentInMinutes.Of(15);

        //when
        var normalized = SlotToNormalizedSlot.Apply(timeSlot, fifteenMinutes);
        var normalized2 = SlotToNormalizedSlot.Apply(timeSlot2, fifteenMinutes);

        //then
        Assert.Equal(DateTime.Parse("2023-09-09T00:15:00Z"), normalized.From);
        Assert.Equal(DateTime.Parse("2023-09-09T00:30:00Z"), normalized.To);
        Assert.Equal(DateTime.Parse("2023-09-09T00:30:00Z"), normalized2.From);
        Assert.Equal(DateTime.Parse("2023-09-09T00:45:00Z"), normalized2.To);
    }
}