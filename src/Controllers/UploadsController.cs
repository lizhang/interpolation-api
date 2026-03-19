using InterpolationApi.Operations.GeneratePresignedUrls;
using Microsoft.AspNetCore.Mvc;

namespace InterpolationApi.Controllers;

[ApiController]
[Route("api/uploads")]
[Tags("Uploads")]
public class UploadsController(IGeneratePresignedUrlsOperation operation) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<GeneratePresignedUrlsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(GeneratePresignedUrlsRequest request, CancellationToken ct)
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
    }
}
