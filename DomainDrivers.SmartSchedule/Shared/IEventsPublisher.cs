using MediatR;

namespace DomainDrivers.SmartSchedule.Shared;

public interface IEventsPublisher
{
    //remember about transactions scope
    Task Publish(IEvent @event);
}

public class EventsPublisher : IEventsPublisher
{
    private readonly IMediator _mediator;

    public EventsPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Publish(IEvent @event)
    {
        await _mediator.Publish(@event);
    }
}