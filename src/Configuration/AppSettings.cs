namespace InterpolationApi.Configuration;

public class AppSettings
{
    public string S3BucketName { get; set; } = "";
    public string SqsQueueUrl { get; set; } = "";
    public string DynamoDbTableName { get; set; } = "";
    public string AwsRegion { get; set; } = "us-east-1";
}
