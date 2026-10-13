using System.Text.Json;

namespace DataForm_FormGenerator;

public sealed class MockAiSchemaService : IAiSchemaService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public async Task<GenerationResult> GenerateAsync(string educationType, CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.UtcNow;
        await Task.Delay(350, cancellationToken);
        var schema = CreateSchema(educationType);
        var json = JsonSerializer.Serialize(schema, SerializerOptions);

        return new GenerationResult
        {
            Schema = schema,
            Json = json,
            Duration = DateTimeOffset.UtcNow - start
        };
    }

    private static EducationFormSchema CreateSchema(string educationType)
        => EducationFormCatalog.Build(educationType);
}
