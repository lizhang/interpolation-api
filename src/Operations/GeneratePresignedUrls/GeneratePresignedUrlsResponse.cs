namespace InterpolationApi.Operations.GeneratePresignedUrls;

public class GeneratePresignedUrlsResponse
{
    public string UploadId { get; set; } = "";
    public FrameUpload Start { get; set; } = new();
    public FrameUpload End { get; set; } = new();
}

public class FrameUpload
{
    public string Key { get; set; } = "";
    public PresignedUpload Upload { get; set; } = new();
}

public class PresignedUpload
{
    public string Url { get; set; } = "";
    public Dictionary<string, string> Fields { get; set; } = [];
}
