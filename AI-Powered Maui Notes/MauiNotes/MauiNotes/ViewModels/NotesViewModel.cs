using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;
using MauiNotes.Models.AI;
using MauiNotes.Models;
using MauiNotes.Services.AI;
using MauiNotes.Services.Persistence;
using CustomRichTextEditor;
using Syncfusion.Maui.RichTextEditor;

namespace MauiNotes.ViewModels;

public partial class NotesViewModel : ViewModelBase
{
    private NoteTreeNode? selectedFolderNode;
    private NoteTreeNode? selectedTreeNode;
    private NoteModel? selectedNote;
    private string searchText = string.Empty;
    private string editorHtml = string.Empty;

    // True while the editor's HTML Value is being set programmatically (note
    // load/switch, rename-driven title sync). The RTE still raises its own
    // value notifications for such a programmatic swap because Value is a
    // two-way binding, so EditorHtml must ignore those callbacks -
    // otherwise merely opening or renaming a note would incorrectly bump
    // ModifiedDate.
    private bool isApplyingProgrammaticContent;
    private object? moveTarget;
    private NoteTreeNode? moveCurrentDestination;
    private bool isMoveToPromptOpen;
    private CustomRichTextEditor.CustomRichTextEditor? activeEditor;
    private bool isEditorFocused;
    private bool isBoldActive;
    private bool isItalicActive;
    private bool isUnderlineActive;
    private bool isStrikethroughActive;
    private bool isHighlightActive;
    private bool isMonostyledActive;
    private bool isBulletListActive;
    private bool isNumberedListActive;
    private string currentParagraphFormat = "Heading1";
    private bool isFormatPopoverOpen;
    private bool isTextPaletteOpen;
    private bool isHighlightPaletteOpen;
    private bool isNodeMenuOpen;
    private string selectedTextColor = "#FF9F0A";
    private string selectedHighlightColor = "#FFFF00";
    private object? menuTarget;
    private NoteTreeNode? activeFolderRenameNode;
    private readonly IAiEditingService aiEditingService;
    private readonly INotePersistenceService notePersistenceService;
    private CancellationTokenSource? aiCancellation;
    private CancellationTokenSource? autosaveCancellation;
    private bool isPersistenceAvailable = true;
    private bool isSaving;
    private bool isPersistenceStatusError;
    private string persistenceStatus = string.Empty;
    private NoteModel? aiSourceNote;
    private CustomRichTextEditor.CustomRichTextEditor? aiEditor;
    private string aiSourceHtml = string.Empty;
    private string aiSelectedText = string.Empty;
    private string aiSelectedHtml = string.Empty;
    private RichTextSelectionSnapshot? aiSelectedSnapshot;
    private bool aiReplaceWholeNote;
    private RichTextReplaceReceipt? aiAppliedReceipt;
    private NoteModel? aiAppliedNote;
    private AiEditingAction? aiLastAction;
    private string aiLastCustomInstruction = string.Empty;
    private bool hasAiReviewResult;
    private bool isAiPopupOpen;
    private bool isAiProcessing;
    private bool isAiReviewPopupOpen;
    private string aiCustomInstruction = string.Empty;
    private string aiError = string.Empty;
    private string aiStatus = string.Empty;
    private int aiRequestVersion;

    public ObservableCollection<NoteTreeNode> TreeNodes { get; } = new();
    public ObservableCollection<NoteTreeNode> MoveDestinationFolders { get; } = new();
    public IReadOnlyList<AiActionListItem> AiActionItems { get; } =
    [
        new(AiEditingAction.GrammarCheck, "Grammar", "\uE765"),
        new(AiEditingAction.Proofread, "Proofread", "\uE75B"),
        new(AiEditingAction.Rewrite, "Rewrite", "\uE710"),
        new(AiEditingAction.FriendlyTone, "Friendly", "\uE7A3", ShowDividerBefore: true),
        new(AiEditingAction.ProfessionalTone, "Professional", "\uE797"),
        new(AiEditingAction.Concise, "Concise", "\uE776"),
        new(AiEditingAction.Summarize, "Summary", "\uE7F4", ShowDividerBefore: true),
        new(AiEditingAction.KeyPoints, "Key Points", "\uE72C"),
        new(AiEditingAction.ConvertToList, "List", "\uE7AE"),
        new(AiEditingAction.ConvertToTable, "Table", "\uE7AF")
    ];

