using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using InterpolationApi.Configuration;
using Microsoft.Extensions.Options;

namespace InterpolationApi.Services;

public class SqsService : ISqsService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly AppSettings _settings;
    private readonly ILogger<SqsService> _logger;

    public SqsService(IAmazonSQS sqsClient, IOptions<AppSettings> settings, ILogger<SqsService> logger)
    {
        _sqsClient = sqsClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendMessageAsync<T>(T message, CancellationToken ct)
    {
        var body = JsonSerializer.Serialize(message);

        await _sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = _settings.SqsQueueUrl,
            MessageBody = body
        }, ct);

        _logger.LogInformation("SQS message sent");
    }
}
