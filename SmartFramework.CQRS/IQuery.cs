namespace SmartFramework.CQRS;

public interface IQuery<out TResponse> : IMessage;

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken= default);
}

public interface IStreamQuery<out TResponse> : IMessage;

public interface IStreamQueryHandler<in TQuery, TResponse> where TQuery : IStreamQuery<TResponse> 
{
    IAsyncEnumerable<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}