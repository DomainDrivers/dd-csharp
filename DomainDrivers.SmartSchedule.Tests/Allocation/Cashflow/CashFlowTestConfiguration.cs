using DomainDrivers.SmartSchedule.Allocation;
using DomainDrivers.SmartSchedule.Allocation.Cashflow;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Allocation.Cashflow;

public static class CashFlowTestConfiguration
{
    public static CashFlowFacade CashFlowFacade(IEventsPublisher eventsPublisher, TimeProvider timeProvider)
    {
        return new CashFlowFacade(new InMemoryCashflowRepository(), eventsPublisher, timeProvider, new InMemoryUnitOfWork());
    }
}

public class InMemoryCashflowRepository : ICashflowRepository
{
    private readonly Dictionary<ProjectAllocationsId, SmartSchedule.Allocation.Cashflow.Cashflow> _cashflows = new();

    public Task<SmartSchedule.Allocation.Cashflow.Cashflow?> FindById(ProjectAllocationsId projectId)
    {
        return Task.FromResult(_cashflows.GetValueOrDefault(projectId));
    }

    public Task<SmartSchedule.Allocation.Cashflow.Cashflow> GetById(ProjectAllocationsId projectId)
    {
        return Task.FromResult(_cashflows[projectId]);
    }

    public Task<SmartSchedule.Allocation.Cashflow.Cashflow> Add(SmartSchedule.Allocation.Cashflow.Cashflow cashflow)
    {
        _cashflows.Add(cashflow.ProjectId, cashflow);
        return Task.FromResult(cashflow);
    }

    public Task<SmartSchedule.Allocation.Cashflow.Cashflow> Update(SmartSchedule.Allocation.Cashflow.Cashflow cashflow)
    {
        _cashflows[cashflow.ProjectId] = cashflow;
        return Task.FromResult(cashflow);
    }

    public Task<IList<SmartSchedule.Allocation.Cashflow.Cashflow>> FindAll()
    {
        return Task.FromResult<IList<SmartSchedule.Allocation.Cashflow.Cashflow>>(_cashflows.Values.ToList());
    }
}