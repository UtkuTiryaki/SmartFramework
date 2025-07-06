namespace SmartFramework.EventSourcing;

public interface IEventStore : IEventStoreBootstrap
{
    Task SaveStreamAsync(EventStream eventStream, CancellationToken cancellationToken = default);
    Task<EventStream> LoadStreamAsync(Guid aggregateId, CancellationToken cancellationToken = default);
}

public interface IEventStoreBootstrap
{
    Task BootstrapAsync(CancellationToken cancellationToken = default);
}