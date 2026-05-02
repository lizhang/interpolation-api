using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using InterpolationApi.Configuration;
using Microsoft.Extensions.Options;

namespace InterpolationApi.Services;

public class DynamoDbService : IDynamoDbService
{
    private readonly IAmazonDynamoDB _dynamoClient;
    private readonly AppSettings _settings;
    private readonly ILogger<DynamoDbService> _logger;

    public DynamoDbService(IAmazonDynamoDB dynamoClient, IOptions<AppSettings> settings, ILogger<DynamoDbService> logger)
    {
        _dynamoClient = dynamoClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task PutItemAsync(string pk, string sk, Dictionary<string, string> attributes, CancellationToken ct)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["email"] = new AttributeValue { S = pk },
            ["jobId"] = new AttributeValue { S = sk }
        };

        foreach (var (key, value) in attributes)
            item[key] = new AttributeValue { S = value };

        await _dynamoClient.PutItemAsync(new PutItemRequest { TableName = _settings.DynamoDbTableName, Item = item }, ct);
    }

    public async Task<Dictionary<string, string>?> GetItemAsync(string pk, string sk, CancellationToken ct)
    {
        var response = await _dynamoClient.GetItemAsync(new GetItemRequest
        {
            TableName = _settings.DynamoDbTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["email"] = new AttributeValue { S = pk },
                ["jobId"] = new AttributeValue { S = sk }
            }
        }, ct);

        if (!response.IsItemSet)
            return null;

        return response.Item
            .Where(kvp => kvp.Value.S != null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.S);
    }

    // updates must be non-empty; field names must not be DynamoDB reserved words
    public async Task UpdateItemAsync(string pk, string sk, Dictionary<string, string> updates, CancellationToken ct)
    {
        var setParts = new List<string>();
        var expressionValues = new Dictionary<string, AttributeValue>();
        var index = 0;

        foreach (var (key, value) in updates)
        {
            var placeholder = $":v{index++}";
            setParts.Add($"{key} = {placeholder}");
            expressionValues[placeholder] = new AttributeValue { S = value };
        }

        await _dynamoClient.UpdateItemAsync(new UpdateItemRequest
        {
            TableName = _settings.DynamoDbTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["email"] = new AttributeValue { S = pk },
                ["jobId"] = new AttributeValue { S = sk }
            },
            UpdateExpression = "SET " + string.Join(", ", setParts),
            ExpressionAttributeValues = expressionValues
        }, ct);
    }

    // field is a compile-time constant from the operation layer — direct interpolation is intentional
    public async Task IncrementCounterAsync(string pk, string sk, string field, CancellationToken ct)
    {
        await _dynamoClient.UpdateItemAsync(new UpdateItemRequest
        {
            TableName = _settings.DynamoDbTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["email"] = new AttributeValue { S = pk },
                ["jobId"] = new AttributeValue { S = sk }
            },
            UpdateExpression = $"ADD {field} :one",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":one"] = new AttributeValue { N = "1" }
            }
        }, ct);
    }

    public async Task<Dictionary<string, long>> IncrementAndGetAsync(string pk, string sk, string field, CancellationToken ct)
    {
        var response = await _dynamoClient.UpdateItemAsync(new UpdateItemRequest
        {
            TableName = _settings.DynamoDbTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["email"] = new AttributeValue { S = pk },
                ["jobId"] = new AttributeValue { S = sk }
            },
            UpdateExpression = $"ADD {field} :one",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":one"] = new AttributeValue { N = "1" }
            },
            ReturnValues = ReturnValue.ALL_NEW
        }, ct);

        return response.Attributes
            .Where(kvp => kvp.Value.N != null)
            .ToDictionary(kvp => kvp.Key, kvp => long.Parse(kvp.Value.N));
    }
}
