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

    public async Task<GetStatsResponse> ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("GetStats started");

        var (visits, submissions) = await _dynamoDbService.IncrementVisitAndGetStatsAsync(ct);

        return new GetStatsResponse
        {
            Visits = visits,
            Submissions = submissions
        };
    }
}
