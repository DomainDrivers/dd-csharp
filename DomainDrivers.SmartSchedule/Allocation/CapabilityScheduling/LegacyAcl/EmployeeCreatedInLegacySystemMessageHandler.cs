using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation.CapabilityScheduling.LegacyAcl;

public class EmployeeCreatedInLegacySystemMessageHandler
{
    private readonly CapabilityScheduler _capabilityScheduler;

    public EmployeeCreatedInLegacySystemMessageHandler(CapabilityScheduler capabilityScheduler)
    {
        _capabilityScheduler = capabilityScheduler;
    }

    //subscribe to message bus
    //StreamListener to (message_bus)
    public async Task Handle(EmployeeDataFromLegacyEsbMessage message)
    {
        var allocatableResourceId = new AllocatableResourceId(message.ResourceId);
        var capabilitySelectors = new TranslateToCapabilitySelector().Translate(message);
        await _capabilityScheduler.ScheduleResourceCapabilitiesForPeriod(allocatableResourceId, capabilitySelectors,
            message.TimeSlot);
    }
}

public record EmployeeDataFromLegacyEsbMessage(
    Guid ResourceId,
    IList<IList<string>> SkillsPerformedTogether,
    IList<string> ExclusiveSkills,
    IList<string> Permissions,
    TimeSlot TimeSlot)
{
    public override int GetHashCode()
    {
        return HashCode.Combine(ResourceId,
            SkillsPerformedTogether.Aggregate(0, (hash, pair) => HashCode.Combine(hash, pair.CalculateHashCode())),
            ExclusiveSkills.CalculateHashCode(),
            Permissions.CalculateHashCode(), TimeSlot);
    }

    public virtual bool Equals(EmployeeDataFromLegacyEsbMessage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return ResourceId == other.ResourceId &&
               SkillsPerformedTogether.SequenceEqual(other.SkillsPerformedTogether,
                   EqualityComparer<IList<string>>.Create((x, y) => x!.SequenceEqual(y!))) &&
               ExclusiveSkills.SequenceEqual(other.ExclusiveSkills) && Permissions.SequenceEqual(other.Permissions) &&
               TimeSlot == other.TimeSlot;
    }
}