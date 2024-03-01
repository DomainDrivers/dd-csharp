using MediatR;

namespace DomainDrivers.SmartSchedule.Shared;

public interface IEvent : INotification
{
    DateTime OccurredAt { get; }
}