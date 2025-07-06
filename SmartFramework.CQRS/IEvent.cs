namespace SmartFramework.CQRS;

public interface IEvent : IMessage;

public interface IEventHandler<in TEvent> where TEvent : IEvent 
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}