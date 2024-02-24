using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public record Allocations(ISet<AllocatedCapability> All)
{
    public static Allocations None()
    {
        return new Allocations(new HashSet<AllocatedCapability>());
    }

    public Allocations Add(AllocatedCapability newOne)
    {
        var all = new HashSet<AllocatedCapability>(All) { newOne };
        return new Allocations(all);
    }

    public Allocations Remove(AllocatableCapabilityId toRemove, TimeSlot slot)
    {
        var allocatedCapability = Find(toRemove);

        if (allocatedCapability == null)
        {
            return this;
        }

        return RemoveFromSlot(allocatedCapability, slot);
    }

    private Allocations RemoveFromSlot(AllocatedCapability allocatedCapability, TimeSlot slot)
    {
        var leftOvers = allocatedCapability.TimeSlot
            .LeftoverAfterRemovingCommonWith(slot)
            .Where(leftOver => leftOver.Within(allocatedCapability.TimeSlot))
            .Select(leftOver => new AllocatedCapability(allocatedCapability.AllocatedCapabilityId, allocatedCapability.Capability, leftOver))
            .ToHashSet();
        var newSlots = new HashSet<AllocatedCapability>(All);
        newSlots.Remove(allocatedCapability);
        newSlots.UnionWith(leftOvers);
        return new Allocations(newSlots);
    }

    public AllocatedCapability? Find(AllocatableCapabilityId allocatedCapabilityId)
    {
        return All.FirstOrDefault(ar => ar.AllocatedCapabilityId == allocatedCapabilityId);
    }

    public virtual bool Equals(Allocations? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return All.SetEquals(other.All);
    }

    public override int GetHashCode()
    {
        return All.CalculateHashCode();
    }
}