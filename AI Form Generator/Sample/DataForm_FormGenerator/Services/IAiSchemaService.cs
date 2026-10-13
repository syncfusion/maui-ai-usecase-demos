namespace DataForm_FormGenerator;

public interface IAiSchemaService
{
    Task<GenerationResult> GenerateAsync(string educationType, CancellationToken cancellationToken = default);
}
