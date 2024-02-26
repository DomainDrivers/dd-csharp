using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;

namespace DomainDrivers.SmartSchedule.Resource.Device;

public record DeviceId(Guid Id)
{
    public static DeviceId NewOne() 
    {
        return new DeviceId(Guid.NewGuid());
    }

    public AllocatableResourceId ToAllocatableResourceId()
    {
        return new AllocatableResourceId(Id);
    }
}