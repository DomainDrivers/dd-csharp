using DomainDrivers.SmartSchedule.Planning.Parallelization;

namespace DomainDrivers.SmartSchedule.Planning.Scheduling;

public class ScheduleBasedOnStartDayCalculator
{
    public IDictionary<string, TimeSlot> Calculate(DateTime startDate, ParallelStagesList parallelizedStages, IComparer<ParallelStages> comparing)
    {
        var scheduleMap = new Dictionary<string, TimeSlot>();
        var currentStart = startDate;
        var allSorted = parallelizedStages.AllSorted(comparing);

        foreach (var stages in allSorted)
        {
            var parallelizedStagesEnd = currentStart;

            foreach (var stage in stages.Stages)
            {
                var stageEnd = currentStart.Add(stage.Duration);
                scheduleMap.Add(stage.StageName, new TimeSlot(currentStart, stageEnd));

                if (stageEnd > parallelizedStagesEnd)
                {
                    parallelizedStagesEnd = stageEnd;
                }
            }

            currentStart = parallelizedStagesEnd;
        }

        return scheduleMap;
    }
}