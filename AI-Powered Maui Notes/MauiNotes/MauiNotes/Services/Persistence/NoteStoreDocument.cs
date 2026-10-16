namespace MauiNotes.Services.Persistence;

public sealed class NoteStoreDocument
{
    public int Version { get; set; } = 1;
    public List<NoteFolderData> Folders { get; set; } = new();
    public Guid? SelectedFolderId { get; set; }
    public Guid? SelectedNoteId { get; set; }
}

public sealed class NoteFolderData
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<NoteFolderData> Folders { get; set; } = new();
    public List<NoteData> Notes { get; set; } = new();
}

public sealed class NoteData
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    // DateTimeOffset so the local-kind information survives the JSON
    // round-trip (a local DateTime deserializes back as Kind=Unspecified).
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
    public bool IsLocked { get; set; }
}
