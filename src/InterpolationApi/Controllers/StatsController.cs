using InterpolationApi.Controllers.Dtos;
using InterpolationApi.Operations.GetStats;
using Microsoft.AspNetCore.Mvc;

namespace InterpolationApi.Controllers;

[ApiController]
[Route("api/stats")]
[Tags("Stats")]
public class StatsController(IGetStatsOperation operation) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<GetStatsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await operation.ExecuteAsync(ct);
        return Ok(new GetStatsResponse { Visits = result.Visits, Submissions = result.Submissions });
    }
}
