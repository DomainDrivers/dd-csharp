using DomainDrivers.SmartSchedule.Optimization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Simulation;

public record AvailableResourceCapability
    (Guid ResourceId, Capability Capability, TimeSlot TimeSlot) : ICapacityDimension
{
    public bool Performs(Capability capability)
    {
        return Capability == capability;
    }
}