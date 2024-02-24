using DomainDrivers.SmartSchedule.Shared;
using DomainDrivers.SmartSchedule.Simulation;

namespace DomainDrivers.SmartSchedule.Tests.Simulation;

public class AvailableCapabilitiesBuilder
{
    private readonly IList<AvailableResourceCapability> _availabilities = new List<AvailableResourceCapability>();
    private Guid? _currentResourceId;
    private ISet<Capability>? _capabilities;
    private TimeSlot? _timeSlot;
    private SelectingPolicy _selectingPolicy;

    public AvailableCapabilitiesBuilder WithEmployee(Guid id)
    {
        if (_currentResourceId.HasValue)
        {
            _availabilities.Add(new AvailableResourceCapability(_currentResourceId.Value,
                new CapabilitySelector(_capabilities!, _selectingPolicy), _timeSlot!));
        }

        _currentResourceId = id;
        return this;
    }

    public AvailableCapabilitiesBuilder ThatBrings(Capability capability)
    {
        _capabilities = new HashSet<Capability>() { capability };
        _selectingPolicy = SelectingPolicy.OneOfAll;
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
            _availabilities.Add(new AvailableResourceCapability(_currentResourceId.Value,
                new CapabilitySelector(_capabilities!, _selectingPolicy), _timeSlot!));
        }

        return new SimulatedCapabilities(_availabilities);
    }
    
    public AvailableCapabilitiesBuilder ThatBringsSimultaneously(params Capability[] skills)
    {
        _capabilities = new HashSet<Capability>(skills);
        _selectingPolicy = SelectingPolicy.AllSimultaneously;
        return this;
    }
}