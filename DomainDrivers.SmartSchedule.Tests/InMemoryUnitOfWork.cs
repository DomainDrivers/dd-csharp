using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests;

public class InMemoryUnitOfWork : IUnitOfWork
{
    public async Task<T> InTransaction<T>(Func<Task<T>> operation)
    {
        return await operation();
    }

    public async Task InTransaction(Func<Task> operation)
    {
        await operation();
    }
}