namespace InterpolationApi.Models;

public class JobRecord
{
    public string Email { get; set; } = "";
    public string UploadId { get; set; } = "";
    public string QueueId { get; set; } = "";
    public string StartFrameKey { get; set; } = "";
    public string EndFrameKey { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string ResultsJson { get; set; } = "";
    public string Step { get; set; } = "QUEUED";
}
