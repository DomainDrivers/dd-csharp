namespace DomainDrivers.SmartSchedule.Optimization;

public record Item(string Name, double Value, TotalWeight TotalWeight)
{
    public bool IsWeightZero
    {
        get { return TotalWeight.Components().Count == 0; }
    }
}