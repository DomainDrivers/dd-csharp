using DomainDrivers.SmartSchedule.Simulation;

namespace DomainDrivers.SmartSchedule.Tests.Simulation;

public class AvailableCapabilitiesBuilder
{
    private readonly IList<AvailableResourceCapability> _availabilities = new List<AvailableResourceCapability>();
    private Guid? _currentResourceId;
    private Capability? _capability;
    private TimeSlot? _timeSlot;

    public AvailableCapabilitiesBuilder WithEmployee(Guid id)
    {
        if (_currentResourceId.HasValue)
        {
            _availabilities.Add(new AvailableResourceCapability(_currentResourceId.Value, _capability!, _timeSlot!));
        }

        _currentResourceId = id;
        return this;
    }

    public AvailableCapabilitiesBuilder ThatBrings(Capability capability)
    {
        _capability = capability;
        return this;
    }

    public AvailableCapabilitiesBuilder ThatIsAvailableAt(TimeSlot timeSlot)
    {
        _timeSlot = timeSlot;
        return this;
    }

    public SimulatedCapabilities Build()
    {
        if (_currentResourceId.HasValue)
        {
            _availabilities.Add(new AvailableResourceCapability(_currentResourceId.Value, _capability!, _timeSlot!));
        }

        return new SimulatedCapabilities(_availabilities);
    }
}