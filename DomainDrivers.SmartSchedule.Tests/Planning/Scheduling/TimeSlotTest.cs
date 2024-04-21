using DomainDrivers.SmartSchedule.Planning.Scheduling;

namespace DomainDrivers.SmartSchedule.Tests.Planning.Scheduling;

public class TimeSlotTest
{
    [Fact]
    public void SlotsOverlapping()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2022-01-01T00:00:00Z"), DateTime.Parse("2022-01-10T00:00:00Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2022-01-05T00:00:00Z"), DateTime.Parse("2022-01-15T00:00:00Z"));
        var slot3 = new TimeSlot(DateTime.Parse("2022-01-10T00:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));
        var slot4 = new TimeSlot(DateTime.Parse("2022-01-05T00:00:00Z"), DateTime.Parse("2022-01-10T00:00:00Z"));
        var slot5 = new TimeSlot(DateTime.Parse("2022-01-01T00:00:00Z"), DateTime.Parse("2022-01-10T00:00:00Z"));

        //expect
        Assert.True(slot1.OverlapsWith(slot2));
        Assert.True(slot1.OverlapsWith(slot1));
        Assert.True(slot1.OverlapsWith(slot3));
        Assert.True(slot1.OverlapsWith(slot4));
        Assert.True(slot1.OverlapsWith(slot5));
    }

    [Fact]
    public void SlotsNotOverlapping()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2022-01-01T00:00:00Z"), DateTime.Parse("2022-01-10T00:00:00Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2022-01-10T01:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));
        var slot3 = new TimeSlot(DateTime.Parse("2022-01-11T00:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));

        //expect
        Assert.False(slot1.OverlapsWith(slot2));
        Assert.False(slot1.OverlapsWith(slot3));
    }

    [Fact]
    public void TwoSlotsHaveCommonPartWhenSlotsOverlap()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2022-01-01T00:00:00Z"), DateTime.Parse("2022-01-15T00:00:00Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2022-01-10T00:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));

        //when
        var common = slot1.CommonPartWith(slot2);

        //then
        Assert.False(common.IsEmpty);
        Assert.Equal(DateTime.Parse("2022-01-10T00:00:00Z"), common.From);
        Assert.Equal(DateTime.Parse("2022-01-15T00:00:00Z"), common.To);
    }

    [Fact]
    public void TwoSlotsHaveCommonPartWhenFullOverlap()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2022-01-10T00:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2022-01-10T00:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));

        //when
        var common = slot1.CommonPartWith(slot2);

        //then
        Assert.False(common.IsEmpty);
        Assert.Equal(slot1, common);
    }

    [Fact]
    public void StretchTimeSlot()
    {
        // Arrange
        var initialFrom = DateTime.Parse("2022-01-01T10:00:00Z");
        var initialTo = DateTime.Parse("2022-01-01T12:00:00Z");
        var timeSlot = new TimeSlot(initialFrom, initialTo);

        // Act
        var stretchedSlot = timeSlot.Stretch(TimeSpan.FromHours(1));

        // Assert
        Assert.Equal(DateTime.Parse("2022-01-01T09:00:00Z"), stretchedSlot.From);
        Assert.Equal(DateTime.Parse("2022-01-01T13:00:00Z"), stretchedSlot.To);
    }
}