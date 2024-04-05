using DomainDrivers.SmartSchedule.Availability;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Planning;

public record CriticalStagePlanned(
    ProjectId ProjectId,
    TimeSlot StageTimeSlot,
    ResourceId? CriticalResource,
    DateTime OccurredAt) : IPublishedEvent;