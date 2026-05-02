using InterpolationApi.Models;
using InterpolationApi.Services;

namespace InterpolationApi.Operations.SubmitJob;

public class SubmitJobOperation : ISubmitJobOperation
{
    private readonly IDynamoDbService _dynamoDbService;
    private readonly ISqsService _sqsService;
    private readonly ILogger<SubmitJobOperation> _logger;

    public SubmitJobOperation(
        IDynamoDbService dynamoDbService,
        ISqsService sqsService,
        ILogger<SubmitJobOperation> logger)
    {
        _dynamoDbService = dynamoDbService;
        _sqsService = sqsService;
        _logger = logger;
    }

    public async Task<SubmitJobResult> ExecuteAsync(SubmitJobInput input, CancellationToken ct)
    {
        _logger.LogInformation("SubmitJob started for {Email}, uploadId {UploadId}", input.Email, input.UploadId);

        Validate(input);

        var sessionAttrs = await _dynamoDbService.GetItemAsync(input.Email, input.UploadId, ct);

        if (sessionAttrs is null)
        {
            _logger.LogWarning("Upload session {UploadId} not found for {Email}", input.UploadId, input.Email);
            throw new InvalidOperationException($"Upload session '{input.UploadId}' not found.");
        }

        var startFrameKey = sessionAttrs["startFrameKey"];
        var endFrameKey   = sessionAttrs["endFrameKey"];
        var jobId         = $"job_{Guid.NewGuid():N}";
        var createdAt     = DateTime.UtcNow.ToString("o");

        await _dynamoDbService.UpdateItemAsync(input.Email, input.UploadId, new Dictionary<string, string>
        {
            ["queueId"] = jobId,
            ["step"]    = "Queued"
        }, ct);

        await _sqsService.SendMessageAsync(new SqsJobMessage
        {
            JobId         = jobId,
            Email         = input.Email,
            UploadId      = input.UploadId,
            StartFrameKey = startFrameKey,
            EndFrameKey   = endFrameKey,
            CreatedAt     = createdAt
        }, ct);

        _logger.LogInformation("Job {JobId} queued for {Email}", jobId, input.Email);

        try
        {
            await _dynamoDbService.IncrementCounterAsync("app_stats", "stats", "submissions", ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to increment submission counter for job {JobId}", jobId);
        }

        return new SubmitJobResult { JobId = jobId, Status = "QUEUED" };
    }

    private static void Validate(SubmitJobInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(input.UploadId))
            throw new ArgumentException("UploadId is required.");
    }
}
