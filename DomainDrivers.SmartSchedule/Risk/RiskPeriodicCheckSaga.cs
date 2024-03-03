using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Availability;

namespace DomainDrivers.SmartSchedule.Risk;

public class RiskPeriodicCheckSaga
{
    static readonly Earnings RiskThresholdValue = Earnings.Of(1000);
    static readonly int UpcomingDeadlineAvailabilitySearch = 30;
    static readonly int UpcomingDeadlineReplacementSuggestion = 15;

    private RiskPeriodicCheckSagaId _riskSagaId;
    private int _version;

    public ProjectAllocationsId ProjectId { get; }
    public Demands MissingDemands { get; }
    public Earnings? Earnings { get; }
    public DateTime? Deadline { get; }

    public bool AreDemandsSatisfied
    {
        get { return false; }
    }

    public RiskPeriodicCheckSaga(ProjectAllocationsId projectId, Demands missingDemands)
    {
        _riskSagaId = RiskPeriodicCheckSagaId.NewOne();
        ProjectId = projectId;
        MissingDemands = missingDemands;
    }

    public RiskPeriodicCheckSaga(ProjectAllocationsId projectId, Earnings earnings)
    {
        _riskSagaId = RiskPeriodicCheckSagaId.NewOne();
        ProjectId = projectId;
        MissingDemands = Demands.None();
        Earnings = earnings;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private RiskPeriodicCheckSaga() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public RiskPeriodicCheckSagaStep? Handle(EarningsRecalculated @event)
    {
        return default;
    }

    public RiskPeriodicCheckSagaStep? Handle(ProjectAllocationsDemandsScheduled @event)
    {
        return default;
    }

    public RiskPeriodicCheckSagaStep? Handle(ProjectAllocationScheduled @event)
    {
        return default;
    }

    public RiskPeriodicCheckSagaStep? Handle(ResourceTakenOver @event)
    {
        return default;
    }

    public RiskPeriodicCheckSagaStep? Handle(CapabilityReleased @event)
    {
        return default;
    }

    public RiskPeriodicCheckSagaStep? Handle(CapabilitiesAllocated @event)
    {
        return default;
    }

    public RiskPeriodicCheckSagaStep? HandleWeeklyCheck(DateTime when)
    {
        return default;
    }
}