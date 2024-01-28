using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Scheduling;

public class ScheduleBasedOnStartDayCalculator
{
    public IDictionary<string, TimeSlot> Calculate(DateTime startDate, ParallelStagesList parallelizedStages, IComparer<ParallelStages> comparing)
    {
        var scheduleMap = new Dictionary<string, TimeSlot>();
        var currentStart = startDate;
        var allSorted = parallelizedStages.AllSorted(comparing);
        //TODO
        return scheduleMap;
    }
}