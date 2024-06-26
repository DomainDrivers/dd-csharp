﻿using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public record ProjectAllocationsDemandsScheduled(
    Guid Uuid,
    ProjectAllocationsId ProjectAllocationsId,
    Demands MissingDemands,
    DateTime OccurredAt) : IPrivateEvent
{
    public ProjectAllocationsDemandsScheduled(ProjectAllocationsId projectId, Demands missingDemands,
        DateTime occurredAt)
        : this(Guid.NewGuid(), projectId, missingDemands, occurredAt)
    {
    }
}