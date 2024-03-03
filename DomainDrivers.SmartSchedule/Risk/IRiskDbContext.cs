using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Risk;

public interface IRiskDbContext
{
    public DbSet<RiskPeriodicCheckSaga> RiskPeriodicCheckSagas { get; }
}