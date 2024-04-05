using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public record NeededResourcesChosen(
    ProjectId ProjectId,
    ISet<ResourceId> NeededResources,
    TimeSlot TimeSlot,
    DateTime OccurredAt) : IPublishedEvent
{
    public override int GetHashCode()
    {
        return HashCode.Combine(ProjectId, NeededResources.CalculateHashCode(), TimeSlot, OccurredAt);
    }

    public virtual bool Equals(NeededResourcesChosen? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ProjectId == other.ProjectId && NeededResources.SetEquals(other.NeededResources) &&
               TimeSlot == other.TimeSlot && OccurredAt == other.OccurredAt;
    }
}
