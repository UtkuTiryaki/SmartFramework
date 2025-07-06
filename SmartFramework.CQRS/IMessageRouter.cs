namespace SmartFramework.CQRS;

public interface IMessageRouter
{
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken);
    Task SendAsync(ICommand command, CancellationToken cancellationToken);
    Task<TResponse> FetchAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken);
    Task PublishAsync(IEvent @event, CancellationToken cancellationToken);
    IAsyncEnumerable<TResponse> FetchAsync<TResponse>(IStreamQuery<TResponse> query, CancellationToken cancellationToken);
}