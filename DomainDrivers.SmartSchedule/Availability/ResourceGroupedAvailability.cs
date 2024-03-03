using DomainDrivers.SmartSchedule.Availability.Segment;
using DomainDrivers.SmartSchedule.Shared;
using static DomainDrivers.SmartSchedule.Availability.Segment.SegmentInMinutes;

namespace DomainDrivers.SmartSchedule.Availability;

public class ResourceGroupedAvailability
{
    public ResourceGroupedAvailability(IList<ResourceAvailability> resourceAvailabilities)
    {
        Availabilities = resourceAvailabilities;
    }

    public IList<ResourceAvailability> Availabilities { get; }

    public ResourceId? ResourceId
    {
        get
        {
            //resourceId are the same;
            return Availabilities.Select(x => x.ResourceId).FirstOrDefault();
        }
    }

    public int Size
    {
        get { return Availabilities.Count; }
    }

    public bool IsEntirelyAvailable
    {
        get { return Availabilities.All(ra => ra.BlockedBy.ByNone); }
    }

    public bool HasNoSlots
    {
        get { return Availabilities.Count == 0; }
    }
    
    public static ResourceGroupedAvailability Of(ResourceId resourceId, TimeSlot timeslot)
    {
        var resourceAvailabilities = Segments
            .Split(timeslot, DefaultSegment())
            .Select(segment => new ResourceAvailability(ResourceAvailabilityId.NewOne(), resourceId, segment))
            .ToList();
        return new ResourceGroupedAvailability(resourceAvailabilities);
    }

    public static ResourceGroupedAvailability Of(ResourceId resourceId, TimeSlot timeslot,
        ResourceId parentId)
    {
        var resourceAvailabilities = Segments
            .Split(timeslot, DefaultSegment())
            .Select(segment => new ResourceAvailability(ResourceAvailabilityId.NewOne(), resourceId, parentId, segment))
            .ToList();
        return new ResourceGroupedAvailability(resourceAvailabilities);
    }

    public bool Block(Owner requester)
    {
        foreach (var resourceAvailability in Availabilities)
        {
            if (!resourceAvailability.Block(requester))
            {
                return false;
            }
        }

        return true;
    }

    public bool Disable(Owner requester)
    {
        foreach (var resourceAvailability in Availabilities)
        {
            if (!resourceAvailability.Disable(requester))
            {
                return false;
            }
        }

        return true;
    }

    public bool Release(Owner requester)
    {
        foreach (var resourceAvailability in Availabilities)
        {
            if (!resourceAvailability.Release(requester))
            {
                return false;
            }
        }

        return true;
    }

    public bool BlockedEntirelyBy(Owner owner)
    {
        return Availabilities.All(ra => ra.BlockedBy == owner);
    }

    public bool IsDisabledEntirelyBy(Owner owner)
    {
        return Availabilities.All(ra => ra.IsDisabledBy(owner));
    }

    public IList<ResourceAvailability> FindBlockedBy(Owner owner)
    {
        return Availabilities
            .Where(ra => ra.BlockedBy == owner)
            .ToList();
    }

    public ISet<Owner> Owners()
    {
        return Availabilities
            .Select(x => x.BlockedBy)
            .ToHashSet();
    }
}