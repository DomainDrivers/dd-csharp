using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Planning.Scheduling;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public class PlanChosenResources
{
    private readonly IProjectRepository _projectRepository;
    private readonly IAvailabilityFacade _availabilityFacade;
    private readonly IEventsPublisher _eventsPublisher;
    private readonly TimeProvider _timeProvider;

    public PlanChosenResources(IProjectRepository projectRepository, IAvailabilityFacade availabilityFacade,
        IEventsPublisher eventsPublisher, TimeProvider timeProvider)
    {
        _projectRepository = projectRepository;
        _availabilityFacade = availabilityFacade;
        _eventsPublisher = eventsPublisher;
        _timeProvider = timeProvider;
    }

    public async Task DefineResourcesWithinDates(ProjectId projectId, ISet<ResourceId> chosenResources,
        TimeSlot timeBoundaries)
    {
        var project = await _projectRepository.GetById(projectId);
        project.AddChosenResources(new ChosenResources(chosenResources, timeBoundaries));
        await _projectRepository.Save(project);
        await _eventsPublisher.Publish(new NeededResourcesChosen(projectId, chosenResources, timeBoundaries,
            _timeProvider.GetUtcNow().DateTime));
    }

    public async Task AdjustStagesToResourceAvailability(ProjectId projectId, TimeSlot timeBoundaries,
        params Stage[] stages)
    {
        var neededResources = NeededResources(stages);
        var project = await _projectRepository.GetById(projectId);
        await DefineResourcesWithinDates(projectId, neededResources, timeBoundaries);
        var neededResourcesCalendars = await _availabilityFacade.LoadCalendars(neededResources, timeBoundaries);
        var schedule = CreateScheduleAdjustingToCalendars(neededResourcesCalendars, stages.ToList());
        project.AddSchedule(schedule);
        await _projectRepository.Save(project);
    }

    private Schedule CreateScheduleAdjustingToCalendars(Calendars neededResourcesCalendars,
        IList<Stage> stages)
    {
        return Schedule.BasedOnChosenResourcesAvailability(neededResourcesCalendars, stages);
    }

    private ISet<ResourceId> NeededResources(Stage[] stages)
    {
        return stages.SelectMany(stage => stage.Resources).ToHashSet();
    }
}