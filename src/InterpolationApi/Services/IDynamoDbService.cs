namespace InterpolationApi.Services;

public interface IDynamoDbService
{
    Task PutItemAsync(string pk, string sk, Dictionary<string, string> attributes, CancellationToken ct);
    Task<Dictionary<string, string>?> GetItemAsync(string pk, string sk, CancellationToken ct);
    Task UpdateItemAsync(string pk, string sk, Dictionary<string, string> updates, CancellationToken ct);
    Task IncrementCounterAsync(string pk, string sk, string field, CancellationToken ct);
    Task<Dictionary<string, long>> IncrementAndGetAsync(string pk, string sk, string field, CancellationToken ct);
}
