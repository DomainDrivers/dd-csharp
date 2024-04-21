using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Scheduling;

public class ScheduleBasedOnReferenceStageCalculator
{
    public IDictionary<string, TimeSlot> Calculate(Stage referenceStage,
        TimeSlot referenceStageProposedTimeSlot,
        ParallelStagesList parallelizedStages,
        IComparer<ParallelStages> comparing)
    {
        var all = parallelizedStages.AllSorted(comparing);
        var referenceStageIndex = FindReferenceStageIndex(referenceStage, all);

        if (referenceStageIndex == -1)
        {
            return new Dictionary<string, TimeSlot>();
        }

        var scheduleMap = new Dictionary<string, TimeSlot>();
        var stagesBeforeReference = all.Take(referenceStageIndex).ToList();
        var stagesAfterReference = all.Skip(referenceStageIndex + 1).ToList();

        CalculateStagesBeforeCritical(stagesBeforeReference, referenceStageProposedTimeSlot, scheduleMap);
        CalculateStagesAfterCritical(stagesAfterReference, referenceStageProposedTimeSlot, scheduleMap);
        CalculateStagesWithReferenceStage(all[referenceStageIndex], referenceStageProposedTimeSlot, scheduleMap);

        return scheduleMap;
    }

    private IDictionary<string, TimeSlot> CalculateStagesBeforeCritical(IList<ParallelStages> before,
        TimeSlot stageProposedTimeSlot,
        IDictionary<string, TimeSlot> scheduleMap)
    {
        var currentStart = stageProposedTimeSlot.From;

        for (int i = before.Count - 1; i >= 0; i--)
        {
            var currentStages = before[i];
            var stageDuration = currentStages.Duration;
            var start = currentStart - stageDuration;

            foreach (var stage in currentStages.Stages)
            {
                scheduleMap[stage.StageName] = new TimeSlot(start, start + stage.Duration);
            }

            currentStart = start;
        }

        return scheduleMap;
    }

    private IDictionary<string, TimeSlot> CalculateStagesAfterCritical(IList<ParallelStages> after,
        TimeSlot stageProposedTimeSlot,
        IDictionary<string, TimeSlot> scheduleMap)
    {
        var currentStart = stageProposedTimeSlot.To;

        foreach (var currentStages in after)
        {
            foreach (var stage in currentStages.Stages)
            {
                scheduleMap[stage.StageName] = new TimeSlot(currentStart, currentStart + stage.Duration);
            }

            currentStart += currentStages.Duration;
        }

        return scheduleMap;
    }

    private IDictionary<string, TimeSlot> CalculateStagesWithReferenceStage(ParallelStages stagesWithReference,
        TimeSlot stageProposedTimeSlot,
        IDictionary<string, TimeSlot> scheduleMap)
    {
        var currentStart = stageProposedTimeSlot.From;

        foreach (var stage in stagesWithReference.Stages)
        {
            scheduleMap[stage.StageName] = new TimeSlot(currentStart, currentStart + stage.Duration);
        }

        return scheduleMap;
    }

    private int FindReferenceStageIndex(Stage referenceStage, IList<ParallelStages> all)
    {
        var stagesWithTheReferenceStageWithProposedTimeIndex = -1;

        for (int i = 0; i < all.Count; i++)
        {
            var stages = all[i];
            var stagesNames = stages.Stages.Select(s => s.StageName).ToHashSet();

            if (stagesNames.Contains(referenceStage.StageName))
            {
                stagesWithTheReferenceStageWithProposedTimeIndex = i;
                break;
            }
        }

        return stagesWithTheReferenceStageWithProposedTimeIndex;
    }
}