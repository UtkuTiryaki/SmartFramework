using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using SmartFramework.Domain;

namespace SmartFramework.EventSourcing.DynamoDb;

public class DynamoDbEventStore(IAmazonDynamoDB dynamoDb) : IEventStore
{
    private const string TableName = "EventStore";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public async Task BootstrapAsync(CancellationToken cancellationToken = default)
    {
        var existingTables = await dynamoDb.ListTablesAsync(cancellationToken);
        if (existingTables.TableNames.Contains(TableName)) return;

        var request = new CreateTableRequest
        {
            TableName = TableName,
            AttributeDefinitions =
            [
                new AttributeDefinition("PK", ScalarAttributeType.S),
                new AttributeDefinition("SK", ScalarAttributeType.S)
            ],
            KeySchema =
            [
                new KeySchemaElement("PK", KeyType.HASH),
                new KeySchemaElement("SK", KeyType.RANGE)
            ],
            BillingMode = BillingMode.PAY_PER_REQUEST
        };
        
        await dynamoDb.CreateTableAsync(request, cancellationToken);
        var count = 0;
        while (count < 10)
        {
            var desc = await dynamoDb.DescribeTableAsync(TableName, cancellationToken);
            if (desc.Table.TableStatus == TableStatus.ACTIVE) break;
            await Task.Delay(1000, cancellationToken);
            count++;
        }
    }

    public async Task SaveStreamAsync(EventStream eventStream, CancellationToken cancellationToken = default)
    {
        var startingVersion = eventStream.CommittedEvents.Count;
        for (var i = 0; i < eventStream.UncommittedEvents.Count; i++)
        {
            var version = startingVersion + i + 1;
            var domainEvent = eventStream.UncommittedEvents[i];
            var pk = $"AGGREGATE#{eventStream.AggregateId}";
            var sk = $"EVENT#{version:D6}";
            
            var item = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new AttributeValue(pk),
                ["SK"] = new AttributeValue(sk),
                ["EventType"] = new AttributeValue(domainEvent.GetType().AssemblyQualifiedName),
                ["EventData"] = new AttributeValue(JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), Options)),
                ["Timestamp"] = new AttributeValue(DateTime.UtcNow.ToString("o")),
                ["Version"] = new AttributeValue { N = version.ToString() }
            };
            
            await dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = TableName,
                Item = item,
                ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
            }, cancellationToken);
        }
    }

    public async Task<EventStream> LoadStreamAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        var pk = $"AGGREGATE#{aggregateId}";
        var req = new QueryRequest
        {
            TableName = TableName,
            KeyConditionExpression = "PK = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue(pk)
            },
            ScanIndexForward = true
        };

        var resp = await dynamoDb.QueryAsync(req, cancellationToken);
        var events = resp.Items.OrderBy(item => int.Parse(item["Version"].N))
            .Select(item =>
            {
                var type = Type.GetType(item["EventType"].S!);
                return (DomainEvent)JsonSerializer.Deserialize(item["EventData"].S!, type!, Options)!;
            }).ToList();

        return new EventStream(aggregateId, events, []);
    }
}