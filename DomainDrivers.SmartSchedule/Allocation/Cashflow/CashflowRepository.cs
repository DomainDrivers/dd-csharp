using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public class CashflowRepository : ICashflowRepository
{
    private readonly ICashflowDbContext _cashflowDbContext;

    public CashflowRepository(ICashflowDbContext cashflowDbContext)
    {
        _cashflowDbContext = cashflowDbContext;
    }

    public async Task<Cashflow?> FindById(ProjectAllocationsId projectId)
    {
        return await _cashflowDbContext.Cashflows
            .SingleOrDefaultAsync(x => x.ProjectId == projectId);
    }

    public async Task<Cashflow> GetById(ProjectAllocationsId projectId)
    {
        return await _cashflowDbContext.Cashflows
            .SingleAsync(x => x.ProjectId == projectId);
    }

    public async Task<Cashflow> Add(Cashflow cashflow)
    {
        return (await _cashflowDbContext.Cashflows.AddAsync(cashflow)).Entity;
    }

    public Task<Cashflow> Update(Cashflow cashflow)
    {
        return Task.FromResult(_cashflowDbContext.Cashflows.Update(cashflow).Entity);
    }

    public async Task<IList<Cashflow>> FindAll()
    {
        return await _cashflowDbContext.Cashflows.ToListAsync();
    }
}