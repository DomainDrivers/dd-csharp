namespace DomainDrivers.SmartSchedule.Shared;

//to use if we don't want to expose DbSet from Entity Framework for entities with possible large number of records
public interface IBaseRepository<T, in TId>
{
    T? FindById(TId id);

    T GetReferenceById(TId id);

    bool ExistsById(TId id);

    void DeleteById(TId id);

    void Delete(T entity);

    long Count();

    void Save(T entity);

    IEnumerable<T> FindAllById(IEnumerable<TId> ids);

    void SaveAll(IEnumerable<T> entities);
}
