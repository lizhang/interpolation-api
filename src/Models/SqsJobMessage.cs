namespace InterpolationApi.Models;

public class SqsJobMessage
{
    public string JobId { get; set; } = "";
    public string Email { get; set; } = "";
    public string UploadId { get; set; } = "";
    public string StartFrameKey { get; set; } = "";
    public string EndFrameKey { get; set; } = "";
    public string CreatedAt { get; set; } = "";
}
