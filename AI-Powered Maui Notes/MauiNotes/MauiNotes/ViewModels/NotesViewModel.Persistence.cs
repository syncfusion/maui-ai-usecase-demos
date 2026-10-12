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

    private NoteStoreDocument CreateStoreDocument() => new()
    {
        Folders = TreeNodes
            .Where(node => node.IsFolder)
            .Select(CreateFolderData)
            .ToList(),
        SelectedFolderId = SelectedFolderNode?.Id,
        SelectedNoteId = SelectedNote?.Id
    };

    private static NoteFolderData CreateFolderData(NoteTreeNode folder) => new()
    {
        Id = folder.Id,
        Title = folder.Title,
        Folders = folder.Children
            .Where(child => child.IsFolder)
            .Select(CreateFolderData)
            .ToList(),
        Notes = folder.Children
            .Where(child => child.Note is not null)
            .Select(child => CreateNoteData(child.Note!))
            .ToList()
    };

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
                CreateStoreDocument(),
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
                CreateStoreDocument(),
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
