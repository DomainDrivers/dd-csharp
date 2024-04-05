using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public record NotSatisfiedDemands(
    Guid Uuid,
    IDictionary<ProjectAllocationsId, Demands> MissingDemands,
    DateTime OccurredAt) : IPublishedEvent
{
    public NotSatisfiedDemands(IDictionary<ProjectAllocationsId, Demands> missingDemands, DateTime occuredAt) :
        this(Guid.NewGuid(), missingDemands, occuredAt)
    {
    }

    public static NotSatisfiedDemands ForOneProject(ProjectAllocationsId projectId, Demands scheduledDemands,
        DateTime occurredAt)
    {
        return new NotSatisfiedDemands(Guid.NewGuid(), new Dictionary<ProjectAllocationsId, Demands>()
        {
            { projectId, scheduledDemands }
        }, occurredAt);
    }

    public static NotSatisfiedDemands AllSatisfied(ProjectAllocationsId projectId, DateTime occurredAt)
    {
        return new NotSatisfiedDemands(Guid.NewGuid(), new Dictionary<ProjectAllocationsId, Demands>()
        {
            { projectId, Demands.None() }
        }, occurredAt);
    }
}