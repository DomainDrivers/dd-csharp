using MediatR;

namespace DomainDrivers.SmartSchedule.Shared;

//metadata:
//correlationId
//potential aggregate's id
//causationId - id of a message that caused this message
//messageId - unique id of the
//user - if there is any (might be a system event)
public interface IPrivateEvent : INotification
{
    DateTime OccurredAt { get; }
}