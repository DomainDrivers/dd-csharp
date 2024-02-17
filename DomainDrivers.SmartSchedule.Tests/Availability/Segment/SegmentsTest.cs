using DomainDrivers.SmartSchedule.Availability.Segment;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Availability.Segment;

public class SegmentsTest
{
    [Fact]
    public void UnitHasToBeMultipleOf15Minutes()
    {
        //expect
        Assert.Throws<ArgumentException>(() => SegmentInMinutes.Of(20));
        Assert.Throws<ArgumentException>(() => SegmentInMinutes.Of(18));
        Assert.Throws<ArgumentException>(() => SegmentInMinutes.Of(7));
        Assert.NotNull(SegmentInMinutes.Of(15));
        Assert.NotNull(SegmentInMinutes.Of(30));
        Assert.NotNull(SegmentInMinutes.Of(45));
    }

    [Fact]
    public void SplittingIntoSegmentsWhenThereIsNoLeftover()
    {
        //given
        var start = DateTime.Parse("2023-09-09T00:00:00Z");
        var end = DateTime.Parse("2023-09-09T01:00:00Z");
        var timeSlot = new TimeSlot(start, end);

        //when
        var segments = Segments.Split(timeSlot, SegmentInMinutes.Of(15));

        //then
        Assert.Equal(4, segments.Count);
        Assert.Equal(DateTime.Parse("2023-09-09T00:00:00Z"), segments[0].From);
        Assert.Equal(DateTime.Parse("2023-09-09T00:15:00Z"), segments[0].To);
        Assert.Equal(DateTime.Parse("2023-09-09T00:15:00Z"), segments[1].From);
        Assert.Equal(DateTime.Parse("2023-09-09T00:30:00Z"), segments[1].To);
        Assert.Equal(DateTime.Parse("2023-09-09T00:30:00Z"), segments[2].From);
        Assert.Equal(DateTime.Parse("2023-09-09T00:45:00Z"), segments[2].To);
        Assert.Equal(DateTime.Parse("2023-09-09T00:45:00Z"), segments[3].From);
        Assert.Equal(DateTime.Parse("2023-09-09T01:00:00Z"), segments[3].To);
    }

    [Fact]
    public void SplittingIntoSegmentsJustNormalizesIfChosenSegmentLargerThanPassedSlot()
    {
        //given
        var start = DateTime.Parse("2023-09-09T00:10:00Z");
        var end = DateTime.Parse("2023-09-09T01:00:00Z");
        var timeSlot = new TimeSlot(start, end);

        //when
        var segments = Segments.Split(timeSlot, SegmentInMinutes.Of(90));

        //then
        Assert.Single(segments);
        Assert.Equal(DateTime.Parse("2023-09-09T00:00:00Z"), segments[0].From);
        Assert.Equal(DateTime.Parse("2023-09-09T01:30:00Z"), segments[0].To);
    }

    [Fact]
    public void NormalizingATimeSlot()
    {
        //given
        var start = DateTime.Parse("2023-09-09T00:10:00Z");
        var end = DateTime.Parse("2023-09-09T01:00:00Z");
        var timeSlot = new TimeSlot(start, end);

        //when
        var segment = Segments.NormalizeToSegmentBoundaries(timeSlot, SegmentInMinutes.Of(90));

        //then
        Assert.Equal(DateTime.Parse("2023-09-09T00:00:00Z"), segment.From);
        Assert.Equal(DateTime.Parse("2023-09-09T01:30:00Z"), segment.To);
    }

    [Fact]
    public void SlotsAreNormalizedBeforeSplitting()
    {
        //given
        var start = DateTime.Parse("2023-09-09T00:10:00Z");
        var end = DateTime.Parse("2023-09-09T00:59:00Z");
        var timeSlot = new TimeSlot(start, end);
        var oneHour = SegmentInMinutes.Of(60);

        //when
        var segments = Segments.Split(timeSlot, oneHour);

        //then
        Assert.Single(segments);
        Assert.Equal(DateTime.Parse("2023-09-09T00:00:00Z"), segments[0].From);
        Assert.Equal(DateTime.Parse("2023-09-09T01:00:00Z"), segments[0].To);
    }
    
    [Fact]
    public void SplittingIntoSegmentsWithoutNormalization()
    {
        //given
        var start = DateTime.Parse("2023-09-09T00:00:00Z");
        var end = DateTime.Parse("2023-09-09T00:59:00Z");
        var timeSlot = new TimeSlot(start, end);

        //when
        var segments = SlotToSegments.Apply(timeSlot, SegmentInMinutes.Of(30));

        //then
        Assert.Equal(2, segments.Count);
        Assert.Equal(DateTime.Parse("2023-09-09T00:00:00Z"), segments[0].From);
        Assert.Equal(DateTime.Parse("2023-09-09T00:30:00Z"), segments[0].To);
        Assert.Equal(DateTime.Parse("2023-09-09T00:30:00Z"), segments[1].From);
        Assert.Equal(DateTime.Parse("2023-09-09T00:59:00Z"), segments[1].To);
    }
}