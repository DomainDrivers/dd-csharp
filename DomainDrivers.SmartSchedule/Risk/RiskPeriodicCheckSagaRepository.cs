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
    
    public async Task<RiskPeriodicCheckSaga> FindByProjectIdOrCreate(ProjectAllocationsId projectId)
    {
        var found = await FindByProjectId(projectId);
        if (found == null) {
            found = await Add(new RiskPeriodicCheckSaga(projectId));
        }
        return found;
    }
    
    public async Task<IList<RiskPeriodicCheckSaga>> FindByProjectIdInOrElseCreate(List<ProjectAllocationsId> interested)
    {
        var found = await FindByProjectIdIn(interested);
        var foundIds = found.Select(x => x.ProjectId).ToList();
        var missing = interested.Where(projectId => !foundIds.Contains(projectId))
            .Select(x => new RiskPeriodicCheckSaga(x)).ToList();
        foreach (var missingSaga in missing)
        {
            found.Add(await Add(missingSaga));
        }
        return found;
    }

    public async Task<IList<RiskPeriodicCheckSaga>> FindAll()
    {
        return await _riskDbContext.RiskPeriodicCheckSagas.ToListAsync();
    }
    
    public async Task<RiskPeriodicCheckSaga> Add(RiskPeriodicCheckSaga riskPeriodicCheckSaga)
    {
        return (await _riskDbContext.RiskPeriodicCheckSagas.AddAsync(riskPeriodicCheckSaga)).Entity;
    }
}