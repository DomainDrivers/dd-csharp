using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Resource.Device;

public class Device
{
    public DeviceId Id { get; private set; } = DeviceId.NewOne();
    
    private int _version;
    
    public string Model { get; private set; }
    
    public ISet<Capability> Capabilities { get; private set; }

    public Device(DeviceId id, string model, ISet<Capability> capabilities)
    {
        Id = id;
        Model = model;
        Capabilities = capabilities;
    }
}