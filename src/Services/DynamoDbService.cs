using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using InterpolationApi.Configuration;
using InterpolationApi.Models;
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

    public async Task SaveUploadSessionAsync(JobRecord job, CancellationToken ct)
    {
        var request = new PutItemRequest
        {
            TableName = _settings.DynamoDbTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                ["email"] = new AttributeValue { S = job.Email },
                ["jobId"] = new AttributeValue { S = job.UploadId },
                ["queueId"] = new AttributeValue { S = "" },
                ["startFrameKey"] = new AttributeValue { S = job.StartFrameKey },
                ["endFrameKey"] = new AttributeValue { S = job.EndFrameKey },
                ["createdAt"] = new AttributeValue { S = job.CreatedAt },
                ["ResultsJson"] = new AttributeValue { S = job.ResultsJson },
                ["step"] = new AttributeValue { S = "Uploaded" },
            }
        };

        await _dynamoClient.PutItemAsync(request, ct);

        _logger.LogInformation("Upload session {UploadId} saved for {Email}", job.UploadId, job.Email);
    }

    public async Task<JobRecord?> GetUploadSessionAsync(string email, string uploadId, CancellationToken ct)
    {
        var request = new GetItemRequest
        {
            TableName = _settings.DynamoDbTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["email"] = new AttributeValue { S = email },
                ["jobId"] = new AttributeValue { S = uploadId },
            }
        };

        var response = await _dynamoClient.GetItemAsync(request, ct);

        if (!response.IsItemSet)
            return null;

        return new JobRecord
        {
            Email = response.Item["email"].S,
            UploadId = response.Item["jobId"].S,
            StartFrameKey = response.Item["startFrameKey"].S,
            EndFrameKey = response.Item["endFrameKey"].S,
            CreatedAt = response.Item["createdAt"].S,
        };
    }

    public async Task SaveJobRecordAsync(JobRecord job, CancellationToken ct)
    {
        var request = new UpdateItemRequest
        {
            TableName = _settings.DynamoDbTableName,
            Key = new Dictionary<string, AttributeValue>
             {
                { "email", new AttributeValue { S = job.Email } },
                { "jobId", new AttributeValue { S = job.UploadId } }
            },
            UpdateExpression = "SET queueId = :q, step = :s",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":q"] = new AttributeValue { S = job.QueueId },
                [":s"] = new AttributeValue { S = "Queued" }
            }
        };

        await _dynamoClient.UpdateItemAsync(request, ct);

        _logger.LogInformation("Job record {JobId} saved for {Email}", job.QueueId, job.Email);
    }
}
