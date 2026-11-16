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
            NoteStoreDocument? document = null;
            try
            {
                document = notePersistenceService.Load();
            }
            catch (IOException ex)
            {
                SetPersistenceStatus($"Could not load saved notes: {ex.Message}", true);
            }
            catch (InvalidDataException ex)
            {
                SetPersistenceStatus($"Saved notes are corrupt and could not be loaded: {ex.Message}", true);
            }

            if (document is not null)
            {
                LoadFromStoreDocument(document);
            }
            else
            {
                LoadSampleData();
            }
        }
        finally
        {
            isLoadingData = false;
            ScheduleAutosave();
        }
    }

    private void LoadFromStoreDocument(NoteStoreDocument document)
    {
        foreach (NoteFolderData root in document.Folders)
            AddFolderRecursive(root, null);

        RecalculateCounts();

        SelectedFolderNode =
            document.SelectedFolderId is { } folderId
                ? FindFolderById(folderId, TreeNodes)
                : TreeNodes.FirstOrDefault();
        SelectedNote =
            document.SelectedNoteId is { } noteId
                ? FindNoteById(noteId, TreeNodes)
                : SelectedFolderNode?.Notes.FirstOrDefault();
        SelectedTreeNode = SelectedNote is not null
            ? FindNoteNode(SelectedNote, TreeNodes)
            : SelectedFolderNode;
    }

    private void AddFolderRecursive(NoteFolderData folder, NoteTreeNode? parent)
    {
        var treeFolder = new NoteTreeNode
        {
            Title = folder.Title,
            Parent = parent
        };
        if (parent is null)
            TreeNodes.Add(treeFolder);
        else
            parent.Children.Add(treeFolder);

        foreach (NoteFolderData child in folder.Folders)
            AddFolderRecursive(child, treeFolder);

        foreach (NoteData note in folder.Notes)
        {
            var model = new NoteModel
            {
                Title = note.Title,
                Content = note.Content,
                PreviewText = ExtractPreviewText(note.Content),
                CreatedDate = note.CreatedDate,
                ModifiedDate = note.ModifiedDate,
                IsLocked = note.IsLocked
            };
            treeFolder.Notes.Add(model);
            AddNoteTreeNode(treeFolder, model);
        }
    }

    private static NoteTreeNode? FindFolderById(Guid id, IEnumerable<NoteTreeNode> folders)
    {
        foreach (NoteTreeNode folder in folders)
        {
            if (folder.Id == id)
                return folder;

            NoteTreeNode? match = FindFolderById(id, folder.Children.Where(child => child.IsFolder));
            if (match is not null)
                return match;
        }

        return null;
    }

    private static NoteModel? FindNoteById(Guid id, IEnumerable<NoteTreeNode> folders)
    {
        foreach (NoteTreeNode folder in folders)
        {
            NoteModel? note = folder.Notes.FirstOrDefault(n => n.Id == id);
            if (note is not null)
                return note;

            NoteModel? match = FindNoteById(id, folder.Children.Where(child => child.IsFolder));
            if (match is not null)
                return match;
        }

        return null;
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
