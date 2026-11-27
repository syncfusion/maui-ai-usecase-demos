namespace MauiNotes.Services.Persistence;

public interface INotePersistenceService
{
    Task<NoteStoreDocument?> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(NoteStoreDocument document, CancellationToken cancellationToken);
}
