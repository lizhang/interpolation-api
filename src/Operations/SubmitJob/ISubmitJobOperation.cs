namespace InterpolationApi.Operations.SubmitJob;

public interface ISubmitJobOperation
{
    Task<SubmitJobResponse> ExecuteAsync(SubmitJobRequest request, CancellationToken ct);
}
