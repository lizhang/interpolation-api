using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using InterpolationApi.Configuration;
using InterpolationApi.Models;
using Microsoft.Extensions.Options;

namespace InterpolationApi.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3;
    private readonly AppSettings _settings;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IAmazonS3 s3, IOptions<AppSettings> settings, ILogger<S3Service> logger)
    {
        _s3 = s3;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PresignedPostData> GeneratePresignedPostAsync(string key, string contentType, CancellationToken ct)
    {
        var request = new CreatePresignedPostRequest
        {
            BucketName = _settings.S3BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(15)           
        };
        request.Fields["Content-Type"] = contentType;
        request.Conditions.Add(new ExactMatchCondition("Content-Type", contentType));
        var response = _s3.CreatePresignedPost(request);

        return new PresignedPostData
        {
            Url = response.Url,
            Fields = response.Fields
        };
    }

}
