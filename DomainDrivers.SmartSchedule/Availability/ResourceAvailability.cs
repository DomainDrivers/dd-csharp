using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Availability;

public class ResourceAvailability
{
    public ResourceAvailabilityId Id { get; }
    public ResourceId ResourceId { get; }
    public ResourceId ResourceParentId { get; }
    public TimeSlot Segment { get; }
    public Blockade Blockade { get; private set; }
    public int Version { get; private set; }

    public ResourceAvailability(ResourceAvailabilityId id, ResourceId resourceId,
        ResourceId resourceParentId, TimeSlot segment, Blockade blockade, int version)
    {
        Id = id;
        ResourceId = resourceId;
        ResourceParentId = resourceParentId;
        Segment = segment;
        Blockade = blockade;
        Version = version;
    }

    public ResourceAvailability(ResourceAvailabilityId availabilityId, ResourceId resourceId,
        TimeSlot segment)
    {
        Id = availabilityId;
        ResourceId = resourceId;
        ResourceParentId = ResourceId.None();
        Segment = segment;
        Blockade = Blockade.None();
    }

    public ResourceAvailability(ResourceAvailabilityId availabilityId, ResourceId resourceId,
        ResourceId resourceParentId, TimeSlot segment)
    {
        Id = availabilityId;
        ResourceId = resourceId;
        ResourceParentId = resourceParentId;
        Segment = segment;
        Blockade = Blockade.None();
    }

    public Owner BlockedBy
    {
        get { return Blockade.TakenBy; }
    }

    public bool IsDisabled
    {
        get { return Blockade.Disabled; }
    }

    public bool Block(Owner requester)
    {
        if (IsAvailableFor(requester))
        {
            Blockade = Blockade.OwnedBy(requester);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Release(Owner requester)
    {
        if (IsAvailableFor(requester))
        {
            Blockade = Blockade.None();
            return true;
        }

        return false;
    }

    public bool Disable(Owner requester)
    {
        Blockade = Blockade.DisabledBy(requester);
        return true;
    }

    public bool Enable(Owner requester)
    {
        if (Blockade.CanBeTakenBy(requester))
        {
            Blockade = Blockade.None();
            return true;
        }

        return false;
    }

    private bool IsAvailableFor(Owner requester)
    {
        return Blockade.CanBeTakenBy(requester) && !IsDisabled;
    }

    public bool IsDisabledBy(Owner owner)
    {
        return Blockade.IsDisabledBy(owner);
    }

    protected bool Equals(ResourceAvailability other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ResourceAvailability)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}