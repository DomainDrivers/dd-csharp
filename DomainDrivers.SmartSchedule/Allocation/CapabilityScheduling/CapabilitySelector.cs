using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public class CapabilitySelector
{
    public static CapabilitySelector CanPerformOneOf(ISet<Capability> capabilities)
    {
        return null!;
    }

    public static CapabilitySelector CanPerformAllAtTheTime(ISet<Capability> beingAnAdmin)
    {
        return null!;
    }

    public bool CanPerform(Capability capability)
    {
        return false;
    }

    public bool CanPerform(ISet<Capability> capabilities)
    {
        return false;
    }
}