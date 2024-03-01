using MediatR;

namespace DomainDrivers.SmartSchedule.Shared;

public interface IEventsPublisher
{
    Task PublishAfterCommit(IEvent @event);
}

public class EventsPublisher : IEventsPublisher
{
    private readonly IMediator _mediator;

    public EventsPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublishAfterCommit(IEvent @event)
    {
        await _mediator.Publish(@event);
    }
}