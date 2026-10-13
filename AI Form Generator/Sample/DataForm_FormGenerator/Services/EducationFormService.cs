namespace DataForm_FormGenerator;

/// <summary>
/// Thin coordinator that owns the IAiSchemaService and the POCO factory.
/// The parent EducationFormViewModel injects this into each
/// EducationEntryViewModel so individual cards never need to know about
/// credentials or schema parsing.
/// </summary>
public sealed class EducationFormService
{
    private readonly IAiSchemaService schemaService;

    public EducationFormService(IAiSchemaService schemaService)
    {
        this.schemaService = schemaService;
    }

    /// <summary>True while a generation is in flight.</summary>
    public bool IsBusy { get; private set; }

    public event EventHandler? BusyChanged;

    /// <summary>
    /// Validates the Azure OpenAI credentials by issuing a small probe
    /// completion against the configured deployment. Surfaces any
    /// configuration / 401 / 404 errors back to the caller via an
    /// <see cref="InvalidOperationException"/> so the view-model can put
    /// the message in its error banner before the user clicks "Add
    /// Education Fields" for the first time.
    /// </summary>
    public Task ValidateCredentialsAsync(CancellationToken cancellationToken = default)
    {
        if (schemaService is AzureOpenAiSchemaService azure)
        {
            return azure.GetResultsFromAI(cancellationToken);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Runs the schema service for <paramref name="educationType"/>, then
    /// returns the schema title and a freshly-constructed POCO ready to be
    /// bound to SfDataForm. Lets credential / AI exceptions bubble up so
    /// the view-model can surface them in the error banner; returns
    /// <c>(null, null)</c> only when the requested level isn't a known
    /// catalog value (i.e. caller asked for a level that doesn't exist).
    /// </summary>
    public async Task<(string? Title, object? Entry)> GenerateAsync(string educationType)
    {
        var normalized = (educationType ?? string.Empty).Trim();
        if (!EducationFormCatalog.IsKnown(normalized))
        {
            return (null, null);
        }

        try
        {
            SetBusy(true);
            var result = await schemaService.GenerateAsync(normalized);
            var schema = result.Schema;
            if (schema is null)
            {
                throw new InvalidOperationException("AI returned a null schema.");
            }

            if (schema.Fields.Count == 0)
            {
                throw new InvalidOperationException(
                    "AI returned an empty schema. Please retry — if this persists, the deployment may be unreachable.");
            }

            var entry = CreateEntry(normalized);
            return (schema.Title, entry);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool value)
    {
        IsBusy = value;
        BusyChanged?.Invoke(this, EventArgs.Empty);
    }

    private static object CreateEntry(string educationType)
    {
        var normalized = educationType.Trim().ToLowerInvariant();
        if (normalized == EducationFormCatalog.HighSchool.ToLowerInvariant())
        {
            return new HighSchoolEntry();
        }
        if (normalized == EducationFormCatalog.Diploma.ToLowerInvariant())
        {
            return new DiplomaEntry();
        }
        if (normalized == EducationFormCatalog.AssociateDegree.ToLowerInvariant())
        {
            return new AssociateEntry();
        }
        if (normalized == EducationFormCatalog.BachelorsDegree.ToLowerInvariant())
        {
            return new BachelorsEntry();
        }
        if (normalized == EducationFormCatalog.MastersDegree.ToLowerInvariant())
        {
            return new MasterEntry();
        }
        return new HighSchoolEntry();
    }
}