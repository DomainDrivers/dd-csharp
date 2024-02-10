using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public class Project
{
    private decimal _earnings;
    private Demands _demands;
    private Allocations _allocations;

    public Project(Demands demands, decimal earnings)
    {
        _demands = demands;
        _earnings = earnings;
        _allocations = Allocations.None();
    }

    public Demands MissingDemands()
    {
        return _demands.MissingDemands(_allocations);
    }

    public decimal Earnings
    {
        get { return _earnings; }
    }

    public AllocatedCapability? Remove(AllocatedCapability capability, TimeSlot forSlot)
    {
        var toRemove = _allocations.Find(capability.AllocatedCapabilityId);

        if (toRemove == null)
        {
            return null;
        }

        _allocations = _allocations.Remove(capability.AllocatedCapabilityId, forSlot);
        return toRemove;
    }

    public Allocations Add(AllocatedCapability allocatedCapability)
    {
        _allocations = _allocations.Add(allocatedCapability);
        return _allocations;
    }
}