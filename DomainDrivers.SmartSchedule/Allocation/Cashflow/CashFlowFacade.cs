using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.Cashflow;

public class CashFlowFacade
{
    private readonly ICashflowRepository _cashflowRepository;
    private readonly IEventsPublisher _eventsPublisher;
    private readonly TimeProvider _timeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CashFlowFacade(ICashflowRepository cashflowRepository, IEventsPublisher eventsPublisher,
        TimeProvider timeProvider, IUnitOfWork unitOfWork)
    {
        _cashflowRepository = cashflowRepository;
        _eventsPublisher = eventsPublisher;
        _timeProvider = timeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task AddIncomeAndCost(ProjectAllocationsId projectId, Income income, Cost cost)
    {
        await _unitOfWork.InTransaction(async () =>
        {
            var cashflow = await _cashflowRepository.FindById(projectId);
            if (cashflow == null)
            {
                cashflow = new Cashflow(projectId);
                await _cashflowRepository.Add(cashflow);
            }

            cashflow.Update(income, cost);
            await _cashflowRepository.Update(cashflow);
            await _eventsPublisher.Publish(new EarningsRecalculated(projectId, cashflow.Earnings(),
                _timeProvider.GetUtcNow().DateTime));
        });
    }

    public async Task<Earnings> Find(ProjectAllocationsId projectId)
    {
        var byId = await _cashflowRepository.GetById(projectId);
        return byId.Earnings();
    }

    public async Task<IDictionary<ProjectAllocationsId, Earnings>> FindAllEarnings()
    {
        return (await _cashflowRepository.FindAll())
            .ToDictionary(x => x.ProjectId, x => x.Earnings());
    }
}