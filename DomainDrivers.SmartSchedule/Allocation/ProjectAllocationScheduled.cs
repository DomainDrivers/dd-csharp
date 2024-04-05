using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public record ProjectAllocationScheduled(
    Guid Uuid,
    ProjectAllocationsId ProjectAllocationsId,
    TimeSlot FromTo,
    DateTime OccurredAt) : IPrivateEvent, IPublishedEvent
{
    public ProjectAllocationScheduled(ProjectAllocationsId projectId, TimeSlot FromTo,
        DateTime occurredAt)
        : this(Guid.NewGuid(), projectId, FromTo, occurredAt)
    {
    }
}