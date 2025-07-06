using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartFramework.CQRS;

namespace SmartFramework.Domain.Persistence;

public class EventualConsistencyMiddleware<TContext> where TContext : DbContext
{
    public const string DomainEventsKey = "DomainEventsKey";

    private readonly RequestDelegate _next;

    public EventualConsistencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IMessageRouter router, TContext context)
    {
        var transaction = await context.Database.BeginTransactionAsync();
        
        httpContext.Response.OnCompleted(async () =>
        {
            try
            {
                if (httpContext.Items.TryGetValue(DomainEventsKey, out var value) &&
                    value is Queue<DomainEvent> domainEvents)
                {
                    while (domainEvents.TryDequeue(out var nextEvent)) await router.PublishAsync(nextEvent);
                }

                await transaction.CommitAsync();
            }
            catch (EventualConsistencyException)
            {
                // FehlerHandling
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        });
        
        await _next(httpContext);
    }
}