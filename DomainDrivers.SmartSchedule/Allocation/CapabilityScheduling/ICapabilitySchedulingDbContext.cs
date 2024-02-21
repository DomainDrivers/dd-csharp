using Microsoft.EntityFrameworkCore;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public interface ICapabilitySchedulingDbContext
{
    public DbSet<AllocatableCapability> AllocatableCapabilities { get; }
}