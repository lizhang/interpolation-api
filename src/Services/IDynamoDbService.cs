using InterpolationApi.Models;

namespace InterpolationApi.Services;

public interface IDynamoDbService
{
    Task SaveUploadSessionAsync(JobRecord session, CancellationToken ct);
    Task<JobRecord?> GetUploadSessionAsync(string email, string uploadId, CancellationToken ct);
    Task SaveJobRecordAsync(JobRecord job, CancellationToken ct);
}
