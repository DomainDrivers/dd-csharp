using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Allocation;

public class PublishMissingDemandsService
{
    private readonly ProjectAllocationsRepository _projectAllocationsRepository;
    private readonly CreateHourlyDemandsSummaryService _createHourlyDemandsSummaryService;
    private readonly IEventsPublisher _eventsPublisher;
    private readonly TimeProvider _timeProvider;

    public PublishMissingDemandsService(ProjectAllocationsRepository projectAllocationsRepository,
        CreateHourlyDemandsSummaryService createHourlyDemandsSummaryService, IEventsPublisher eventsPublisher,
        TimeProvider timeProvider)
    {
        _projectAllocationsRepository = projectAllocationsRepository;
        _createHourlyDemandsSummaryService = createHourlyDemandsSummaryService;
        _eventsPublisher = eventsPublisher;
        _timeProvider = timeProvider;
    }

    public async Task Publish()
    {
        var when = _timeProvider.GetUtcNow().DateTime;
        var projectAllocations =
           await _projectAllocationsRepository.FindAllContainingDate(when);
        var missingDemands = _createHourlyDemandsSummaryService.Create(projectAllocations, when);
        //add metadata to event
        //if needed call EventStore and translate multiple private events to a new published event
        await _eventsPublisher.Publish(missingDemands);
    }
}

public class CreateHourlyDemandsSummaryService
{
    public NotSatisfiedDemands Create(IList<ProjectAllocations> projectAllocations, DateTime when)
    {
        var missingDemands =
            projectAllocations
                .Where(x => x.HasTimeSlot)
                .ToDictionary(x => x.ProjectId, x => x.MissingDemands());
        return new NotSatisfiedDemands(missingDemands, when);
    }
}