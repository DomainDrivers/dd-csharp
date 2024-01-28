using DomainDrivers.SmartSchedule.Shared;
using NUnit.Framework.Legacy;

namespace DomainDrivers.SmartSchedule.Tests.Shared;

public class TimeSlotTest
{
    [Fact]
    public void CreatingMonthlyTimeSlotAtUtc()
    {
        //when
        var january2023 = TimeSlot.CreateMonthlyTimeSlotAtUtc(2023, 1);

        //then
        Assert.Equal(january2023.From, new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(january2023.To, new DateTime(2023, 2, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void CreatingDailyTimeSlotAtUtc()
    {
        //when
        var specificDay = TimeSlot.CreateDailyTimeSlotAtUtc(2023, 1, 15);

        //then
        Assert.Equal(specificDay.From, new DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(specificDay.To, new DateTime(2023, 1, 16, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void OneSlotWithinAnother()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2023-01-02T00:00:00Z"), DateTime.Parse("2023-01-02T23:59:59Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2023-01-01T00:00:00Z"), DateTime.Parse("2023-01-03T00:00:00Z"));

        //expect
        Assert.True(slot1.Within(slot2));
        Assert.False(slot2.Within(slot1));
    }

    [Fact]
    public void OneSlotIsNotWithinAnotherIfTheyJustOverlap()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2023-01-01T00:00:00Z"), DateTime.Parse("2023-01-02T23:59:59Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2023-01-02T00:00:00Z"), DateTime.Parse("2023-01-03T00:00:00Z"));

        //expect
        Assert.False(slot1.Within(slot2));
        Assert.False(slot2.Within(slot1));

        //given
        var slot3 = new TimeSlot(DateTime.Parse("2023-01-02T00:00:00Z"), DateTime.Parse("2023-01-03T23:59:59Z"));
        var slot4 = new TimeSlot(DateTime.Parse("2023-01-01T00:00:00Z"), DateTime.Parse("2023-01-02T23:59:59Z"));

        //expect
        Assert.False(slot3.Within(slot4));
        Assert.False(slot4.Within(slot3));
    }

    [Fact]
    public void SlotIsNotWithinAnotherWhenTheyAreCompletelyOutside()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2023-01-01T00:00:00Z"), DateTime.Parse("2023-01-01T23:59:59Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2023-01-02T00:00:00Z"), DateTime.Parse("2023-01-03T00:00:00Z"));

        //expect
        Assert.False(slot1.Within(slot2));
    }

    [Fact]
    public void SlotIsWithinItself()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2023-01-01T00:00:00Z"), DateTime.Parse("2023-01-01T23:59:59Z"));

        //expect
        Assert.True(slot1.Within(slot1));
    }

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
    public void RemovingCommonPartsShouldHaveNoEffectWhenThereIsNoOverlap()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2022-01-01T00:00:00Z"), DateTime.Parse("2022-01-10T00:00:00Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2022-01-15T00:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));

        //expect
        CollectionAssert.IsSubsetOf(new List<TimeSlot> { slot1, slot2 }, slot1.LeftoverAfterRemovingCommonWith(slot2));
    }

    [Fact]
    public void RemovingCommonPartsWhenThereIsFullOverlap()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2022-01-01T00:00:00Z"), DateTime.Parse("2022-01-10T00:00:00Z"));

        //expect
        Assert.Empty(slot1.LeftoverAfterRemovingCommonWith(slot1));
    }

    [Fact]
    public void RemovingCommonPartsWhenThereIsOverlap()
    {
        //given
        var slot1 = new TimeSlot(DateTime.Parse("2022-01-01T00:00:00Z"), DateTime.Parse("2022-01-15T00:00:00Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2022-01-10T00:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));

        //when
        var difference = slot1.LeftoverAfterRemovingCommonWith(slot2);

        //then
        Assert.Equal(2, difference.Count);
        Assert.Equal(DateTime.Parse("2022-01-01T00:00:00Z"), difference[0].From);
        Assert.Equal(DateTime.Parse("2022-01-10T00:00:00Z"), difference[0].To);
        Assert.Equal(DateTime.Parse("2022-01-15T00:00:00Z"), difference[1].From);
        Assert.Equal(DateTime.Parse("2022-01-20T00:00:00Z"), difference[1].To);

        //given
        var slot3 = new TimeSlot(DateTime.Parse("2022-01-05T00:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));
        var slot4 = new TimeSlot(DateTime.Parse("2022-01-01T00:00:00Z"), DateTime.Parse("2022-01-10T00:00:00Z"));

        //when
        var difference2 = slot3.LeftoverAfterRemovingCommonWith(slot4);

        //then
        Assert.Equal(2, difference2.Count);
        Assert.Equal(DateTime.Parse("2022-01-01T00:00:00Z"), difference2[0].From);
        Assert.Equal(DateTime.Parse("2022-01-05T00:00:00Z"), difference2[0].To);
        Assert.Equal(DateTime.Parse("2022-01-10T00:00:00Z"), difference2[1].From);
        Assert.Equal(DateTime.Parse("2022-01-20T00:00:00Z"), difference2[1].To);
    }

    [Fact]
    public void RemovingCommonPartWhenOneSlotInFullyWithinAnother()
    {
        // given
        var slot1 = new TimeSlot(DateTime.Parse("2022-01-01T00:00:00Z"), DateTime.Parse("2022-01-20T00:00:00Z"));
        var slot2 = new TimeSlot(DateTime.Parse("2022-01-10T00:00:00Z"), DateTime.Parse("2022-01-15T00:00:00Z"));

        // when
        var difference = slot1.LeftoverAfterRemovingCommonWith(slot2);

        // then
        Assert.Equal(2, difference.Count);
        Assert.Equal(DateTime.Parse("2022-01-01T00:00:00Z"), difference[0].From);
        Assert.Equal(DateTime.Parse("2022-01-10T00:00:00Z"), difference[0].To);
        Assert.Equal(DateTime.Parse("2022-01-15T00:00:00Z"), difference[1].From);
        Assert.Equal(DateTime.Parse("2022-01-20T00:00:00Z"), difference[1].To);
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