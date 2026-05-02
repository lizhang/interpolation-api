namespace InterpolationApi.Services;

public interface ISqsService
{
    Task SendMessageAsync<T>(T message, CancellationToken ct);
}
