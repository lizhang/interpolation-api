namespace InterpolationApi.Operations.GeneratePresignedUrls;

public interface IGeneratePresignedUrlsOperation
{
    Task<GeneratePresignedUrlsResult> ExecuteAsync(GeneratePresignedUrlsInput input, CancellationToken ct);
}
