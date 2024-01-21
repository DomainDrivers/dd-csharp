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

    public Allocations Remove(AllocatedCapability toRemove, TimeSlot slot)
    {
        var allocatedCapability = Find(toRemove, slot);

        if (allocatedCapability == null)
        {
            return this;
        }

        return RemoveFromSlot(allocatedCapability, slot);
    }

    private Allocations RemoveFromSlot(AllocatedCapability allocatedResource, TimeSlot slot)
    {
        var leftOvers = allocatedResource.TimeSlot
            .LeftoverAfterRemovingCommonWith(slot)
            .Where(leftOver => leftOver.Within(allocatedResource.TimeSlot))
            .Select(leftOver => new AllocatedCapability(allocatedResource.ResourceId, allocatedResource.Capability, leftOver))
            .ToHashSet();
        var newSlots = new HashSet<AllocatedCapability>(All);
        newSlots.Remove(allocatedResource);
        newSlots.UnionWith(leftOvers);
        return new Allocations(newSlots);
    }

    public AllocatedCapability? Find(AllocatedCapability capability, TimeSlot timeSlot)
    {
        return All.FirstOrDefault(ar => ar == capability);
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