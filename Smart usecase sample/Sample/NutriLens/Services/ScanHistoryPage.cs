using System.Text.Json;
using NutriLens.Models;

namespace NutriLens.Services;

/// <summary>One persisted AI scan: the full AI analysis + save metadata.</summary>
public sealed class SavedScan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Path of the captured label image (may be empty).</summary>
    public string ImagePath { get; set; } = string.Empty;

    public IngredientAnalysisResult Result { get; set; } = new();
}

public interface IScanHistoryStore
{
    Task<IReadOnlyList<SavedScan>> GetAllAsync();

    /// <summary>Adds a scan (newest first). Deduplicates the same product+score.</summary>
    Task<SavedScan> AddAsync(
        IngredientAnalysisResult result,
        string? imagePath = null);

    Task ClearAsync();
}

/// <summary>
/// JSON-file-backed scan history stored in FileSystem.AppDataDirectory.
/// Same persistence pattern the app already uses (JSON serialization);
/// swappable for a SQLite implementation behind IScanHistoryStore later.
/// </summary>
public sealed class JsonScanHistoryStore : IScanHistoryStore
{
    private static readonly string FilePath =
        Path.Combine(FileSystem.AppDataDirectory, "scan_history.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private List<SavedScan>? cached;

    public async Task<IReadOnlyList<SavedScan>> GetAllAsync()
    {
        await gate.WaitAsync();
        try
        {
            return (await LoadAsync()).AsReadOnly();
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<SavedScan> AddAsync(
        IngredientAnalysisResult result,
        string? imagePath = null)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        await gate.WaitAsync();
        try
        {
            var list = await LoadAsync();

            // Deduplicate: saving the same product with the same score again
            // just refreshes its timestamp instead of spamming duplicates.
            var existing = list.FirstOrDefault(s =>
                string.Equals(s.Result.ProductName, result.ProductName,
                    StringComparison.OrdinalIgnoreCase) &&
                s.Result.Score == result.Score);

            SavedScan scan;

            if (existing is not null)
            {
                existing.SavedAtUtc = DateTime.UtcNow;
                existing.ImagePath = imagePath ?? existing.ImagePath;
                existing.Result = result;
                list.Remove(existing);
                list.Insert(0, existing);
                scan = existing;
            }
            else
            {
                scan = new SavedScan
                {
                    Result = result,
                    ImagePath = imagePath ?? string.Empty
                };
                list.Insert(0, scan);
            }

            await File.WriteAllTextAsync(
                FilePath,
                JsonSerializer.Serialize(list, JsonOptions));

            cached = list;
            return scan;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task ClearAsync()
    {
        await gate.WaitAsync();
        try
        {
            cached = [];
            File.Delete(FilePath);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<List<SavedScan>> LoadAsync()
    {
        if (cached is not null)
            return cached;

        if (!File.Exists(FilePath))
        {
            cached = [];
            return cached;
        }

        try
        {
            var raw = await File.ReadAllTextAsync(FilePath);
            cached = JsonSerializer.Deserialize<List<SavedScan>>(raw, JsonOptions) ?? [];
        }
        catch
        {
            // Corrupt file: start fresh rather than crash.
            cached = [];
        }

        return cached;
    }
}