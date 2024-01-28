using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Scheduling;

public record Schedule(IDictionary<string, TimeSlot> Dates)
{
    public static Schedule None()
    {
        return new Schedule(new Dictionary<string, TimeSlot>());
    }

    public static Schedule BasedOnStartDay(DateTime startDate, ParallelStagesList parallelizedStages)
    {
        var scheduleMap = new ScheduleBasedOnStartDayCalculator().Calculate(startDate, parallelizedStages,
            Comparer<ParallelStages>.Create((x, y) => x.Print().CompareTo(y.Print())));
        return new Schedule(scheduleMap);
    }

    public static Schedule BasedOnReferenceStageTimeSlot(Stage referenceStage, TimeSlot stageProposedTimeSlot,
        ParallelStagesList parallelizedStages)
    {
        var scheduleMap = new ScheduleBasedOnReferenceStageCalculator().Calculate(referenceStage, stageProposedTimeSlot,
            parallelizedStages, Comparer<ParallelStages>.Create((x, y) => x.Print().CompareTo(y.Print())));
        return new Schedule(scheduleMap);
    }

    public static Schedule BasedOnChosenResourcesAvailability(Calendars chosenResourcesCalendars, IList<Stage> stages)
    {
        var schedule =
            new ScheduleBasedOnChosenResourcesAvailabilityCalculator().Calculate(chosenResourcesCalendars, stages);
        return new Schedule(schedule);
    }

    public virtual bool Equals(Schedule? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Dates.DictionaryEqual(other.Dates);
    }

    public override int GetHashCode()
    {
        return Dates.CalculateHashCode();
    }
}