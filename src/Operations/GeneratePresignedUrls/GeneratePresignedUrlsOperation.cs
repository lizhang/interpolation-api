using InterpolationApi.Models;
using InterpolationApi.Services;

namespace InterpolationApi.Operations.GeneratePresignedUrls;

public class GeneratePresignedUrlsOperation : IGeneratePresignedUrlsOperation
{
    private readonly IS3Service _s3Service;
    private readonly IDynamoDbService _dynamoDbService;
    private readonly ILogger<GeneratePresignedUrlsOperation> _logger;

    public GeneratePresignedUrlsOperation(
        IS3Service s3Service,
        IDynamoDbService dynamoDbService,
        ILogger<GeneratePresignedUrlsOperation> logger)
    {
        _s3Service = s3Service;
        _dynamoDbService = dynamoDbService;
        _logger = logger;
    }

    public async Task<GeneratePresignedUrlsResponse> ExecuteAsync(GeneratePresignedUrlsRequest request, CancellationToken ct)
    {
        _logger.LogInformation("GeneratePresignedUrls started for {Email}", request.Email);

        Validate(request);

        var uploadId = $"upl_{Guid.NewGuid():N}";
        var startKey = $"uploads/{uploadId}/{request.StartFile.Name}";
        var endKey = $"uploads/{uploadId}/{request.EndFile.Name}";

        var startUpload = await _s3Service.GeneratePresignedPostAsync(startKey, request.StartFile.ContentType, ct);
        var endUpload = await _s3Service.GeneratePresignedPostAsync(endKey, request.EndFile.ContentType, ct);

        var session = new JobRecord
        {
            Email = request.Email,
            UploadId = uploadId,
            StartFrameKey = startKey,
            EndFrameKey = endKey,
            CreatedAt = DateTime.UtcNow.ToString("o"),
        };

        await _dynamoDbService.SaveUploadSessionAsync(session, ct);

        _logger.LogInformation("Upload session {UploadId} created for {Email}", uploadId, request.Email);

        return new GeneratePresignedUrlsResponse
        {
            UploadId = uploadId,
            Start = new FrameUpload
            {
                Key = startKey,
                Upload = new PresignedUpload { Url = startUpload.Url, Fields = startUpload.Fields }
            },
            End = new FrameUpload
            {
                Key = endKey,
                Upload = new PresignedUpload { Url = endUpload.Url, Fields = endUpload.Fields }
            }
        };
    }

    private static void Validate(GeneratePresignedUrlsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.StartFile?.ContentType))
            throw new ArgumentException("Start file content type is required.");

        if (request.StartFile.Size <= 0)
            throw new ArgumentException("Start file size must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.EndFile?.ContentType))
            throw new ArgumentException("End file content type is required.");

        if (request.EndFile.Size <= 0)
            throw new ArgumentException("End file size must be greater than zero.");
    }
}
