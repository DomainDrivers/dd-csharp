namespace DomainDrivers.SmartSchedule.Resource.Device;

public record DeviceId(Guid Id)
{
    public static DeviceId NewOne() 
    {
        return new DeviceId(Guid.NewGuid());
    }
}