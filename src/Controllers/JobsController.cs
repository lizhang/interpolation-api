using InterpolationApi.Controllers.Dtos;
using InterpolationApi.Operations.GeneratePresignedUrls;
using InterpolationApi.Operations.SubmitJob;
using Microsoft.AspNetCore.Mvc;

namespace InterpolationApi.Controllers;

[ApiController]
[Tags("Jobs")]
public class JobsController(
    IGeneratePresignedUrlsOperation generatePresignedUrlsOperation,
    ISubmitJobOperation submitJobOperation) : ControllerBase
{
    [HttpPost("api/uploads")]
    [ProducesResponseType<GeneratePresignedUrlsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostUpload(GeneratePresignedUrlsRequest request, CancellationToken ct)
    {
        try
        {
            var input = new GeneratePresignedUrlsInput
            {
                Email     = request.Email,
                StartFile = new UploadFile { Name = request.StartFile.Name, ContentType = request.StartFile.ContentType, Size = request.StartFile.Size },
                EndFile   = new UploadFile { Name = request.EndFile.Name,   ContentType = request.EndFile.ContentType,   Size = request.EndFile.Size }
            };
            var result = await generatePresignedUrlsOperation.ExecuteAsync(input, ct);
            return Ok(new GeneratePresignedUrlsResponse
            {
                UploadId = result.UploadId,
                Start = new FrameUpload { Key = result.Start.Key, Upload = new PresignedUpload { Url = result.Start.Upload.Url, Fields = result.Start.Upload.Fields } },
                End   = new FrameUpload { Key = result.End.Key,   Upload = new PresignedUpload { Url = result.End.Upload.Url,   Fields = result.End.Upload.Fields } }
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("api/jobs")]
    [ProducesResponseType<SubmitJobResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostJob(SubmitJobRequest request, CancellationToken ct)
    {
        try
        {
            var input = new SubmitJobInput { Email = request.Email, UploadId = request.UploadId };
            var result = await submitJobOperation.ExecuteAsync(input, ct);
            return Ok(new SubmitJobResponse { JobId = result.JobId, Status = result.Status });
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
