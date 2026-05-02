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

    public async Task<GeneratePresignedUrlsResult> ExecuteAsync(GeneratePresignedUrlsInput input, CancellationToken ct)
    {
        _logger.LogInformation("GeneratePresignedUrls started for {Email}", input.Email);

        Validate(input);

        var uploadId = $"upl_{Guid.NewGuid():N}";
        var startKey = $"uploads/{uploadId}/{input.StartFile.Name}";
        var endKey = $"uploads/{uploadId}/{input.EndFile.Name}";

        var startUpload = await _s3Service.GeneratePresignedPostAsync(startKey, input.StartFile.ContentType, ct);
        var endUpload = await _s3Service.GeneratePresignedPostAsync(endKey, input.EndFile.ContentType, ct);

        await _dynamoDbService.PutItemAsync(input.Email, uploadId, new Dictionary<string, string>
        {
            ["startFrameKey"] = startKey,
            ["endFrameKey"]   = endKey,
            ["createdAt"]     = DateTime.UtcNow.ToString("o"),
            ["queueId"]       = "",
            ["ResultsJson"]   = "",
            ["step"]          = "Uploaded"
        }, ct);

        _logger.LogInformation("Upload session {UploadId} created for {Email}", uploadId, input.Email);

        return new GeneratePresignedUrlsResult
        {
            UploadId = uploadId,
            Start = new PresignedFrame { Key = startKey, Upload = new PresignedPost { Url = startUpload.Url, Fields = startUpload.Fields } },
            End   = new PresignedFrame { Key = endKey,   Upload = new PresignedPost { Url = endUpload.Url,   Fields = endUpload.Fields } }
        };
    }

    private static void Validate(GeneratePresignedUrlsInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(input.StartFile?.ContentType))
            throw new ArgumentException("Start file content type is required.");

        if (input.StartFile.Size <= 0)
            throw new ArgumentException("Start file size must be greater than zero.");

        if (string.IsNullOrWhiteSpace(input.EndFile?.ContentType))
            throw new ArgumentException("End file content type is required.");

        if (input.EndFile.Size <= 0)
            throw new ArgumentException("End file size must be greater than zero.");
    }
}
