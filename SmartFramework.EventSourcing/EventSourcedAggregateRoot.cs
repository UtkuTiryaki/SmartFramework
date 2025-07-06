using SmartFramework.Domain;

namespace SmartFramework.EventSourcing;

public abstract record EventSourcedAggregateRoot
{
    public Guid Id { get; init; }
    public IReadOnlyList<DomainEvent> UncommittedEvents { get; private init; } = [];
    
    public abstract EventSourcedAggregateRoot Apply(DomainEvent domainEvent);

    public T AddDomainEvent<T>(DomainEvent domainEvent) where T : EventSourcedAggregateRoot
    {
        var updated = (T)this.Apply(domainEvent);
        var updatedEvents = updated.UncommittedEvents.Concat([domainEvent]).ToList();
        return updated with { UncommittedEvents = updatedEvents };
    }

    public T LoadFromHistory<T>(IEnumerable<DomainEvent> history) where T : EventSourcedAggregateRoot
    {
        var aggregate = (T)this;
        aggregate = history.Aggregate(aggregate, (current, domainEvent) => (T)current.Apply(domainEvent));
        return aggregate with { UncommittedEvents = [] };
    }
}