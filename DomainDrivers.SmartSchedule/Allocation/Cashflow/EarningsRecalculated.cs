using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public record EarningsRecalculated(Guid Uuid, ProjectAllocationsId ProjectId, Earnings Earnings, DateTime OccurredAt)
    : IEvent
{
    public EarningsRecalculated(ProjectAllocationsId projectId, Earnings earnings, DateTime occurredAt)
        : this(Guid.NewGuid(), projectId, earnings, occurredAt) { }
}