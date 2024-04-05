namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public interface ICashflowRepository
{
    Task<Cashflow?> FindById(ProjectAllocationsId projectId);

    Task<Cashflow> GetById(ProjectAllocationsId projectId);

    Task<Cashflow> Add(Cashflow cashflow);

    Task<Cashflow> Update(Cashflow cashflow);

    Task<IList<Cashflow>> FindAll();
}