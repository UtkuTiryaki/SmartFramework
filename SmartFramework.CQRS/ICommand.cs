namespace SmartFramework.CQRS;

public interface ICommand : IMessage;

public interface ICommand<out TResponse> : IMessage;

public interface ICommandHandler<in TCommand> where TCommand : ICommand 
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse> 
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}