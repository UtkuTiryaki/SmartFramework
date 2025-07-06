using SmartFramework.Domain;

namespace SmartFramework.EventSourcing;

public record EventStream(Guid AggregateId, IReadOnlyList<DomainEvent> CommittedEvents, IReadOnlyList<DomainEvent> UncommittedEvents)
{
    public EventStream(Guid aggregateId) : this(aggregateId, [], []) { }
    public EventStream Append(IEnumerable<DomainEvent> newEvents)
        => this with { UncommittedEvents = UncommittedEvents.Concat(newEvents).ToList() };
    public IReadOnlyList<DomainEvent> AllEvents => CommittedEvents.Concat(UncommittedEvents).ToList();
    public TAggregate Replay<TAggregate>(TAggregate initial, Func<TAggregate, DomainEvent, TAggregate> apply) => 
        AllEvents.Aggregate(initial, apply);
    public TAggregate Replay<TAggregate>() where TAggregate : EventSourcedAggregateRoot
        => Replay(EventSourcedAggregateFactory.Create<TAggregate>(), (agg, e) => (TAggregate)agg.Apply(e));
    public static EventStream Create(Guid aggregateId) => new(aggregateId);
}