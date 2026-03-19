using Amazon.Runtime;
using InterpolationApi.Configuration;
using InterpolationApi.Operations.GeneratePresignedUrls;
using InterpolationApi.Operations.SubmitJob;
using InterpolationApi.Services;

namespace InterpolationApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInterpolationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("App"));

        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<Amazon.S3.IAmazonS3>();
        services.AddAWSService<Amazon.SQS.IAmazonSQS>();
        services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();

        services.AddSingleton<AWSCredentials>(_ => FallbackCredentialsFactory.GetCredentials());

        services.AddSingleton<IS3Service, S3Service>();
        services.AddSingleton<ISqsService, SqsService>();
        services.AddSingleton<IDynamoDbService, DynamoDbService>();

        services.AddScoped<IGeneratePresignedUrlsOperation, GeneratePresignedUrlsOperation>();
        services.AddScoped<ISubmitJobOperation, SubmitJobOperation>();

        return services;
    }
}
