using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFramework.CQRS;

namespace SmartFramework.Messaging;

public class SqsEventConsumerOptions<T> where T : IIntegrationEvent
{
    public string QueueUrl { get; set; } = string.Empty;
}

public class ServiceBusConsumer<T>(
    IAmazonSQS sqsClient,
    IServiceProvider serviceProvider,
    SqsEventConsumerOptions<T> options,
    ILogger<ServiceBusConsumer<T>> logger) : BackgroundService where T : class, IIntegrationEvent
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
        
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("SQS Event Consumer started for queue: {QueueUrl}", options.QueueUrl);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEventAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in event processing loop");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
        
        logger.LogInformation("SQS Event Consumer stopped for queue: {QueueUrl}", options.QueueUrl);
    }

    private async Task ProcessEventAsync(CancellationToken cancellationToken)
    {
        var receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = options.QueueUrl,
            MessageAttributeNames = ["All"],
            AttributeNames = new List<string> { "All" },
            MaxNumberOfMessages = 1
        };
        
        var response = await sqsClient.ReceiveMessageAsync(receiveRequest, cancellationToken);
        if (response.Messages.Count is 0) return;

        foreach (var message in response.Messages)
        {
            if (serviceProvider.GetService(typeof(T)) is not IIntegrationEventHandler<T> handler)
                throw new NotImplementedException($"IntegrationEventHandler not found for type {typeof(T).Name}");
            
            var integrationEvent = JsonSerializer.Deserialize<T>(message.Body, Options);
            if (integrationEvent is null) continue;
            
            await handler.HandleAsync(integrationEvent, cancellationToken);
            await sqsClient.DeleteMessageAsync(options.QueueUrl, message.ReceiptHandle, cancellationToken);
        }
    }
}