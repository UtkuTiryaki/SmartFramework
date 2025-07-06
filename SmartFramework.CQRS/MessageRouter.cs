namespace SmartFramework.CQRS;

internal sealed class MessageRouter(IServiceProvider serviceProvider) : IMessageRouter
{
    public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SendAsync(ICommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TResponse> FetchAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task PublishAsync(IEvent @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResponse> FetchAsync<TResponse>(IStreamQuery<TResponse> query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}