namespace InterpolationApi.Operations.GeneratePresignedUrls;

public class GeneratePresignedUrlsInput
{
    public string Email { get; set; } = "";
    public UploadFile StartFile { get; set; } = new();
    public UploadFile EndFile { get; set; } = new();
}

public class UploadFile
{
    public string Name { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; set; }
}
