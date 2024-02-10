namespace DomainDrivers.SmartSchedule.Allocation;

public record ProjectAllocationsDemandsScheduled(
    Guid Uuid,
    ProjectAllocationsId ProjectAllocationsId,
    Demands MissingDemands,
    DateTime OccurredAt)
{
    public ProjectAllocationsDemandsScheduled(ProjectAllocationsId projectId, Demands missingDemands,
        DateTime occurredAt)
        : this(Guid.NewGuid(), projectId, missingDemands, occurredAt)
    {
    }
}