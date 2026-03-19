namespace InterpolationApi.Operations.GeneratePresignedUrls;

public class GeneratePresignedUrlsRequest
{
    public string Email { get; set; } = "";
    public FileMetadata StartFile { get; set; } = new();
    public FileMetadata EndFile { get; set; } = new();
}

public class FileMetadata
{
    public string Name { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; set; }
}