    public event EventHandler? NoteSelected;
    public event Action<NoteTreeNode>? FolderRenameRequested;

    public NoteTreeNode? SelectedTreeNode
    {
        get => selectedTreeNode;
        set
        {
            if (!SetProperty(ref selectedTreeNode, value) || value is null)
                return;

            if (value.Note is not null)
            {
                SelectedFolderNode = value.Parent;
                SelectedNote = value.Note;
                NoteSelected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SelectedFolderNode = value;
            }
        }
    }

    public NoteTreeNode? SelectedFolderNode
    {
        get => selectedFolderNode;
        set
        {
            NoteTreeNode? previousNode = selectedFolderNode;
            if (!SetProperty(ref selectedFolderNode, value))
                return;

            if (previousNode is not null)
                previousNode.IsSelected = false;
            if (value is not null)
                value.IsSelected = true;

            if (!ReferenceEquals(selectedTreeNode?.Parent, value) && !ReferenceEquals(selectedTreeNode, value))
            {
                selectedTreeNode = value;
                OnPropertyChanged(nameof(SelectedTreeNode));
            }

            if (selectedTreeNode?.Note is null)
                SelectedNote = value?.Notes.FirstOrDefault();
            IsNodeMenuOpen = false;
            OnPropertyChanged(nameof(SelectedHeaderText));
            OnPropertyChanged(nameof(SelectedCountText));
            OnPropertyChanged(nameof(IsFolderMenuTarget));
            OnPropertyChanged(nameof(IsEditorReadOnly));
            OnPropertyChanged(nameof(IsEditorEditable));
            OnPropertyChanged(nameof(IsAiActionEnabled));
            OnPropertyChanged(nameof(CanRunCustomAi));
            OnPropertyChanged(nameof(CanUseAiReviewActions));
            OnPropertyChanged(nameof(CanRetryAiRequest));
            OnPropertyChanged(nameof(CanFinishAiReview));
            ScheduleAutosave();
        }
    }

    public NoteModel? SelectedNote
    {
        get => selectedNote;
        set
        {
            NoteModel? previousNote = selectedNote;
            if (previousNote is not null)
                LogHtmlBoundary("note.navigation.previous-model", previousNote.Content, previousNote.Id);
            if (!SetProperty(ref selectedNote, value))
                return;

            if (IsAiPopupOpen || IsAiProcessing)
                CloseAiAssistant();

            if (previousNote is not null)
                previousNote.IsSelected = false;
            if (value is not null)
                value.IsSelected = true;

            // Loading a note's content into the editor must never mutate its
            // timestamps. Assign the private field directly instead of going
            // through the EditorHtml setter, so that opening/selecting a note
            // is never treated as an edit. Guard against the RTE's own
            // Value notification firing for this programmatic HTML update. The
            // native Value assignment (and its resulting notification
            // callback) happens asynchronously on the UI thread after this
            // setter returns, so the flag must be cleared on a later
            // dispatcher cycle rather than immediately/synchronously here.
            isApplyingProgrammaticContent = true;
            string selectedHtml = value?.Content ?? string.Empty;
            LogHtmlBoundary("note.navigation.selected-model", selectedHtml, value?.Id);
            if (value is not null)
            {
                string normalizedHtml = NormalizeEscapedHtml(selectedHtml);
                if (!string.Equals(selectedHtml, normalizedHtml, StringComparison.Ordinal))
                {
                    Debug.WriteLine($"[HTML] Normalized escaped markup in stored note before editor reload; noteId={value.Id}.");
                    value.Content = normalizedHtml;
                    UpdateNoteTitleFromEditor(normalizedHtml);
                    value.PreviewText = ExtractPreviewText(normalizedHtml);
                    selectedHtml = normalizedHtml;
                }
            }

            editorHtml = selectedHtml;
            LogHtmlBoundary("note.navigation.editor-value-assignment", editorHtml, value?.Id);
            OnPropertyChanged(nameof(EditorHtml));
            ClearProgrammaticContentFlagAfterDispatch();
            OnPropertyChanged(nameof(SelectedHeaderText));
            OnPropertyChanged(nameof(IsFolderMenuTarget));
            OnPropertyChanged(nameof(EditorTimestampText));
            OnPropertyChanged(nameof(IsEditorReadOnly));
            OnPropertyChanged(nameof(IsEditorEditable));
            OnPropertyChanged(nameof(IsAiActionEnabled));
            OnPropertyChanged(nameof(CanRunCustomAi));
            ScheduleAutosave();
        }
    }

