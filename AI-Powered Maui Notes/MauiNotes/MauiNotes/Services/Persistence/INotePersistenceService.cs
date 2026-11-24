namespace MauiNotes.Services.Persistence;

public interface INotePersistenceService
{
    NoteStoreDocument? Load();

    Task SaveAsync(NoteStoreDocument document, CancellationToken cancellationToken);
}
