namespace InterpolationApi.Operations.SubmitJob;

public interface ISubmitJobOperation
{
    Task<SubmitJobResult> ExecuteAsync(SubmitJobInput input, CancellationToken ct);
}
