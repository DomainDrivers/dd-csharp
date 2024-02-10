using System.Diagnostics.CodeAnalysis;
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
        var allocatedCapability = new AllocatedCapability(resourceId.Id, capability, requestedSlot);
        var newAllocations = Allocations.Add(allocatedCapability);
        if (NothingAllocated(newAllocations) || !WithinProjectTimeSlot(requestedSlot))
        {
            return null;
        }

        Allocations = newAllocations;
        return new CapabilitiesAllocated(allocatedCapability.AllocatedCapabilityId, _projectId, MissingDemands(),
            when);
    }

    private bool NothingAllocated(Allocations newAllocations)
    {
        return newAllocations == Allocations;
    }

    private bool WithinProjectTimeSlot(TimeSlot requestedSlot)
    {
        if (!HasTimeSlot)
        {
            return true;
        }

        return requestedSlot.Within(_timeSlot);
    }

    public CapabilityReleased? Release(Guid allocatedCapabilityId, TimeSlot timeSlot, DateTime when)
    {
        var newAllocations = Allocations.Remove(allocatedCapabilityId, timeSlot);
        if (newAllocations == Allocations)
        {
            return null;
        }

        Allocations = newAllocations;

        return new CapabilityReleased(_projectId, MissingDemands(), when);
    }

    public Demands MissingDemands()
    {
        return _demands.MissingDemands(Allocations);
    }

    [MemberNotNullWhen(true, nameof(_timeSlot))]
    public bool HasTimeSlot
    {
        get { return _timeSlot != null && _timeSlot != TimeSlot.Empty(); }
    }

    public ProjectAllocationScheduled? DefineSlot(TimeSlot timeSlot, DateTime when)
    {
        _timeSlot = timeSlot;
        return new ProjectAllocationScheduled(_projectId, timeSlot, when);
    }

    public ProjectAllocationsDemandsScheduled? AddDemands(Demands newDemands, DateTime when)
    {
        _demands = _demands.WithNew(newDemands);
        return new ProjectAllocationsDemandsScheduled(_projectId, MissingDemands(), when);
    }
}