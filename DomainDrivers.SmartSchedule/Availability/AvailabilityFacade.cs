using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Availability;

public class AvailabilityFacade
{
    //can start with an in-memory repository for the aggregate

    public async Task CreateResourceSlots(ResourceAvailabilityId resourceId, TimeSlot timeslot)
    {
        await Task.CompletedTask;
    }

    public async Task<bool> Block(ResourceAvailabilityId resourceId, TimeSlot timeSlot, Owner requester)
    {
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> Release(ResourceAvailabilityId resourceId, TimeSlot timeSlot, Owner requester)
    {
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> Disable(ResourceAvailabilityId resourceId, TimeSlot timeSlot, Owner requester)
    {
        await Task.CompletedTask;
        return true;
    }
}