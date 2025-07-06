namespace SmartFramework.Domain;

public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    protected AggregateRoot() { }
    
    protected readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> PopDomainEvents()
    {
        var copy = _domainEvents.AsReadOnly();
        _domainEvents.Clear();
        return copy;
    }
}