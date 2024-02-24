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
        return await _capabilitySchedulingDbContext.AllocatableCapabilities.FromSql(
                $"""
                 SELECT ac.*
                 FROM allocatable_capabilities ac
                 CROSS JOIN LATERAL jsonb_array_elements(ac.possible_capabilities -> 'capabilities') AS o(obj)
                 WHERE
                     o.obj ->> 'name' = {name}
                     AND o.obj ->> 'type' = {type}
                     AND ac.from_date <= {from}
                     AND ac.to_date >= {to}
                 """)
            .ToListAsync();
    }

    public async Task<AllocatableCapability?> FindByResourceIdAndCapabilityAndTimeSlot(Guid allocatableResourceId,
        string name, string type, DateTime from, DateTime to)
    {
        return await _capabilitySchedulingDbContext.AllocatableCapabilities.FromSql(
                $"""
                 SELECT ac.*
                 FROM allocatable_capabilities ac
                 CROSS JOIN LATERAL jsonb_array_elements(ac.possible_capabilities -> 'capabilities') AS o(obj)
                 WHERE
                     ac.resource_id = {allocatableResourceId}
                     AND o.obj ->> 'name' = {name}
                     AND o.obj ->> 'type' = {type}
                     AND ac.from_date = {from}
                     AND ac.to_date = {to}
                 """)
            .SingleOrDefaultAsync();
    }

    public async Task<IList<AllocatableCapability>> FindByResourceIdAndTimeSlot(Guid allocatableResourceId,
        DateTime from, DateTime to)
    {
        return await _capabilitySchedulingDbContext.AllocatableCapabilities.FromSql(
                $"""
                 SELECT ac.*
                 FROM allocatable_capabilities ac
                 WHERE
                     ac.resource_id = {allocatableResourceId}
                     AND ac.from_date = {from}
                     AND ac.to_date = {to}
                 """)
            .ToListAsync();
    }

    public async Task<IList<AllocatableCapability>> FindAllById(IList<AllocatableCapabilityId> allocatableCapabilityIds)
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