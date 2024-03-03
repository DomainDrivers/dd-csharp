using DomainDrivers.SmartSchedule.Allocation;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Risk;

public class RiskPeriodicCheckSagaRepository
{
    private readonly IRiskDbContext _riskDbContext;

    public RiskPeriodicCheckSagaRepository(IRiskDbContext riskDbContext)
    {
        _riskDbContext = riskDbContext;
    }

    public async Task<RiskPeriodicCheckSaga?> FindByProjectId(ProjectAllocationsId projectId)
    {
        return await _riskDbContext.RiskPeriodicCheckSagas.SingleOrDefaultAsync(x => x.ProjectId == projectId);
    }

    public async Task<IList<RiskPeriodicCheckSaga>> FindByProjectIdIn(List<ProjectAllocationsId> interested)
    {
        return await _riskDbContext.RiskPeriodicCheckSagas
            .Where(x => interested.Contains(x.ProjectId))
            .ToListAsync();
    }

    public async Task<IList<RiskPeriodicCheckSaga>> FindAll()
    {
        return await _riskDbContext.RiskPeriodicCheckSagas.ToListAsync();
    }

    public async Task Add(RiskPeriodicCheckSaga riskPeriodicCheckSaga)
    {
        await _riskDbContext.RiskPeriodicCheckSagas.AddAsync(riskPeriodicCheckSaga);
    }
}