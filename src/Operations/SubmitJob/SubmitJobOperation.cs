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

    public async Task<SubmitJobResponse> ExecuteAsync(SubmitJobRequest request, CancellationToken ct)
    {
        _logger.LogInformation("SubmitJob started for {Email}, uploadId {UploadId}", request.Email, request.UploadId);

        Validate(request);

        var session = await _dynamoDbService.GetUploadSessionAsync(request.Email, request.UploadId, ct);

        if (session is null)
        {
            _logger.LogWarning("Upload session {UploadId} not found for {Email}", request.UploadId, request.Email);
            throw new InvalidOperationException($"Upload session '{request.UploadId}' not found.");
        }

        var jobId = $"job_{Guid.NewGuid():N}";
        var createdAt = DateTime.UtcNow.ToString("o");

        var jobRecord = new JobRecord
        {
            Email = request.Email,
            UploadId = request.UploadId,
            QueueId = jobId,
            StartFrameKey = session.StartFrameKey,
            EndFrameKey = session.EndFrameKey,
            CreatedAt = createdAt,
            ResultsJson = "",
        };

        await _dynamoDbService.SaveJobRecordAsync(jobRecord, ct);

        var message = new SqsJobMessage
        {
            JobId = jobId,
            Email = request.Email,
            UploadId = request.UploadId,
            StartFrameKey = session.StartFrameKey,
            EndFrameKey = session.EndFrameKey,
            CreatedAt = createdAt,
        };

        await _sqsService.SendJobMessageAsync(message, ct);

        _logger.LogInformation("Job {JobId} queued for {Email}", jobId, request.Email);

        return new SubmitJobResponse
        {
            JobId = jobId,
            Status = "QUEUED"
        };
    }

    private static void Validate(SubmitJobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.UploadId))
            throw new ArgumentException("UploadId is required.");
    }
}
