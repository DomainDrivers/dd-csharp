using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public class ProjectAllocations
{
    private ProjectAllocationsId _projectId;
    private Demands _demands;
    private TimeSlot? _timeSlot;

    public ProjectAllocations(ProjectAllocationsId projectId, Allocations allocations, Demands scheduledDemands,
        TimeSlot timeSlot)
    {
        _projectId = projectId;
        Allocations = allocations;
        _demands = scheduledDemands;
        _timeSlot = timeSlot;
    }

    public Allocations Allocations { get; private set; }

    public static ProjectAllocations Empty(ProjectAllocationsId projectId)
    {
        return new ProjectAllocations(projectId, Allocations.None(), Demands.None(), TimeSlot.Empty());
    }

    public static ProjectAllocations WithDemands(ProjectAllocationsId projectId, Demands demands)
    {
        return new ProjectAllocations(projectId, Allocations.None(), demands);
    }

    private ProjectAllocations(ProjectAllocationsId projectId, Allocations allocations, Demands demands)
    {
        _projectId = projectId;
        Allocations = allocations;
        _demands = demands;
    }

    public CapabilitiesAllocated? Allocate(ResourceId resourceId, Capability capability,
        TimeSlot requestedSlot, DateTime when)
    {
        if (NothingAllocated() || !WithinProjectTimeSlot(requestedSlot))
        {
            return null;
        }

        return new CapabilitiesAllocated(null, null, null, null);
    }

    private bool NothingAllocated()
    {
        return false;
    }

    private bool WithinProjectTimeSlot(TimeSlot requestedSlot)
    {
        return false;
    }

    public CapabilityReleased? Release(Guid allocatedCapabilityId, TimeSlot timeSlot, DateTime when)
    {
        if (NothingReleased())
        {
            return null;
        }

        return new CapabilityReleased(null, null, null);
    }

    private bool NothingReleased()
    {
        return false;
    }

    public Demands MissingDemands()
    {
        return _demands.MissingDemands(Allocations);
    }

    public bool HasTimeSlot
    {
        get { return _timeSlot != null && _timeSlot != TimeSlot.Empty(); }
    }
}