    public string SelectedHeaderText
    {
        get => "Notes";
    }

    public string SelectedCountText
    {
        get
        {
            int count = TreeNodes.Sum(folder => folder.NoteCount);
            return $"{count} {(count == 1 ? "Note" : "Notes")}";
        }
    }

    /// <summary>
    /// Read-only timestamp shown above the RTE for the selected note, e.g.
    /// "21 September 2026 at 3:08 PM". Never stored inside the RTE HTML.
    /// </summary>
    public string EditorTimestampText => SelectedNote?.EditorTimestampText ?? string.Empty;

    public bool IsEditorReadOnly =>
        SelectedNote is null || SelectedNote.IsLocked || IsAiProcessing ||
        IsAiPopupOpen || !isPersistenceAvailable;
    public bool IsEditorEditable =>
        SelectedNote is not null && !SelectedNote.IsLocked && !IsAiProcessing &&
        !IsAiPopupOpen && isPersistenceAvailable;

    public bool IsSaving
    {
        get => isSaving;
        private set
        {
            if (SetProperty(ref isSaving, value))
                OnPropertyChanged(nameof(CanSaveNotes));
        }
    }

    public bool CanSaveNotes => isPersistenceAvailable && !IsSaving;

    public string PersistenceStatus
    {
        get => persistenceStatus;
        private set
        {
            if (SetProperty(ref persistenceStatus, value))
                OnPropertyChanged(nameof(HasPersistenceStatus));
        }
    }

    public bool HasPersistenceStatus => IsPersistenceStatusError;

    public bool IsPersistenceStatusError
    {
        get => isPersistenceStatusError;
        private set
        {
            if (SetProperty(ref isPersistenceStatusError, value))
                OnPropertyChanged(nameof(HasPersistenceStatus));
        }
    }

