using InterpolationApi.Services;

namespace InterpolationApi.Operations.GetStats;

public class GetStatsOperation : IGetStatsOperation
{
    private readonly IDynamoDbService _dynamoDbService;
    private readonly ILogger<GetStatsOperation> _logger;

    public GetStatsOperation(IDynamoDbService dynamoDbService, ILogger<GetStatsOperation> logger)
    {
        _dynamoDbService = dynamoDbService;
        _logger = logger;
    }

    public async Task<AppStats> ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("GetStats started");

        var counters = await _dynamoDbService.IncrementAndGetAsync("app_stats", "stats", "visits", ct);

        return new AppStats
        {
            Visits      = counters.TryGetValue("visits",      out var v) ? (int)v : 0,
            Submissions = counters.TryGetValue("submissions", out var s) ? (int)s : 0
        };
    }
}
