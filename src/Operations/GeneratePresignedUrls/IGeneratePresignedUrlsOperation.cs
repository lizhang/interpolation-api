namespace InterpolationApi.Operations.GeneratePresignedUrls;

public interface IGeneratePresignedUrlsOperation
{
    Task<GeneratePresignedUrlsResponse> ExecuteAsync(GeneratePresignedUrlsRequest request, CancellationToken ct);
}
