using DomainDrivers.SmartSchedule.Planning.Scheduling;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Planning.Scheduling.Assertions;

public class StageAssert
{
    private readonly TimeSlot _actual;
    private readonly ScheduleAssert _scheduleAssert;

    public StageAssert(TimeSlot actual, ScheduleAssert scheduleAssert)
    {
        _actual = actual;
        _scheduleAssert = scheduleAssert;
    }

    public StageAssert ThatStarts(string start)
    {
        Assert.Equal(DateTime.Parse(start), _actual.From);
        return this;
    }

    public StageAssert WithSlot(TimeSlot slot)
    {
        Assert.Equal(slot, _actual);
        return this;
    }

    public StageAssert ThatEnds(string end)
    {
        Assert.Equal(DateTime.Parse(end), _actual.To);
        return this;
    }

    public ScheduleAssert And()
    {
        return _scheduleAssert;
    }

    public StageAssert IsBefore(string stage)
    {
        var schedule = _scheduleAssert.Schedule;
        Assert.True(_actual.To <= schedule.Dates[stage].From);
        return this;
    }

    public StageAssert StartsTogetherWith(string stage)
    {
        var schedule = _scheduleAssert.Schedule;
        Assert.Equal(_actual.From, schedule.Dates[stage].From);
        return this;
    }

    public StageAssert IsAfter(string stage)
    {
        var schedule = _scheduleAssert.Schedule;
        Assert.True(_actual.From >= schedule.Dates[stage].To);
        return this;
    }
}