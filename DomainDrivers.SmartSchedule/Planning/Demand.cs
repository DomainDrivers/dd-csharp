using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public record Demand(Capability Capability)
{
    public static Demand DemandFor(Capability capability)
    {
        return new Demand(capability);
    }
}