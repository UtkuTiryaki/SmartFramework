using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartFramework.CQRS;

namespace SmartFramework.Domain.Persistence;

// Outbox Pattern mit Messaging Lib

public class AbstractContext(
    DbContextOptions options,
    IMessageRouter messageRouter, 
    IHttpContextAccessor httpContextAccessor) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<AggregateRoot>()
            .Select(entry => entry.Entity.PopDomainEvents())
            .SelectMany(x => x)
            .ToList();

        if (IsUserWaiting)
        {
            AddDomainEventsToOfflineProcessingQueue(domainEvents);
            return await base.SaveChangesAsync(cancellationToken);
        }

        await PublishDomainEvents(domainEvents, cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private bool IsUserWaiting => httpContextAccessor.HttpContext is not null;

    private async Task PublishDomainEvents(List<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await messageRouter.PublishAsync(domainEvent, cancellationToken);
        }
    }
    
    private void AddDomainEventsToOfflineProcessingQueue(List<DomainEvent> domainEvents)
    {
        var domainEventsQueue = httpContextAccessor.HttpContext.Items.TryGetValue(EventualConsistencyMiddleware<AbstractContext>.DomainEventsKey, out var value) &&
                                value is Queue<DomainEvent> existingDomainEvents
            ? existingDomainEvents
            : new Queue<DomainEvent>();

        domainEvents.ForEach(domainEventsQueue.Enqueue);
        httpContextAccessor.HttpContext.Items[EventualConsistencyMiddleware<AbstractContext>.DomainEventsKey] = domainEventsQueue;
    }
}