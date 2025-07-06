using SmartFramework.CQRS;

namespace SmartFramework.Messaging;

public interface IIntegrationEvent : IEvent;

public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent 
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}