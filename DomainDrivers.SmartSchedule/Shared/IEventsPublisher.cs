using MediatR;

namespace DomainDrivers.SmartSchedule.Shared;

public interface IEventsPublisher
{
    //remember about transactions scope
    Task Publish(IPublishedEvent @event);
}

public class EventsPublisher : IEventsPublisher
{
    private readonly IMediator _mediator;

    public EventsPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Publish(IPublishedEvent @event)
    {
        await _mediator.Publish(@event);
    }
}