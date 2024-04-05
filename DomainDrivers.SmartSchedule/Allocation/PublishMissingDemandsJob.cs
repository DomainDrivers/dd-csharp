using Quartz;

namespace DomainDrivers.SmartSchedule.Allocation;

public class PublishMissingDemandsJob  : IJob
{
    private readonly PublishMissingDemandsService _publishMissingDemandsService;

    public PublishMissingDemandsJob(PublishMissingDemandsService publishMissingDemandsService)
    {
        _publishMissingDemandsService = publishMissingDemandsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _publishMissingDemandsService.Publish();
    }
}