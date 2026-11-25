namespace MauiNotes.Services.Persistence;

public interface INotePersistenceService
{
    Task<NoteStoreDocument?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(NoteStoreDocument document, CancellationToken cancellationToken);
}
