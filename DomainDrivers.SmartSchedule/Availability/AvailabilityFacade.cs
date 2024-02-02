namespace DomainDrivers.SmartSchedule.Availability;

public class AvailabilityFacade
{
    public Calendars AvailabilitiesOfResources()
    {
        return Calendars.Of();
    }
}