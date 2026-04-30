namespace InterpolationApi.Operations.GetStats;

public interface IGetStatsOperation
{
    Task<GetStatsResponse> ExecuteAsync(CancellationToken ct);
}
