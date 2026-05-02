namespace InterpolationApi.Operations.GeneratePresignedUrls;

public class GeneratePresignedUrlsResult
{
    public string UploadId { get; set; } = "";
    public PresignedFrame Start { get; set; } = new();
    public PresignedFrame End { get; set; } = new();
}

public class PresignedFrame
{
    public string Key { get; set; } = "";
    public PresignedPost Upload { get; set; } = new();
}

public class PresignedPost
{
    public string Url { get; set; } = "";
    public Dictionary<string, string> Fields { get; set; } = [];
}
