using InterpolationApi.Extensions;
using Amazon.Lambda.AspNetCoreServer.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInterpolationServices(builder.Configuration);
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title = "Interpolation API";
        doc.Info.Version = "v1";
        doc.Info.Description = "API for submitting frame interpolation jobs.";
        return Task.CompletedTask;
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.MapOpenApi();
app.MapScalarApiReference();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Interpolation API v1"));
app.MapGet("/api/health", () => Results.Ok(new { ok = true }));
app.MapControllers();

app.Run();
