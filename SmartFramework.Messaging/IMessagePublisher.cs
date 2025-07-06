using Amazon.SimpleNotificationService.Model;

namespace SmartFramework.Messaging;

public interface IMessagePublisher
{
    Task<PublishResponse> PublishAsync<T>(T message, string topic, CancellationToken cancellationToken = default) where T : class;
}