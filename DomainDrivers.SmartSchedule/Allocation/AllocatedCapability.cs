using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;

namespace DomainDrivers.SmartSchedule.Allocation;

public record AllocatedCapability(Guid ResourceId, Capability Capability, TimeSlot TimeSlot);
