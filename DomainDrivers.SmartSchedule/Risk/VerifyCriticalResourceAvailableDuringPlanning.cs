using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Planning;
using DomainDrivers.SmartSchedule.Shared;
using MediatR;

namespace DomainDrivers.SmartSchedule.Risk;

public class VerifyCriticalResourceAvailableDuringPlanning : INotificationHandler<CriticalStagePlanned>
{
    private readonly AvailabilityFacade _availabilityFacade;
    private readonly IRiskPushNotification _riskPushNotification;

    public VerifyCriticalResourceAvailableDuringPlanning(AvailabilityFacade availabilityFacade,
        IRiskPushNotification riskPushNotification)
    {
        _availabilityFacade = availabilityFacade;
        _riskPushNotification = riskPushNotification;
    }

    public async Task Handle(CriticalStagePlanned criticalStagePlanned, CancellationToken cancellationToken)
    {
        if (criticalStagePlanned.CriticalResource == null)
        {
            return;
        }

        var calendar =
            await _availabilityFacade.LoadCalendar(criticalStagePlanned.CriticalResource,
                criticalStagePlanned.StageTimeSlot);

        if (!ResourceIsAvailable(criticalStagePlanned.StageTimeSlot, calendar))
        {
            _riskPushNotification.NotifyAboutCriticalResourceNotAvailable(criticalStagePlanned.ProjectId,
                criticalStagePlanned.CriticalResource, criticalStagePlanned.StageTimeSlot);
        }
    }

    private bool ResourceIsAvailable(TimeSlot timeSlot, Calendar calendar)
    {
        return calendar.AvailableSlots().Any(slot => slot == timeSlot);
    }
}