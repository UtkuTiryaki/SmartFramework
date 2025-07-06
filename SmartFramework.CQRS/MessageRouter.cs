using Microsoft.Extensions.DependencyInjection;

namespace SmartFramework.CQRS;

internal sealed class MessageRouter(IServiceProvider serviceProvider) : IMessageRouter
{
    public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var requestType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        
        var handler = serviceProvider.GetService(handlerType) ?? 
                      throw new InvalidOperationException($"No handler found for command type {requestType}.");
        
        var method = handlerType.GetMethod("HandleAsync") ?? 
                     throw new InvalidOperationException($"No HandleAsync method found for handler type {handlerType}.");
        
        var result = (Task<TResponse>)method.Invoke(handler, [command, cancellationToken])!;
        
        return await result;
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var requestType = command.GetType();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(requestType);
        
        var handler = serviceProvider.GetService(handlerType) ?? 
                      throw new InvalidOperationException($"No handler found for command type {requestType}.");
        
        var method = handlerType.GetMethod("HandleAsync") ?? 
                     throw new InvalidOperationException($"No HandleAsync method found for handler type {handlerType}.");
        
        var task = (Task)method.Invoke(handler, [command, cancellationToken])!;
        
        await task;
    }

    public async Task<TResponse> FetchAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResponse));
        var handler = serviceProvider.GetService(handlerType) ?? 
                      throw new InvalidOperationException($"No handler found for query type {queryType}.");
            
        var method = handlerType.GetMethod("HandleAsync") ?? 
                     throw new InvalidOperationException($"No HandleAsync method found for handler type {handlerType}.");
            
        var result = (Task<TResponse>)method.Invoke(handler, [query, cancellationToken])!;
        return await result;
    }

    public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        var eventType = @event.GetType();
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType).ToArray();
        if (handlers is null  || handlers.Length is 0) 
        {
            throw new InvalidOperationException($"No handler found for event type {eventType.Name}.");
        }
        
        var method = handlerType.GetMethod("HandleAsync");
        var tasks = handlers.Select(handler => (Task)method!.Invoke(handler, [@event, cancellationToken])!);
        await Task.WhenAll(tasks);
    }

    public IAsyncEnumerable<TResponse> FetchAsync<TResponse>(IStreamQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var requestType = query.GetType();
        var handlerType = typeof(IStreamQueryHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        
        var handler = serviceProvider.GetService(handlerType) ?? 
                      throw new InvalidOperationException($"No handler found for stream request type {requestType}.");
        
        var method = handlerType.GetMethod("HandleAsync") ?? 
                     throw new InvalidOperationException($"No Handle method found for handler type {handlerType}.");
        
        var result = (IAsyncEnumerable<TResponse>)method.Invoke(handler, [query, cancellationToken])!;
        
        return result;
    }
}