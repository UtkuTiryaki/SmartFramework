namespace SmartFramework.EventSourcing;

public interface IEventStore
{
    Task SaveStreamAsync(EventStream eventStream, CancellationToken cancellationToken = default);
    Task<EventStream> LoadStreamAsync(Guid aggregateId, CancellationToken cancellationToken = default);
}