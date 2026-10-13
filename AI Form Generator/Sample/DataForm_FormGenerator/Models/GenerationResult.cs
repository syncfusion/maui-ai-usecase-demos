namespace DataForm_FormGenerator;

public sealed class GenerationResult
{
    public EducationFormSchema Schema { get; init; } = new();

    public string Json { get; init; } = string.Empty;

    public TimeSpan Duration { get; init; }
}
