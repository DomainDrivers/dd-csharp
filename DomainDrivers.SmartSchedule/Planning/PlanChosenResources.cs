using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning.Parallelization;
using DomainDrivers.SmartSchedule.Planning.Scheduling;
using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Planning;

public class PlanChosenResources
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly AvailabilityFacade _availabilityFacade;
    private readonly IUnitOfWork _unitOfWork;

    public PlanChosenResources(IPlanningDbContext planningDbContext, AvailabilityFacade availabilityFacade, IUnitOfWork unitOfWork)
    {
        _planningDbContext = planningDbContext;
        _availabilityFacade = availabilityFacade;
        _unitOfWork = unitOfWork;
    }

    public async Task DefineResourcesWithinDates(ProjectId projectId, ISet<ResourceName> chosenResources,
        TimeSlot timeBoundaries)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            project.AddChosenResources(new ChosenResources(chosenResources, timeBoundaries));
        });
    }

    public async Task AdjustStagesToResourceAvailability(ProjectId projectId, TimeSlot timeBoundaries,
        params Stage[] stages)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var neededResources = NeededResources(stages);
            var project = await _planningDbContext.Projects.SingleAsync(x => x.Id == projectId);
            await DefineResourcesWithinDates(projectId, neededResources, timeBoundaries);
            //TODO when availability is implemented
            var neededResourcesCalendars = _availabilityFacade.AvailabilitiesOfResources();
            var schedule = CreateScheduleAdjustingToCalendars(neededResourcesCalendars, stages.ToList());
            project.AddSchedule(schedule);
        });
    }

    private Schedule CreateScheduleAdjustingToCalendars(Calendars neededResourcesCalendars,
        IList<Stage> stages)
    {
        return Schedule.BasedOnChosenResourcesAvailability(neededResourcesCalendars, stages);
    }

    private ISet<ResourceName> NeededResources(Stage[] stages)
    {
        return stages.SelectMany(stage => stage.Resources).ToHashSet();
    }
}