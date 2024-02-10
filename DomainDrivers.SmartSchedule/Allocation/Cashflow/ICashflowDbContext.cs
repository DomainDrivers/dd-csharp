using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public interface ICashflowDbContext
{
    public DbSet<Cashflow> Cashflows { get; }
}