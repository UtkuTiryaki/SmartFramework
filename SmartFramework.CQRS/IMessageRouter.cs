namespace SmartFramework.CQRS;

public interface IMessageRouter
{
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);
    Task<TResponse> FetchAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
    Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TResponse> FetchAsync<TResponse>(IStreamQuery<TResponse> query, CancellationToken cancellationToken = default);
}