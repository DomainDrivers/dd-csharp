using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public class AllocatableCapabilityRepository
{
    private readonly ICapabilitySchedulingDbContext _capabilitySchedulingDbContext;

    public AllocatableCapabilityRepository(ICapabilitySchedulingDbContext capabilitySchedulingDbContext)
    {
        _capabilitySchedulingDbContext = capabilitySchedulingDbContext;
    }

    public async Task<IList<AllocatableCapability>> FindByCapabilityWithin(string name, string type, DateTime from,
        DateTime to)
    {
        return await _capabilitySchedulingDbContext.AllocatableCapabilities
            .FromSql(
                $"SELECT ac.* FROM allocatable_capabilities ac WHERE ac.capability ->> 'name' = {name} AND ac.capability ->> 'type' = {type} AND ac.from_date <= {from} and ac.to_date >= {to}")
            .ToListAsync();
    }

    public async Task<List<AllocatableCapability>> FindAllById(IList<AllocatableCapabilityId> allocatableCapabilityIds)
    {
        return await _capabilitySchedulingDbContext.AllocatableCapabilities
            .Where(x => allocatableCapabilityIds.Contains(x.Id))
            .ToListAsync();
    }

    public async Task SaveAll(IList<AllocatableCapability> allocatableResources)
    {
        await _capabilitySchedulingDbContext.AllocatableCapabilities.AddRangeAsync(allocatableResources);
    }
}