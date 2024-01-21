namespace DomainDrivers.SmartSchedule.Optimization;

public interface IWeightDimension
{
    bool IsSatisfiedBy(ICapacityDimension capacityDimension);
}

public interface IWeightDimension<in T> : IWeightDimension where T : ICapacityDimension
{
    bool IsSatisfiedBy(T capacityDimension);
}