using DomainDrivers.SmartSchedule.Shared;

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
}