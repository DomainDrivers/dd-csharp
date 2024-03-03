namespace DomainDrivers.SmartSchedule.Risk;

public record RiskPeriodicCheckSagaId(Guid Id)
{
    public static RiskPeriodicCheckSagaId NewOne()
    {
        return new RiskPeriodicCheckSagaId(Guid.NewGuid());
    }
}