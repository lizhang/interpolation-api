using InterpolationApi.Models;

namespace InterpolationApi.Services;

public interface IS3Service
{
    Task<PresignedPostData> GeneratePresignedPostAsync(string key, string contentType, CancellationToken ct);
}
