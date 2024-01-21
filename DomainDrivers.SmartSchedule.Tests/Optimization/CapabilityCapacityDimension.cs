using DomainDrivers.SmartSchedule.Optimization;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Tests.Optimization;

public record CapabilityCapacityDimension
    (Guid Uuid, string Id, string CapacityName, string CapacityType) : ICapacityDimension
{
    public CapabilityCapacityDimension(string id, string capacityName, string capacityType)
        : this(Guid.NewGuid(), id, capacityName, capacityType)
    {
    }
}

public record CapabilityWeightDimension(string Name, string Type) : IWeightDimension<CapabilityCapacityDimension>
{
    public bool IsSatisfiedBy(CapabilityCapacityDimension capacityDimension)
    {
        return capacityDimension.CapacityName == Name && capacityDimension.CapacityType == Type;
    }

    public bool IsSatisfiedBy(ICapacityDimension capacityDimension)
    {
        return IsSatisfiedBy((CapabilityCapacityDimension)capacityDimension);
    }
}

public record CapabilityTimedCapacityDimension(Guid Uuid, string Id, string CapacityName, string CapacityType,
    TimeSlot TimeSlot) : ICapacityDimension
{
    public CapabilityTimedCapacityDimension(string id, string capacityName, string capacityType, TimeSlot timeSlot)
        : this(Guid.NewGuid(), id, capacityName, capacityType, timeSlot)
    {
    }
}

public record CapabilityTimedWeightDimension
    (string Name, string Type, TimeSlot TimeSlot) : IWeightDimension<CapabilityTimedCapacityDimension>
{
    public bool IsSatisfiedBy(CapabilityTimedCapacityDimension capacityTimedDimension)
    {
        return capacityTimedDimension.CapacityName == Name &&
               capacityTimedDimension.CapacityType == Type &&
               TimeSlot.Within(capacityTimedDimension.TimeSlot);
    }

    public bool IsSatisfiedBy(ICapacityDimension capacityTimedDimension)
    {
        return IsSatisfiedBy((CapabilityTimedCapacityDimension)capacityTimedDimension);
    }
}