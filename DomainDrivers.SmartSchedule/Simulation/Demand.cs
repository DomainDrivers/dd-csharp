using DomainDrivers.SmartSchedule.Optimization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Simulation;

public record Demand(Capability Capability, TimeSlot Slot) : IWeightDimension<AvailableResourceCapability>
{
    public static Demand DemandFor(Capability capability, TimeSlot slot)
    {
        return new Demand(capability, slot);
    }

    public bool IsSatisfiedBy(ICapacityDimension capacityDimension)
    {
        return IsSatisfiedBy((AvailableResourceCapability)capacityDimension);
    }

    public bool IsSatisfiedBy(AvailableResourceCapability availableCapability)
    {
        return availableCapability.Performs(Capability) &&
               Slot.Within(availableCapability.TimeSlot);
    }
}