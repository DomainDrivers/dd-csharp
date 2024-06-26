﻿using System.Diagnostics.CodeAnalysis;
using DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public class ProjectAllocations
{
    private TimeSlot? _timeSlot;
    public ProjectAllocationsId ProjectId { get; private set; }

    public Allocations Allocations { get; private set; }

    public Demands Demands { get; private set; }

    public TimeSlot? TimeSlot
    {
        get
        {
            return _timeSlot;
        }
        private set
        {
            //https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities#by-design-restrictions
            _timeSlot = value == null
                ? null
                : new TimeSlot(value.From, value.To);
        }
    }

    public ProjectAllocations(ProjectAllocationsId projectId, Allocations allocations, Demands scheduledDemands,
        TimeSlot timeSlot)
    {
        ProjectId = projectId;
        Allocations = allocations;
        Demands = scheduledDemands;
        TimeSlot = timeSlot;
    }

    public static ProjectAllocations Empty(ProjectAllocationsId projectId)
    {
        return new ProjectAllocations(projectId, Allocations.None(), Demands.None(), TimeSlot.Empty());
    }

    public static ProjectAllocations WithDemands(ProjectAllocationsId projectId, Demands demands)
    {
        return new ProjectAllocations(projectId, Allocations.None(), demands);
    }

    public ProjectAllocations(ProjectAllocationsId projectId, Allocations allocations, Demands demands)
    {
        ProjectId = projectId;
        Allocations = allocations;
        Demands = demands;
    }

    public CapabilitiesAllocated? Allocate(AllocatableCapabilityId allocatableCapabilityId, CapabilitySelector capability,
        TimeSlot requestedSlot, DateTime when)
    {
        var allocatedCapability = new AllocatedCapability(allocatableCapabilityId, capability, requestedSlot);
        var newAllocations = Allocations.Add(allocatedCapability);
        if (NothingAllocated(newAllocations) || !WithinProjectTimeSlot(requestedSlot))
        {
            return null;
        }

        Allocations = newAllocations;
        return new CapabilitiesAllocated(allocatedCapability.AllocatedCapabilityId.Id!.Value, ProjectId, MissingDemands(),
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

        return requestedSlot.Within(TimeSlot);
    }

    public CapabilityReleased? Release(AllocatableCapabilityId allocatedCapabilityId, TimeSlot timeSlot, DateTime when)
    {
        var newAllocations = Allocations.Remove(allocatedCapabilityId, timeSlot);
        if (newAllocations == Allocations)
        {
            return null;
        }

        Allocations = newAllocations;

        return new CapabilityReleased(ProjectId, MissingDemands(), when);
    }

    public Demands MissingDemands()
    {
        return Demands.MissingDemands(Allocations);
    }

    [MemberNotNullWhen(true, nameof(TimeSlot))]
    public bool HasTimeSlot
    {
        get { return TimeSlot != null && TimeSlot != TimeSlot.Empty(); }
    }

    public ProjectAllocationScheduled? DefineSlot(TimeSlot timeSlot, DateTime when)
    {
        TimeSlot = timeSlot;
        return new ProjectAllocationScheduled(ProjectId, timeSlot, when);
    }

    public ProjectAllocationsDemandsScheduled? AddDemands(Demands newDemands, DateTime when)
    {
        Demands = Demands.WithNew(newDemands);
        return new ProjectAllocationsDemandsScheduled(ProjectId, MissingDemands(), when);
    }
}