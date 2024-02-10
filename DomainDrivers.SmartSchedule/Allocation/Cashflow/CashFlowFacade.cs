using DomainDrivers.SmartSchedule.Shared;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public class CashFlowFacade
{
    private readonly ICashflowDbContext _cashflowDbContext;
    private readonly IUnitOfWork _unitOfWork;

    public CashFlowFacade(ICashflowDbContext cashflowDbContext, IUnitOfWork unitOfWork)
    {
        _cashflowDbContext = cashflowDbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task AddIncomeAndCost(ProjectAllocationsId projectId, Income income, Cost cost)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var cashflow = await _cashflowDbContext.Cashflows.FindAsync(projectId);
            if (cashflow == null)
            {
                cashflow = new Cashflow(projectId);
                await _cashflowDbContext.Cashflows.AddAsync(cashflow);
            }

            cashflow.Update(income, cost);
        });
    }

    public async Task<Earnings> Find(ProjectAllocationsId projectId)
    {
        var byId = await _cashflowDbContext.Cashflows.SingleAsync(x => x.ProjectId == projectId);
        return byId.Earnings();
    }
}