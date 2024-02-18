using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Resource.Device;

public record DeviceSummary(DeviceId Id, string Model, ISet<Capability> Assets)
{
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Model, Assets.CalculateHashCode());
    }
    
    public virtual bool Equals(DeviceSummary? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Model == other.Model && Assets.SetEquals(other.Assets);
    }
}