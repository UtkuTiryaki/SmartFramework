namespace SmartFramework.Messaging;

public class SqsEventConsumerOptions<T> where T : IIntegrationEvent
{
    public string QueueUrl { get; set; } = string.Empty;
}