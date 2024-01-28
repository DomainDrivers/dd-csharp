using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning.Scheduling;

public class ScheduleBasedOnChosenResourcesAvailabilityCalculator
{
    public IDictionary<string, TimeSlot> Calculate(Calendars chosenResourcesCalendars, IList<Stage> stages)
    {
        var schedule = new Dictionary<string, TimeSlot>();

        foreach (var stage in stages)
        {
            var proposedSlot = FindSlotForStage(chosenResourcesCalendars, stage);

            if (proposedSlot == TimeSlot.Empty())
            {
                return new Dictionary<string, TimeSlot>();
            }

            schedule[stage.Name] = proposedSlot;
        }

        return schedule;
    }

    private TimeSlot FindSlotForStage(Calendars chosenResourcesCalendars, Stage stage)
    {
        var foundSlots = PossibleSlots(chosenResourcesCalendars, stage);

        if (foundSlots.Contains(TimeSlot.Empty()))
        {
            return TimeSlot.Empty();
        }

        var commonSlotForAllResources = FindCommonPartOfSlots(foundSlots);

        while (!IsSlotLongEnoughForStage(stage, commonSlotForAllResources))
        {
            commonSlotForAllResources = commonSlotForAllResources.Stretch(TimeSpan.FromDays(1));
        }

        return new TimeSlot(commonSlotForAllResources.From, commonSlotForAllResources.From.Add(stage.Duration));
    }

    private bool IsSlotLongEnoughForStage(Stage stage, TimeSlot slot)
    {
        return slot.Duration.CompareTo(stage.Duration) >= 0;
    }

    private TimeSlot FindCommonPartOfSlots(IList<TimeSlot> foundSlots)
    {
            return foundSlots
                .DefaultIfEmpty(TimeSlot.Empty())
                .Aggregate((a, b) => a.CommonPartWith(b));
    }

    private List<TimeSlot> PossibleSlots(Calendars chosenResourcesCalendars, Stage stage)
    {
        return stage.Resources
            .Select(resource =>
                chosenResourcesCalendars
                    .Get(resource)
                    .AvailableSlots()
                    .OrderBy(slot => slot.From)
                    .FirstOrDefault(slot => IsSlotLongEnoughForStage(stage, slot))
                ?? TimeSlot.Empty())
            .ToList();
    }
}