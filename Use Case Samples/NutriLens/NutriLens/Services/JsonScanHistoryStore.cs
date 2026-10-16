using System.Text.Json;
using NutriLens.Models;

namespace NutriLens.Services;

public sealed class SavedScan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;
    public string ImagePath { get; set; } = string.Empty;
    public IngredientAnalysisResult Result { get; set; } = new();
}

public interface IScanHistoryStore
{
    Task<IReadOnlyList<SavedScan>> GetAllAsync();
    Task<SavedScan> AddAsync(IngredientAnalysisResult result, string? imagePath = null);
    Task ClearAsync();
}

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
            return (await LoadAsync()).ToList().AsReadOnly();
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
        ArgumentNullException.ThrowIfNull(result);

        await gate.WaitAsync();
        try
        {
            var list = await LoadAsync();
            var persistedImagePath = await PersistImageAsync(imagePath);

            var existing = list.FirstOrDefault(scan =>
                string.Equals(scan.Result.ProductName, result.ProductName,
                    StringComparison.OrdinalIgnoreCase) &&
                scan.Result.Score == result.Score);

            SavedScan savedScan;
            if (existing is not null)
            {
                existing.SavedAtUtc = DateTime.UtcNow;
                existing.ImagePath = persistedImagePath ?? existing.ImagePath;
                existing.Result = result;
                list.Remove(existing);
                list.Insert(0, existing);
                savedScan = existing;
            }
            else
            {
                savedScan = new SavedScan
                {
                    Result = result,
                    ImagePath = persistedImagePath ?? string.Empty
                };
                list.Insert(0, savedScan);
            }

            await File.WriteAllTextAsync(
                FilePath,
                JsonSerializer.Serialize(list, JsonOptions));

            cached = list;
            return savedScan;
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
            cached = [];
        }

        return cached;
    }

    private static async Task<string?> PersistImageAsync(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            return null;

        var imageDirectory = Path.Combine(FileSystem.AppDataDirectory, "scan-images");
        Directory.CreateDirectory(imageDirectory);

        var destination = Path.Combine(
            imageDirectory,
            $"{Guid.NewGuid():N}{Path.GetExtension(imagePath)}");

        await using var source = File.OpenRead(imagePath);
        await using var target = File.Create(destination);
        await source.CopyToAsync(target);

        return destination;
    }
}