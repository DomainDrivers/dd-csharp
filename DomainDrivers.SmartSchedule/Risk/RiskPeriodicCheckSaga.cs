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
    public Demands MissingDemands { get; private set; }
    public Earnings? Earnings { get; private set; }
    public DateTime? Deadline { get; private set; }

    public bool AreDemandsSatisfied
    {
        get { return MissingDemands.All.Count == 0; }
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

    public RiskPeriodicCheckSaga(ProjectAllocationsId projectId)
    {
        _riskSagaId = RiskPeriodicCheckSagaId.NewOne();
        ProjectId = projectId;
        MissingDemands = Demands.None();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private RiskPeriodicCheckSaga()
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public RiskPeriodicCheckSagaStep? HandleMissingDemands(Demands missingDemands)
    {
        MissingDemands = missingDemands;

        if (AreDemandsSatisfied)
        {
            return RiskPeriodicCheckSagaStep.NotifyAboutDemandsSatisfied;
        }

        return RiskPeriodicCheckSagaStep.DoNothing;
    }
    
    public RiskPeriodicCheckSagaStep Handle(ProjectAllocationScheduled @event)
    {
        Deadline = @event.FromTo.To;
        return RiskPeriodicCheckSagaStep.DoNothing;
    }

    public RiskPeriodicCheckSagaStep Handle(EarningsRecalculated @event)
    {
        Earnings = @event.Earnings;
        return RiskPeriodicCheckSagaStep.DoNothing;
    }
    
    public RiskPeriodicCheckSagaStep Handle(ResourceTakenOver @event)
    {
        if (@event.OccurredAt > Deadline)
        {
            return RiskPeriodicCheckSagaStep.DoNothing;
        }

        return RiskPeriodicCheckSagaStep.NotifyAboutPossibleRisk;
    }

    public RiskPeriodicCheckSagaStep HandleWeeklyCheck(DateTime when)
    {
        if (Deadline == null || when > Deadline)
        {
            return RiskPeriodicCheckSagaStep.DoNothing;
        }

        if (AreDemandsSatisfied)
        {
            return RiskPeriodicCheckSagaStep.DoNothing;
        }

        var daysToDeadline = (Deadline.Value - when).TotalDays;

        if (daysToDeadline > UpcomingDeadlineAvailabilitySearch)
        {
            return RiskPeriodicCheckSagaStep.DoNothing;
        }

        if (daysToDeadline > UpcomingDeadlineReplacementSuggestion)
        {
            return RiskPeriodicCheckSagaStep.FindAvailable;
        }

        if (Earnings!.GreaterThan(RiskThresholdValue))
        {
            return RiskPeriodicCheckSagaStep.SuggestReplacement;
        }

        return RiskPeriodicCheckSagaStep.DoNothing;
    }
}