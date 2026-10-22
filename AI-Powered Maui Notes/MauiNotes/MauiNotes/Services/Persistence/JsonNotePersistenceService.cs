using System.Text.Json;

namespace MauiNotes.Services.Persistence;

public sealed class JsonNotePersistenceService : INotePersistenceService
{
    private const int CurrentVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly SemaphoreSlim saveLock = new(1, 1);
    private readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "maui-notes.json");

    public NoteStoreDocument? Load()
    {
        if (!File.Exists(filePath))
            return null;

        string json = File.ReadAllText(filePath);
        NoteStoreDocument document = JsonSerializer.Deserialize<NoteStoreDocument>(json, JsonOptions)
            ?? throw new InvalidDataException("The saved notes file is empty or invalid.");
        Validate(document);
        return document;
    }

    public async Task SaveAsync(
        NoteStoreDocument document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        Validate(document);
        await saveLock.WaitAsync(cancellationToken);
        string temporaryPath = filePath + ".tmp";
        bool moved = false;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            string json = JsonSerializer.Serialize(document, JsonOptions);
            await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);
            File.Move(temporaryPath, filePath, overwrite: true);
            moved = true;
        }
        finally
        {
            try
            {
                if (!moved && File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            finally
            {
                saveLock.Release();
            }
        }
    }

    private static void Validate(NoteStoreDocument document)
    {
        if (document.Version != CurrentVersion)
            throw new InvalidDataException($"Unsupported notes file version: {document.Version}.");
        if (document.Folders is null)
            throw new InvalidDataException("The saved notes file has no folder collection.");

        var ids = new HashSet<Guid>();
        foreach (NoteFolderData folder in document.Folders)
            ValidateFolder(folder, ids);
    }

    private static void ValidateFolder(NoteFolderData folder, HashSet<Guid> ids)
    {
        if (folder is null || folder.Id == Guid.Empty || !ids.Add(folder.Id))
            throw new InvalidDataException("The saved notes file contains an invalid or duplicate folder ID.");
        if (string.IsNullOrWhiteSpace(folder.Title) || folder.Folders is null || folder.Notes is null)
            throw new InvalidDataException("The saved notes file contains an invalid folder.");

        foreach (NoteFolderData child in folder.Folders)
            ValidateFolder(child, ids);

        foreach (NoteData note in folder.Notes)
        {
            if (note is null || note.Id == Guid.Empty || !ids.Add(note.Id) ||
                note.Content is null || note.Title is null)
            {
                throw new InvalidDataException("The saved notes file contains an invalid or duplicate note.");
            }
        }
    }
}
