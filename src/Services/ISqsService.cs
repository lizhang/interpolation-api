using InterpolationApi.Models;

namespace InterpolationApi.Services;

public interface ISqsService
{
    Task SendJobMessageAsync(SqsJobMessage message, CancellationToken ct);
}