    public bool IsMoveToPromptOpen
    {
        get => isMoveToPromptOpen;
        private set => SetProperty(ref isMoveToPromptOpen, value);
    }

    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    public string EditorHtml
    {
        get => editorHtml;
        set
        {
            if (SelectedNote?.IsLocked == true || IsAiProcessing || isApplyingProgrammaticContent)
                return;

            NoteModel? note = SelectedNote;
            if (note is null)
                return;

            string incomingHtml = value ?? string.Empty;
            if (string.Equals(incomingHtml, editorHtml, StringComparison.Ordinal) &&
                string.Equals(editorHtml, note.Content, StringComparison.Ordinal))
            {
                return;
            }

            LogHtmlBoundary("editor.value.binding.input", incomingHtml, note.Id);
            string normalizedHtml = NormalizeEscapedHtml(incomingHtml);
            bool normalizationChangedValue =
                !string.Equals(incomingHtml, normalizedHtml, StringComparison.Ordinal);
            LogHtmlBoundary("editor.value.binding.normalized", normalizedHtml, note.Id);

            string previousCanonicalHtml = NormalizeEscapedHtml(note.Content);
            bool contentChanged =
                !string.Equals(previousCanonicalHtml, normalizedHtml, StringComparison.Ordinal);
            bool storedContentChanged =
                !string.Equals(note.Content, normalizedHtml, StringComparison.Ordinal);

            if (!contentChanged && !storedContentChanged &&
                string.Equals(editorHtml, normalizedHtml, StringComparison.Ordinal))
            {
                UpdateNoteTitleFromEditor(normalizedHtml);
                return;
            }

            // Value is updated by the RTE as the user edits or pastes. Do not
            // raise PropertyChanged back to the same editor here: that echoes
            // the incoming HTML into its Value property and can replace the
            // active document while the paste is still being applied.
            editorHtml = normalizedHtml;
            note.Content = normalizedHtml;
            UpdateNoteTitleFromEditor(normalizedHtml);
            Debug.WriteLine(
                $"[HTML] note.metadata.after-editor-value; noteId={note.Id}; " +
                $"titleLength={note.Title.Length}; previewLength={note.PreviewText.Length}; " +
                $"titleContainsEscapedTag={Regex.IsMatch(note.Title, @"\\+u003[cC]", RegexOptions.IgnoreCase)}; " +
                $"previewContainsEscapedTag={Regex.IsMatch(note.PreviewText, @"\\+u003[cC]", RegexOptions.IgnoreCase)}; " +
                $"normalizationChangedValue={normalizationChangedValue}; contentChanged={contentChanged}.");

            if (contentChanged)
            {
                note.ModifiedDate = DateTime.Now;
                OnPropertyChanged(nameof(EditorTimestampText));
            }

            if (storedContentChanged)
                ScheduleAutosave();

            if (normalizationChangedValue)
            {
                // The incoming value is already being applied by the editor.
                // Push the canonical HTML back after that change notification
                // completes, guarded against a second edit/timestamp update.
                isApplyingProgrammaticContent = true;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (ReferenceEquals(SelectedNote, note) &&
                        string.Equals(editorHtml, normalizedHtml, StringComparison.Ordinal))
                    {
                        OnPropertyChanged(nameof(EditorHtml));
                    }

                    ClearProgrammaticContentFlagAfterDispatch();
                });
            }
        }
    }

    public CustomRichTextEditor.CustomRichTextEditor? ActiveEditor
    {
        get => activeEditor;
        private set => SetProperty(ref activeEditor, value);
    }

    public bool IsEditorFocused
    {
        get => isEditorFocused;
        private set => SetProperty(ref isEditorFocused, value);
    }

    public bool IsBoldActive
    {
        get => isBoldActive;
        private set => SetProperty(ref isBoldActive, value);
    }

    public bool IsItalicActive
    {
        get => isItalicActive;
        private set => SetProperty(ref isItalicActive, value);
    }

    public bool IsUnderlineActive
    {
        get => isUnderlineActive;
        private set => SetProperty(ref isUnderlineActive, value);
    }

    public bool IsStrikethroughActive
    {
        get => isStrikethroughActive;
        private set => SetProperty(ref isStrikethroughActive, value);
    }

    public bool IsHighlightActive
    {
        get => isHighlightActive;
        private set => SetProperty(ref isHighlightActive, value);
    }

    public bool IsMonostyledActive
    {
        get => isMonostyledActive;
        private set => SetProperty(ref isMonostyledActive, value);
    }

    public bool IsBulletListActive
    {
        get => isBulletListActive;
        private set => SetProperty(ref isBulletListActive, value);
    }

    public bool IsNumberedListActive
    {
        get => isNumberedListActive;
        private set => SetProperty(ref isNumberedListActive, value);
    }

    public string CurrentParagraphFormat
    {
        get => currentParagraphFormat;
        private set => SetProperty(ref currentParagraphFormat, value);
    }

    public bool IsFormatPopoverOpen
    {
        get => isFormatPopoverOpen;
        set => SetProperty(ref isFormatPopoverOpen, value);
    }

    public bool IsTextPaletteOpen
    {
        get => isTextPaletteOpen;
        set => SetProperty(ref isTextPaletteOpen, value);
    }

    public bool IsHighlightPaletteOpen
    {
        get => isHighlightPaletteOpen;
        set => SetProperty(ref isHighlightPaletteOpen, value);
    }

    public bool IsNodeMenuOpen
    {
        get => isNodeMenuOpen;
        set => SetProperty(ref isNodeMenuOpen, value);
    }

    public string MenuTargetTitle => menuTarget switch
    {
        NoteModel note => note.Title,
        NoteTreeNode folder => folder.Title,
        _ => string.Empty
    };

    public bool IsFolderMenuTarget => menuTarget is NoteTreeNode;
    public bool IsNoteMenuTarget => menuTarget is NoteModel;
    public string MenuTargetLockActionText => menuTarget is NoteModel { IsLocked: true } ? "Unlock" : "Lock";

    public string SelectedTextColor
    {
        get => selectedTextColor;
        private set => SetProperty(ref selectedTextColor, value);
    }

    public string SelectedHighlightColor
    {
        get => selectedHighlightColor;
        private set => SetProperty(ref selectedHighlightColor, value);
    }

    public bool IsAiPopupOpen
    {
        get => isAiPopupOpen;
        private set
        {
            if (!SetProperty(ref isAiPopupOpen, value))
                return;

            OnPropertyChanged(nameof(IsEditorReadOnly));
            OnPropertyChanged(nameof(IsEditorEditable));
            OnPropertyChanged(nameof(IsAiActionEnabled));
            OnPropertyChanged(nameof(CanRunCustomAi));
        }
    }

    public bool IsAiProcessing
    {
        get => isAiProcessing;
        private set
        {
            if (!SetProperty(ref isAiProcessing, value))
                return;

            OnPropertyChanged(nameof(IsEditorReadOnly));
            OnPropertyChanged(nameof(IsEditorEditable));
            OnPropertyChanged(nameof(IsAiActionEnabled));
            OnPropertyChanged(nameof(CanRunCustomAi));
            OnPropertyChanged(nameof(CanUseAiReviewActions));
            OnPropertyChanged(nameof(CanRetryAiRequest));
            OnPropertyChanged(nameof(CanFinishAiReview));
            OnPropertyChanged(nameof(IsAiReviewLoading));
        }
    }

    public bool IsAiReviewLoading => IsAiProcessing && isAiReviewPopupOpen;

    internal void SetAiReviewPopupOpen(bool isOpen)
    {
        if (isAiReviewPopupOpen == isOpen)
            return;

        isAiReviewPopupOpen = isOpen;
        OnPropertyChanged(nameof(IsAiReviewLoading));
    }

    public bool IsAiActionEnabled =>
        SelectedNote is not null && !SelectedNote.IsLocked &&
        !IsAiProcessing && isPersistenceAvailable;

    public string AiCustomInstruction
    {
        get => aiCustomInstruction;
        set
        {
            if (SetProperty(ref aiCustomInstruction, value))
                OnPropertyChanged(nameof(CanRunCustomAi));
        }
    }

    public bool CanRunCustomAi => IsAiActionEnabled && IsAiPopupOpen &&
        !string.IsNullOrWhiteSpace(AiCustomInstruction);

    public string AiError
    {
        get => aiError;
        private set
        {
            if (SetProperty(ref aiError, value))
            {
                if (!string.IsNullOrWhiteSpace(value))
                    Debug.WriteLine($"[AI] AiError: {value}");
                OnPropertyChanged(nameof(HasAiError));
                OnPropertyChanged(nameof(CanRetryAiRequest));
            }
        }
    }

    public bool HasAiError => !string.IsNullOrWhiteSpace(AiError);
    public string AiStatus
    {
        get => aiStatus;
        private set
        {
            if (SetProperty(ref aiStatus, value))
                OnPropertyChanged(nameof(HasAiStatus));
        }
    }

    public bool HasAiStatus => !string.IsNullOrWhiteSpace(AiStatus);

    public bool HasAiReviewResult => hasAiReviewResult;
    internal string AiSelectionSnapshotDebugState =>
        aiSelectedSnapshot is null
            ? "null"
            : $"available; range={aiSelectedSnapshot}; revision={aiSelectedSnapshot.DocumentRevision}";
    public bool CanUseAiReviewActions => HasAiReviewResult && !IsAiProcessing;
    public bool CanRetryAiRequest =>
        !IsAiProcessing && aiLastAction.HasValue && (HasAiReviewResult || HasAiError);
    public bool CanFinishAiReview => CanUseAiReviewActions;

    public ICommand ToolbarActionCommand { get; }
    public ICommand NewNoteCommand { get; }
    public ICommand CreateNoteFromMenuCommand { get; }
    public ICommand CreateRootFolderCommand { get; }
    public ICommand CreateSubfolderCommand { get; }
    public ICommand BeginMoveToCommand { get; }
    public ICommand SelectMoveDestinationCommand { get; }
    public ICommand CancelMoveToCommand { get; }
    public ICommand ToggleNoteLockCommand { get; }
    public ICommand DuplicateNoteCommand { get; }
    public ICommand OpenNoteCommand { get; }
    public ICommand ToggleNodeMenuCommand { get; }
    public ICommand BeginRenameCommand { get; }
    public ICommand DeleteNodeCommand { get; }
    public ICommand AiActionCommand { get; }
    public ICommand AiCustomCommand { get; }
    public ICommand CloseAiAssistantCommand { get; }
    public ICommand CancelAiProcessingCommand { get; }
    public ICommand CancelAiReviewCommand { get; }
    public ICommand RevertAiCommand { get; }
    public ICommand RetryAiCommand { get; }
    public ICommand UndoAiCommand { get; }
    public ICommand RedoAiCommand { get; }
    public ICommand DoneAiCommand { get; }
    public ICommand SaveNotesCommand { get; }

    public NotesViewModel(
        IAiEditingService aiEditingService,
        INotePersistenceService notePersistenceService)
    {
        this.aiEditingService = aiEditingService;
        this.notePersistenceService = notePersistenceService;
        ToolbarActionCommand = new Command<string>(ExecuteToolbarAction);
        NewNoteCommand = new Command(CreateNote);
        CreateNoteFromMenuCommand = new Command(CreateNoteFromMenuTarget);
        CreateRootFolderCommand = new Command(CreateRootFolder);
        CreateSubfolderCommand = new Command(CreateSubfolder);
        BeginMoveToCommand = new Command(BeginMoveTo);
        SelectMoveDestinationCommand = new Command<object>(
            SelectMoveDestination,
            CanSelectMoveDestination);
        CancelMoveToCommand = new Command(CancelMoveTo);
        ToggleNoteLockCommand = new Command(ToggleNoteLock);
        DuplicateNoteCommand = new Command(DuplicateNote);
        OpenNoteCommand = new Command<object>(OpenNote);
        ToggleNodeMenuCommand = new Command<object>(ToggleNodeMenu);
        BeginRenameCommand = new Command(BeginRename);
        DeleteNodeCommand = new Command(DeleteSelectedItem);
        AiActionCommand = new Command<string>(async action =>
        {
            if (Enum.TryParse(action, out AiEditingAction parsedAction) &&
                parsedAction != AiEditingAction.Custom)
            {
                await StartAiRequestAsync(parsedAction, string.Empty);
            }
        });
        AiCustomCommand = new Command(async () =>
            await StartAiRequestAsync(AiEditingAction.Custom, AiCustomInstruction));
        CloseAiAssistantCommand = new Command(CloseAiAssistant);
        CancelAiProcessingCommand = new Command(CancelAiProcessing);
        CancelAiReviewCommand = new Command(async () => await CancelAiReviewAsync());
        RevertAiCommand = new Command(async () => await RevertAiResultAsync());
        RetryAiCommand = new Command(async () => await RetryAiRequestAsync());
        UndoAiCommand = new Command(async () => await ApplyEditorHistoryActionAsync(undo: true));
        RedoAiCommand = new Command(async () => await ApplyEditorHistoryActionAsync(undo: false));
        DoneAiCommand = new Command(CloseAiAssistant);
        SaveNotesCommand = new Command(async () => await SaveNotesAsync());
        LoadInitialData();
    }

}
