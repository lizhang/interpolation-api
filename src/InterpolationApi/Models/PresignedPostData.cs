namespace InterpolationApi.Models;

public class PresignedPostData
{
    public string Url { get; set; } = "";
    public Dictionary<string, string> Fields { get; set; } = [];
}
