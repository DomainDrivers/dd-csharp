using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Shared;
using MediatR;

namespace DomainDrivers.SmartSchedule.Risk;

public class VerifyNeededResourcesAvailableInTimeSlot : INotificationHandler<NeededResourcesChosen>
{
    private readonly AvailabilityFacade _availabilityFacade;
    private readonly IRiskPushNotification _riskPushNotification;

    public VerifyNeededResourcesAvailableInTimeSlot(AvailabilityFacade availabilityFacade,
        IRiskPushNotification riskPushNotification)
    {
        _availabilityFacade = availabilityFacade;
        _riskPushNotification = riskPushNotification;
    }


    public async Task Handle(NeededResourcesChosen resourcesNeeded, CancellationToken cancellationToken)
    {
        await NotifyAboutNotAvailableResources(resourcesNeeded.NeededResources, resourcesNeeded.TimeSlot,
            resourcesNeeded.ProjectId);
    }

    private async Task NotifyAboutNotAvailableResources(ISet<ResourceId> resourcedIds, TimeSlot timeSlot,
        ProjectId projectId)
    {
        var notAvailable = new HashSet<ResourceId>();
        var calendars = await _availabilityFacade.LoadCalendars(resourcedIds, timeSlot);

        foreach (var resourceId in resourcedIds)
        {
            if (calendars.Get(resourceId).AvailableSlots().Any(x => timeSlot.Within(x) == false))
            {
                notAvailable.Add(resourceId);
            }
        }

        if (notAvailable.Any())
        {
            _riskPushNotification.NotifyAboutResourcesNotAvailable(projectId, notAvailable);
        }
    }
}