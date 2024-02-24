﻿using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

public record CapabilitySelector(ISet<Capability> Capabilities, SelectingPolicy SelectingPolicy)
{
    public static CapabilitySelector CanPerformAllAtTheTime(ISet<Capability> capabilities)
    {
        return new CapabilitySelector(capabilities, SelectingPolicy.AllSimultaneously);
    }

    public static CapabilitySelector CanPerformOneOf(ISet<Capability> capabilities)
    {
        return new CapabilitySelector(capabilities, SelectingPolicy.OneOfAll);
    }

    public bool CanPerform(Capability capability)
    {
        return Capabilities.Contains(capability);
    }

    public bool CanPerform(ISet<Capability> capabilities)
    {
        if (capabilities.Count == 1)
        {
            return new HashSet<Capability>(Capabilities).IsSupersetOf(capabilities);
        }

        return SelectingPolicy == SelectingPolicy.AllSimultaneously &&
               new HashSet<Capability>(Capabilities).IsSupersetOf(capabilities);
    }

    public virtual bool Equals(CapabilitySelector? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Capabilities.SetEquals(other.Capabilities)
               && SelectingPolicy == other.SelectingPolicy;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Capabilities.CalculateHashCode(), SelectingPolicy);
    }
}

public enum SelectingPolicy
{
    AllSimultaneously,
    OneOfAll
}