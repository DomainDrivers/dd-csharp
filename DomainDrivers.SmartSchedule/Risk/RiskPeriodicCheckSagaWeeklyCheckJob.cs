using Quartz;

namespace DomainDrivers.SmartSchedule.Risk;

public class RiskPeriodicCheckSagaWeeklyCheckJob : IJob
{
    private readonly RiskPeriodicCheckSagaDispatcher _riskPeriodicCheckSagaDispatcher;

    public RiskPeriodicCheckSagaWeeklyCheckJob(RiskPeriodicCheckSagaDispatcher riskPeriodicCheckSagaDispatcher)
    {
        _riskPeriodicCheckSagaDispatcher = riskPeriodicCheckSagaDispatcher;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _riskPeriodicCheckSagaDispatcher.HandleWeeklyCheck();
    }
}