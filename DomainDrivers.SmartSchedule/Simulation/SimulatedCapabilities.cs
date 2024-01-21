using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Simulation;

public record SimulatedCapabilities(IList<AvailableResourceCapability> Capabilities)
{
    public static SimulatedCapabilities None()
    {
        return new SimulatedCapabilities(new List<AvailableResourceCapability>());
    }

    public SimulatedCapabilities Add(IList<AvailableResourceCapability> newCapabilities)
    {
        var newAvailabilities = new List<AvailableResourceCapability>(Capabilities);
        newAvailabilities.AddRange(newCapabilities);
        return new SimulatedCapabilities(newAvailabilities);
    }

    public SimulatedCapabilities Add(AvailableResourceCapability newCapability)
    {
        return Add(new List<AvailableResourceCapability> { newCapability });
    }

    public virtual bool Equals(SimulatedCapabilities? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Capabilities.SequenceEqual(other.Capabilities);
    }

    public override int GetHashCode()
    {
        return Capabilities.CalculateHashCode();
    }
    
}