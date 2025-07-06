using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace SmartFramework.Messaging;

public class MessagePublisher(IAmazonSimpleNotificationService snsService) : IMessagePublisher
{
    private readonly Dictionary<string, string> _topicCache = new();

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public async Task<PublishResponse> PublishAsync<T>(T message, string topic, CancellationToken cancellationToken = default) where T : class
    {
        var topicArn = await GetTopicArnAsync(topic);

        var sendMessageRequest = new PublishRequest
        {
            TopicArn = topicArn,
            Message = JsonSerializer.Serialize(message, Options),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "MessageType", new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = typeof(T).AssemblyQualifiedName
                    }
                }
            }
        };
        
        return await snsService.PublishAsync(sendMessageRequest, cancellationToken);
    }

    private async ValueTask<string> GetTopicArnAsync(string topic)
    {
        if (_topicCache.TryGetValue(topic, out string topicArn)) return topicArn;

        var queueUrlResponse = await snsService.FindTopicAsync(topic);
        _topicCache.TryAdd(topic, queueUrlResponse.TopicArn);
        
        return queueUrlResponse.TopicArn;
    }
}