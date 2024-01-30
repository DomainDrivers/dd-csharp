using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule;

public class UnitOfWork : IUnitOfWork
{
    private readonly SmartScheduleDbContext _dbContext;

    public UnitOfWork(SmartScheduleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> InTransaction<T>(Func<Task<T>> operation)
    {
        if (_dbContext.Database.CurrentTransaction != null)
        {
            return await operation();
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var result = await operation();

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task InTransaction(Func<Task> operation)
    {
        if (_dbContext.Database.CurrentTransaction != null)
        {
            await operation();
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await operation();

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}