using InterpolationApi.Operations.SubmitJob;
using Microsoft.AspNetCore.Mvc;

namespace InterpolationApi.Controllers;

[ApiController]
[Route("api/jobs")]
[Tags("Jobs")]
public class JobsController(ISubmitJobOperation operation) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<SubmitJobResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(SubmitJobRequest request, CancellationToken ct)
    {
        try
        {
            var response = await operation.ExecuteAsync(request, ct);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
