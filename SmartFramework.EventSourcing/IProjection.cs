namespace SmartFramework.EventSourcing;

public interface IProjection
{
    IReadmodel? Project(Guid aggregateRootId);
}