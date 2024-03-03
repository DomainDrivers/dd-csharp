namespace DomainDrivers.SmartSchedule.Risk;

public enum RiskPeriodicCheckSagaStep
{
    FindAvailable,
    DoNothing,
    SuggestReplacement,
    NotifyAboutPossibleRisk,
    NotifyAboutDemandsSatisfied
}