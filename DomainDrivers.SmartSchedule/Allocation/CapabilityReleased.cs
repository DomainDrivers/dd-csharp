using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public record CapabilityReleased(
    Guid EventId,
    ProjectAllocationsId ProjectId,
    Demands MissingDemands,
    DateTime OccurredAt) : IPrivateEvent
{
    public CapabilityReleased(ProjectAllocationsId projectId, Demands missingDemands, DateTime occurredAt)
        : this(Guid.NewGuid(), projectId, missingDemands, occurredAt)
    {
    }
}