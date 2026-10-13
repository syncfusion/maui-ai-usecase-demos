using System.Text.Json;
using MauiNotes.Models;
using MauiNotes.Services.Persistence;

namespace MauiNotes.ViewModels;

public partial class NotesViewModel
{
    private bool isLoadingData;

    private void LoadInitialData()
    {
        isLoadingData = true;
        try
        {
            LoadSampleData();
        }
        finally
        {
            isLoadingData = false;
            ScheduleAutosave();
        }
    }

    private async Task<NoteStoreDocument> CreateStoreDocumentAsync(
        CancellationToken cancellationToken)
    {
        NoteTreeNode[] roots = TreeNodes.ToArray();
        var progress = new SnapshotProgress();
        var folders = new List<NoteFolderData>(roots.Length);

        foreach (NoteTreeNode root in roots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (root.IsFolder)
            {
                folders.Add(await CreateFolderDataAsync(root, progress, cancellationToken));
            }

            if (++progress.NodesSinceYield >= 16)
            {
                progress.NodesSinceYield = 0;
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        return new NoteStoreDocument
        {
            Folders = folders,
            SelectedFolderId = SelectedFolderNode?.Id,
            SelectedNoteId = SelectedNote?.Id
        };
    }

    private static async Task<NoteFolderData> CreateFolderDataAsync(
        NoteTreeNode folder,
        SnapshotProgress progress,
        CancellationToken cancellationToken)
    {
        NoteTreeNode[] children = folder.Children.ToArray();
        var childFolders = new List<NoteFolderData>();
        var notes = new List<NoteData>();

        foreach (NoteTreeNode child in children)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (child.IsFolder)
            {
                childFolders.Add(await CreateFolderDataAsync(child, progress, cancellationToken));
            }
            else if (child.Note is not null)
            {
                notes.Add(CreateNoteData(child.Note));
            }

            if (++progress.NodesSinceYield >= 16)
            {
                progress.NodesSinceYield = 0;
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        return new NoteFolderData
        {
            Id = folder.Id,
            Title = folder.Title,
            Folders = childFolders,
            Notes = notes
        };
    }

    private sealed class SnapshotProgress
    {
        public int NodesSinceYield { get; set; }
    }

    private static NoteData CreateNoteData(NoteModel note) => new()
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        CreatedDate = note.CreatedDate,
        ModifiedDate = note.ModifiedDate,
        IsLocked = note.IsLocked
    };

    private void ScheduleAutosave()
    {
        if (isLoadingData || !isPersistenceAvailable)
            return;

        autosaveCancellation?.Cancel();
        autosaveCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        autosaveCancellation = cancellation;
        _ = SaveAfterDelayAsync(cancellation);
    }

    private async Task SaveAfterDelayAsync(CancellationTokenSource cancellation)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(400), cancellation.Token);
            LogHtmlBoundary("persistence.autosave.selected-note", SelectedNote?.Content, SelectedNote?.Id);
            await notePersistenceService.SaveAsync(
                await CreateStoreDocumentAsync(cancellation.Token),
                cancellation.Token);
            if (ReferenceEquals(autosaveCancellation, cancellation))
                SetPersistenceStatus("Saved", false);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (IOException ex)
        {
            SetPersistenceStatus($"Autosave failed: {ex.Message}", true);
        }
        catch (UnauthorizedAccessException ex)
        {
            SetPersistenceStatus($"Autosave failed: {ex.Message}", true);
        }
        catch (JsonException ex)
        {
            SetPersistenceStatus($"Autosave failed: {ex.Message}", true);
        }
        catch (InvalidDataException ex)
        {
            SetPersistenceStatus($"Autosave failed: {ex.Message}", true);
        }
        finally
        {
            if (ReferenceEquals(autosaveCancellation, cancellation))
            {
                autosaveCancellation = null;
                cancellation.Dispose();
            }
        }
    }

    private async Task SaveNotesAsync()
    {
        if (!CanSaveNotes)
            return;

        autosaveCancellation?.Cancel();
        IsSaving = true;
        SetPersistenceStatus("Saving...", false);
        try
        {
            LogHtmlBoundary("persistence.manual-save.selected-note", SelectedNote?.Content, SelectedNote?.Id);
            await notePersistenceService.SaveAsync(
                await CreateStoreDocumentAsync(CancellationToken.None),
                CancellationToken.None);
            SetPersistenceStatus("Saved", false);
        }
        catch (IOException ex)
        {
            SetPersistenceStatus($"Save failed: {ex.Message}", true);
        }
        catch (UnauthorizedAccessException ex)
        {
            SetPersistenceStatus($"Save failed: {ex.Message}", true);
        }
        catch (JsonException ex)
        {
            SetPersistenceStatus($"Save failed: {ex.Message}", true);
        }
        catch (InvalidDataException ex)
        {
            SetPersistenceStatus($"Save failed: {ex.Message}", true);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void SetPersistenceAvailability(bool available)
    {
        isPersistenceAvailable = available;
        OnPropertyChanged(nameof(IsEditorReadOnly));
        OnPropertyChanged(nameof(IsEditorEditable));
        OnPropertyChanged(nameof(IsAiActionEnabled));
        OnPropertyChanged(nameof(CanRunCustomAi));
        OnPropertyChanged(nameof(CanSaveNotes));
    }

    private void SetPersistenceStatus(string message, bool isError)
    {
        PersistenceStatus = message;
        IsPersistenceStatusError = isError;
    }
}
