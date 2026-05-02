namespace InterpolationApi.Operations.GetStats;

public interface IGetStatsOperation
{
    Task<AppStats> ExecuteAsync(CancellationToken ct);
}
