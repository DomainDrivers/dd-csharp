using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public record CapabilitiesDemanded(Guid Uuid, ProjectId ProjectId, Demands Demands, DateTime OccurredAt) : IPublishedEvent
{
    public CapabilitiesDemanded(ProjectId projectId, Demands demands, DateTime occurredAt) : this(Guid.NewGuid(),
        projectId, demands, occurredAt) { }
}