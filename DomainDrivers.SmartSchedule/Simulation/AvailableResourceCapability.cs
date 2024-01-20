namespace DomainDrivers.SmartSchedule.Simulation;

public record AvailableResourceCapability(Guid ResourceId, Capability Capability, TimeSlot TimeSlot)
{
    public bool Performs(Capability capability)
    {
        return Capability == capability;
    }
}