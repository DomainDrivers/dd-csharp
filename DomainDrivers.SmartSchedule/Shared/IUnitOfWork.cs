namespace DomainDrivers.SmartSchedule.Shared;

public interface IUnitOfWork
{
    Task<T> InTransaction<T>(Func<Task<T>> operation);
    Task InTransaction(Func<Task> operation);
}