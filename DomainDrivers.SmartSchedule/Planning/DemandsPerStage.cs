using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public record DemandsPerStage(IDictionary<string, Demands> Demands)
{
    public static DemandsPerStage Empty()
    {
        return new DemandsPerStage(new Dictionary<string, Demands>());
    }

    public virtual bool Equals(DemandsPerStage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Demands.DictionaryEqual(other.Demands);
    }

    public override int GetHashCode()
    {
        return Demands.CalculateHashCode();
    }
}