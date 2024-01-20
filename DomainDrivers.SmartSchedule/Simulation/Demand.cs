namespace DomainDrivers.SmartSchedule.Simulation;

public record Demand(Capability Capability, TimeSlot Slot)
{
    public static Demand DemandFor(Capability capability, TimeSlot slot)
    {
        return new Demand(capability, slot);
    }

    public bool IsSatisfiedBy(AvailableResourceCapability availableCapability)
    {
        return availableCapability.Performs(Capability) &&
               Slot.Within(availableCapability.TimeSlot);
    }
}