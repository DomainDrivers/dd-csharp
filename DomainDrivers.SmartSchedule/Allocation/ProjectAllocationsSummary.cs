using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public record ProjectsAllocationsSummary(
    IDictionary<ProjectAllocationsId, TimeSlot> TimeSlots,
    IDictionary<ProjectAllocationsId, Allocations> ProjectAllocations,
    IDictionary<ProjectAllocationsId, Demands> Demands)
{
    public static ProjectsAllocationsSummary Of(IList<ProjectAllocations> allProjectAllocations)
    {
        var timeSlots = allProjectAllocations
            .Where(pa => pa.HasTimeSlot)
            .ToDictionary(
                pa => pa.ProjectId,
                pa => pa.TimeSlot!);

        var allocations = allProjectAllocations
            .ToDictionary(
                pa => pa.ProjectId,
                pa => pa.Allocations);

        var demands = allProjectAllocations
            .ToDictionary(
                pa => pa.ProjectId,
                pa => pa.Demands);

        return new ProjectsAllocationsSummary(timeSlots, allocations, demands);
    }

    public virtual bool Equals(ProjectsAllocationsSummary? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return TimeSlots.DictionaryEqual(other.TimeSlots)
               && ProjectAllocations.DictionaryEqual(other.ProjectAllocations)
               && Demands.DictionaryEqual(other.Demands);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TimeSlots.CalculateHashCode(),
            ProjectAllocations.CalculateHashCode(),
            Demands.CalculateHashCode());
    }
}