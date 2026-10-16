using System.Net;
using System.Text.RegularExpressions;
using MauiNotes.Models;

namespace MauiNotes.ViewModels;

public partial class NotesViewModel
{
    private void LoadSampleData()
    {
        DateTimeOffset today = new DateTimeOffset(DateTime.Today, TimeSpan.Zero);

        var work = CreateFolder("Work", null);
        var meetings = CreateFolder("Meetings", work);
        var architecture = CreateFolder("Architecture", work);

        AddNote(meetings, "Sprint Review", today.AddHours(10).AddMinutes(45), CreateSprintReviewContent());
        AddNote(meetings, "Client Discussion", today.AddHours(14).AddMinutes(12), CreateClientDiscussionContent());
        AddNote(architecture, "Project Planning", DateTimeOffset.Now, CreateProjectPlanningHtml());
        AddNote(architecture, "Database Design", today.AddDays(-3).AddHours(11), CreateDatabaseDesignContent());

        var personal = CreateFolder("Personal", null);
        AddNote(personal, "Travel Checklist", today.AddDays(-1).AddHours(18), CreateTravelChecklistContent());
        AddNote(personal, "Shopping List", today.AddDays(-1).AddHours(19), CreateShoppingListContent());
        AddNote(personal, "Workspace Setup", today.AddDays(-4).AddHours(9), CreateWorkspaceSetupContent());
        AddNote(personal, "Weekly Review", today.AddDays(-6).AddHours(16), CreateWeeklyReviewContent());

        var ideas = CreateFolder("Ideas", null);
        AddNote(ideas, "Product Ideas", today.AddDays(-16).AddHours(10), CreateProductIdeasContent());

        TreeNodes.Add(work);
        TreeNodes.Add(personal);
        TreeNodes.Add(ideas);

        RecalculateCounts();
        SelectedFolderNode = architecture;
        SelectedNote = architecture.Notes.FirstOrDefault(n => n.Title == "Project Planning") ?? architecture.Notes.FirstOrDefault();
        SelectedTreeNode = FindNoteNode(SelectedNote, TreeNodes) ?? architecture;
    }
    private static NoteTreeNode CreateFolder(string title, NoteTreeNode? parent)
    {
        var folder = new NoteTreeNode
        {
            Title = title,
            Parent = parent
        };
        parent?.Children.Add(folder);
        return folder;
    }
    private static void AddNote(NoteTreeNode folder, string title, DateTimeOffset sampleDate, string content)
    {
        // The title is never stored independently: it is always derived from
        // the first line (the <h1> block) of the note's HTML content. The
        // "title" parameter here is only used to build that first line for
        // sample data convenience.
        //
        // sampleDate is computed relative to DateTime.Today/DateTime.Now by
        // the caller (see LoadSampleData), so the demo always shows correct
        // Today/Yesterday/previous-week/older categorization no matter when
        // the app is run. Both CreatedDate and ModifiedDate start out equal;
        // ModifiedDate only diverges once the user actually edits the note.
        string normalizedContent = EnsureTitleHeading(content, title);
        var note = new NoteModel
        {
            Title = ExtractFirstLineText(normalizedContent),
            Content = normalizedContent,
            PreviewText = ExtractPreviewText(normalizedContent),
            CreatedDate = sampleDate,
            ModifiedDate = sampleDate
        };
        folder.Notes.Add(note);
        AddNoteTreeNode(folder, note);
    }

    /// <summary>
    /// Ensures the given HTML content starts with an &lt;h1&gt; block containing
    /// the supplied title. If the content already starts with an &lt;h1&gt;, it is
    /// left untouched (its own text becomes the title).
    /// </summary>
    private static string EnsureTitleHeading(string content, string fallbackTitle)
    {
        if (Regex.IsMatch(content, @"^\s*<h1\b", RegexOptions.IgnoreCase))
            return content;

        return $"<h1>{WebUtility.HtmlEncode(fallbackTitle)}</h1>{content}";
    }
    private static void AddNoteTreeNode(NoteTreeNode folder, NoteModel note)
    {
        var noteNode = new NoteTreeNode
        {
            Note = note,
            Parent = folder
        };

        InsertNoteTreeNode(folder, noteNode);
    }
    private static void InsertNoteTreeNode(NoteTreeNode folder, NoteTreeNode noteNode)
    {
        NoteModel note = noteNode.Note!;
        int index = folder.Children.TakeWhile(child => child.IsFolder).Count();
        while (index < folder.Children.Count &&
               folder.Children[index].Note is { } existingNote &&
               existingNote.ModifiedDate >= note.ModifiedDate)
        {
            index++;
        }

        noteNode.Parent = folder;
        folder.Children.Insert(index, noteNode);
    }
    private void CreateNote()
    {
        CreateNoteInFolder(SelectedFolderNode ?? TreeNodes.FirstOrDefault());
    }
    private void CreateNoteFromMenuTarget()
    {
        if (menuTarget is not NoteModel note)
            return;

        CreateNoteInFolder(FindFolderContainingNote(note, TreeNodes));
    }
    private void CreateNoteInFolder(NoteTreeNode? folder)
    {
        if (folder is null)
            return;

        const string newNoteContent = "<h1>New Note</h1><p></p>";
        DateTimeOffset now = DateTimeOffset.Now;
        var note = new NoteModel
        {
            Title = ExtractFirstLineText(newNoteContent),
            Content = newNoteContent,
            PreviewText = ExtractPreviewText(newNoteContent),
            CreatedDate = now,
            ModifiedDate = now
        };

        folder.Notes.Insert(0, note);
        AddNoteTreeNode(folder, note);
        RecalculateCounts();
        OnPropertyChanged(nameof(SelectedCountText));
        SelectedNote = note;
        SelectedTreeNode = FindNoteNode(note, TreeNodes);
        IsNodeMenuOpen = false;
        ScheduleAutosave();
    }
    private void CreateRootFolder()
    {
        var folder = new NoteTreeNode { Title = GetUniqueFolderTitle(TreeNodes, "New Folder") };
        TreeNodes.Add(folder);
        RecalculateCounts();
        SelectedFolderNode = folder;
        SelectedTreeNode = folder;
        OnPropertyChanged(nameof(SelectedHeaderText));
        OnPropertyChanged(nameof(SelectedCountText));
        ScheduleAutosave();
    }
    private void CreateSubfolder()
    {
        if (menuTarget is not NoteTreeNode parent)
            return;

        var folder = new NoteTreeNode
        {
            Title = GetUniqueFolderTitle(parent.Children.Where(child => child.IsFolder), "New Folder"),
            Parent = parent
        };
        int index = parent.Children.TakeWhile(child => child.IsFolder).Count();
        parent.Children.Insert(index, folder);
        RecalculateCounts();
        SelectedTreeNode = folder;
        IsNodeMenuOpen = false;
        OnPropertyChanged(nameof(SelectedHeaderText));
        OnPropertyChanged(nameof(SelectedCountText));
        ScheduleAutosave();
    }
    private static string GetUniqueFolderTitle(IEnumerable<NoteTreeNode> siblings, string baseTitle)
    {
        var titles = siblings.Select(folder => folder.Title).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!titles.Contains(baseTitle))
            return baseTitle;

        for (int suffix = 2; ; suffix++)
        {
            string candidate = $"{baseTitle} {suffix}";
            if (!titles.Contains(candidate))
                return candidate;
        }
    }
    private void BeginMoveTo()
    {
        CancelMoveTo();

        if (menuTarget is not NoteModel && menuTarget is not NoteTreeNode { IsFolder: true })
            return;

        moveTarget = menuTarget;
        moveCurrentDestination = moveTarget switch
        {
            NoteModel note => FindFolderContainingNote(note, TreeNodes),
            NoteTreeNode folder => folder.Parent,
            _ => null
        };

        foreach (NoteTreeNode root in TreeNodes)
            AddMoveDestinations(root);

        IsNodeMenuOpen = false;
        IsMoveToPromptOpen = MoveDestinationFolders.Count > 0;
    }
    private void AddMoveDestinations(NoteTreeNode folder)
    {
        bool isInvalidFolderDestination = moveTarget is NoteTreeNode movingFolder &&
            (IsWithinFolder(folder, movingFolder) || ReferenceEquals(movingFolder.Parent, folder));
        bool isCurrentDestination = ReferenceEquals(moveCurrentDestination, folder);

        if (!isInvalidFolderDestination || isCurrentDestination)
            MoveDestinationFolders.Add(folder);

        foreach (NoteTreeNode childFolder in folder.Children.Where(child => child.IsFolder))
            AddMoveDestinations(childFolder);
    }
    private bool CanSelectMoveDestination(object? parameter)
    {
        return parameter is NoteTreeNode destination &&
            IsMoveToPromptOpen &&
            MoveDestinationFolders.Contains(destination) &&
            !ReferenceEquals(moveCurrentDestination, destination) &&
            moveTarget is not null;
    }
    private void SelectMoveDestination(object? parameter)
    {
        if (!CanSelectMoveDestination(parameter) ||
            parameter is not NoteTreeNode destination ||
            moveTarget is null)
        {
            CancelMoveTo();
            return;
        }

        object target = moveTarget;
        try
        {
            MoveTreeItemToFolder(target, destination);
        }
        finally
        {
            CancelMoveTo();
        }
    }
    private void CancelMoveTo()
    {
        IsMoveToPromptOpen = false;
        moveTarget = null;
        moveCurrentDestination = null;
        MoveDestinationFolders.Clear();
    }
    public bool MoveTreeNodeToFolder(NoteTreeNode item, NoteTreeNode destination)
        => MoveTreeItemToFolder(item.Note is null ? item : item.Note, destination);
    public void SynchronizeAfterTreeDrag(NoteTreeNode item)
    {
        NoteTreeNode? destination = FindFolderContainingNode(item, TreeNodes);
        if (item.Note is not null)
        {
            NoteTreeNode? source = FindFolderContainingNote(item.Note, TreeNodes);
            if (destination is null)
                return;

            if (source is not null && !ReferenceEquals(source, destination))
                source.Notes.Remove(item.Note);
            if (!destination.Notes.Contains(item.Note))
                destination.Notes.Insert(0, item.Note);
            item.Parent = destination;
            if (ReferenceEquals(SelectedNote, item.Note))
                SelectedFolderNode = destination;
        }
        else if (destination is not null || TreeNodes.Contains(item))
        {
            item.Parent = destination;
        }

        RecalculateCounts();
        OnPropertyChanged(nameof(SelectedCountText));
        OnPropertyChanged(nameof(SelectedHeaderText));
        ScheduleAutosave();
    }
    private static NoteTreeNode? FindFolderContainingNode(NoteTreeNode target, IEnumerable<NoteTreeNode> folders)
    {
        foreach (NoteTreeNode folder in folders)
        {
            if (folder.Children.Contains(target))
                return folder;

            NoteTreeNode? match = FindFolderContainingNode(
                target, folder.Children.Where(child => child.IsFolder));
            if (match is not null)
                return match;
        }

        return null;
    }
    private bool MoveTreeItemToFolder(object item, NoteTreeNode destination)
    {
        if (!destination.IsFolder)
            return false;

        if (item is NoteModel note)
            return MoveNoteToFolder(note, destination);

        if (item is NoteTreeNode { IsFolder: true } folder)
            return MoveFolderToFolder(folder, destination);

        return false;
    }
    private bool MoveNoteToFolder(NoteModel note, NoteTreeNode destination)
    {
        NoteTreeNode? source = FindFolderContainingNote(note, TreeNodes);
        if (source is null || ReferenceEquals(source, destination))
            return false;

        NoteTreeNode? noteNode = FindNoteNode(note, TreeNodes);
        if (noteNode is null)
            return false;

        source.Notes.Remove(note);
        source.Children.Remove(noteNode);
        destination.Notes.Remove(note);
        destination.Children.Remove(noteNode);
        destination.Notes.Insert(0, note);
        InsertNoteTreeNode(destination, noteNode);

        RecalculateCounts();
        OnPropertyChanged(nameof(SelectedCountText));
        OnPropertyChanged(nameof(SelectedHeaderText));
        ScheduleAutosave();
        if (ReferenceEquals(SelectedNote, note))
            SelectedFolderNode = destination;

        return true;
    }
    private bool MoveFolderToFolder(NoteTreeNode folder, NoteTreeNode destination)
    {
        if (ReferenceEquals(folder, destination) || IsWithinFolder(destination, folder) ||
            ReferenceEquals(folder.Parent, destination))
            return false;

        if (folder.Parent is NoteTreeNode sourceParent)
            sourceParent.Children.Remove(folder);
        else
            TreeNodes.Remove(folder);

        destination.Children.Remove(folder);
        folder.Parent = destination;
        int index = destination.Children.TakeWhile(child => child.IsFolder).Count();
        destination.Children.Insert(index, folder);

        RecalculateCounts();
        OnPropertyChanged(nameof(SelectedCountText));
        OnPropertyChanged(nameof(SelectedHeaderText));
        ScheduleAutosave();
        return true;
    }
    private void ToggleNoteLock()
    {
        if (menuTarget is not NoteModel note)
            return;

        note.IsLocked = !note.IsLocked;
        ScheduleAutosave();
        OnPropertyChanged(nameof(MenuTargetLockActionText));
        if (ReferenceEquals(SelectedNote, note))
        {
            OnPropertyChanged(nameof(IsEditorReadOnly));
            OnPropertyChanged(nameof(IsEditorEditable));
            OnPropertyChanged(nameof(IsAiActionEnabled));
            OnPropertyChanged(nameof(CanRunCustomAi));
            if (note.IsLocked && IsAiPopupOpen)
                CloseAiAssistant();
        }
    }
    private void DuplicateNote()
    {
        if (menuTarget is not NoteModel sourceNote)
            return;

        NoteTreeNode? folder = FindFolderContainingNote(sourceNote, TreeNodes);
        NoteTreeNode? sourceNode = FindNoteNode(sourceNote, TreeNodes);
        if (folder is null || sourceNode is null)
            return;

        var duplicate = new NoteModel
        {
            Title = sourceNote.Title,
            Content = sourceNote.Content,
            PreviewText = sourceNote.PreviewText,
            CreatedDate = DateTimeOffset.Now,
            ModifiedDate = sourceNote.ModifiedDate
        };
        int noteIndex = folder.Notes.IndexOf(sourceNote);
        folder.Notes.Insert(noteIndex < 0 ? folder.Notes.Count : noteIndex + 1, duplicate);

        int childIndex = folder.Children.IndexOf(sourceNode);
        var duplicateNode = new NoteTreeNode { Note = duplicate, Parent = folder };
        folder.Children.Insert(childIndex + 1, duplicateNode);

        RecalculateCounts();
        OnPropertyChanged(nameof(SelectedCountText));
        SelectedTreeNode = duplicateNode;
        IsNodeMenuOpen = false;
        ScheduleAutosave();
    }

    /// <summary>
    /// The TreeView title is never an independent source of truth: it is
    /// always re-derived from the note's own editor content (the first
    /// &lt;h1&gt; line). This keeps a single source of truth in the ViewModel.
    /// </summary>
    private void OpenNote(object? parameter)
    {
        NoteTreeNode? noteNode = parameter switch
        {
            NoteTreeNode { Note: not null } node => node,
            NoteModel note => FindNoteNode(note, TreeNodes),
            _ => null
        };

        if (noteNode?.Note is not NoteModel selectedNote)
            return;

        // Select the note explicitly instead of relying only on the TreeView
        // selection callback. This keeps the editor and tree synchronized even
        // after a new note has been inserted into the collection.
        if (!ReferenceEquals(selectedTreeNode, noteNode))
        {
            selectedTreeNode = noteNode;
            OnPropertyChanged(nameof(SelectedTreeNode));
        }

        SelectedFolderNode = noteNode.Parent;
        SelectedNote = selectedNote;
        NoteSelected?.Invoke(this, EventArgs.Empty);
    }
    private void ToggleNodeMenu(object? parameter)
    {
        menuTarget = parameter is NoteTreeNode node
            ? (object?)node.Note ?? node
            : parameter;
        IsNodeMenuOpen = true;
        OnPropertyChanged(nameof(IsFolderMenuTarget));
        OnPropertyChanged(nameof(IsNoteMenuTarget));
        OnPropertyChanged(nameof(MenuTargetTitle));
        OnPropertyChanged(nameof(MenuTargetLockActionText));
    }
    private void BeginRename()
    {
        if (menuTarget is not NoteTreeNode { IsFolder: true } folder)
            return;

        if (activeFolderRenameNode is not null)
            CancelFolderRename(activeFolderRenameNode);

        folder.RenameText = folder.Title;
        folder.RenameValidationMessage = string.Empty;
        folder.IsRenameInvalid = false;
        folder.IsRenaming = true;
        activeFolderRenameNode = folder;
        IsNodeMenuOpen = false;
        FolderRenameRequested?.Invoke(folder);
    }

    public void CommitFolderRename(NoteTreeNode folder)
    {
        if (!ReferenceEquals(activeFolderRenameNode, folder) || !folder.IsRenaming)
            return;

        string newTitle = folder.RenameText.Trim();
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            folder.RenameValidationMessage = "Folder name cannot be empty.";
            folder.IsRenameInvalid = true;
            return;
        }

        if (!string.Equals(newTitle, folder.Title, StringComparison.Ordinal))
            folder.Title = newTitle;

        folder.RenameValidationMessage = string.Empty;
        folder.IsRenameInvalid = false;
        folder.IsRenaming = false;
        activeFolderRenameNode = null;
        OnPropertyChanged(nameof(SelectedHeaderText));
        ScheduleAutosave();
    }

    public void CancelFolderRename(NoteTreeNode folder)
    {
        if (!ReferenceEquals(activeFolderRenameNode, folder))
            return;

        folder.RenameText = folder.Title;
        folder.RenameValidationMessage = string.Empty;
        folder.IsRenameInvalid = false;
        folder.IsRenaming = false;
        activeFolderRenameNode = null;
    }

    /// <summary>
    /// Mobile-only: commits a rename started from the rename popup. The
    /// popup's <c>Text</c> is two-way bound to <see cref="RenamePopupText"/>,
    /// so the new title arrives via that property and the popup does not need
    /// to pass it explicitly.
    /// </summary>
    public void CommitFolderRenameFromPopup(NoteTreeNode folder)
    {
        if (folder is null || !folder.IsFolder)
            return;

        string newTitle = (RenamePopupText ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            RenamePopupValidationMessage = "Folder name cannot be empty.";
            RenamePopupIsInvalid = true;
            return;
        }

        if (!string.Equals(newTitle, folder.Title, StringComparison.Ordinal))
            folder.Title = newTitle;

        RenamePopupValidationMessage = string.Empty;
        RenamePopupIsInvalid = false;
        RenamePopupIsOpen = false;
        RenamePopupText = string.Empty;
        OnPropertyChanged(nameof(SelectedHeaderText));
        ScheduleAutosave();
    }

    /// <summary>
    /// Mobile-only: cancels the rename popup. Discards any pending edit
    /// without touching the underlying folder.
    /// </summary>
    public void CancelFolderRenameFromPopup()
    {
        RenamePopupValidationMessage = string.Empty;
        RenamePopupIsInvalid = false;
        RenamePopupIsOpen = false;
        RenamePopupText = string.Empty;
    }
    private void DeleteSelectedItem(object? target = null)
    {
        target ??= menuTarget;
        if (target is NoteTreeNode { Note: not null } noteTreeNode)
            target = noteTreeNode.Note;

        if (target is NoteModel note)
        {
            NoteTreeNode? folder = FindFolderContainingNote(note, TreeNodes);
            if (folder is not null)
            {
                folder.Notes.Remove(note);
                NoteTreeNode? noteNode = folder.Children.FirstOrDefault(child => ReferenceEquals(child.Note, note));
                if (noteNode is not null)
                    folder.Children.Remove(noteNode);
                if (ReferenceEquals(SelectedNote, note))
                {
                    SelectedNote = folder.Notes.OrderByDescending(item => item.ModifiedDate).FirstOrDefault();
                    SelectedTreeNode = FindNoteNode(SelectedNote, TreeNodes) ?? folder;
                }
            }
        }
        else if (target is NoteTreeNode { IsFolder: true } folder)
        {
            NoteTreeNode? parent = folder.Parent;
            bool selectionWillBeDeleted = SelectedFolderNode is not null && IsWithinFolder(SelectedFolderNode, folder);
            DeleteFolderRecursive(folder);

            if (selectionWillBeDeleted)
            {
                SelectedFolderNode = parent ?? TreeNodes.FirstOrDefault();
            }
        }

        RecalculateCounts();
        OnPropertyChanged(nameof(SelectedCountText));
        IsNodeMenuOpen = false;
        OnPropertyChanged(nameof(SelectedHeaderText));
        ScheduleAutosave();
    }
    private void DeleteFolderRecursive(NoteTreeNode folder)
    {
        foreach (var child in folder.Children.ToList())
            DeleteFolderRecursive(child);

        folder.Notes.Clear();
        if (folder.Parent is null)
            TreeNodes.Remove(folder);
        else
            folder.Parent.Children.Remove(folder);
        folder.Children.Clear();
        folder.Parent = null;
    }
    private static NoteTreeNode? FindFolderContainingNote(NoteModel note, IEnumerable<NoteTreeNode> folders)
    {
        foreach (NoteTreeNode folder in folders)
        {
            if (folder.Notes.Contains(note))
                return folder;

            NoteTreeNode? match = FindFolderContainingNote(note, folder.Children.Where(child => child.IsFolder));
            if (match is not null)
                return match;
        }

        return null;
    }
    private static bool IsWithinFolder(NoteTreeNode candidate, NoteTreeNode folder)
    {
        for (NoteTreeNode? current = candidate; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, folder))
                return true;
        }

        return false;
    }
    private static NoteTreeNode? FindNoteNode(NoteModel? note, IEnumerable<NoteTreeNode> folders)
    {
        if (note is null)
            return null;

        foreach (NoteTreeNode folder in folders)
        {
            NoteTreeNode? node = folder.Children.FirstOrDefault(child => ReferenceEquals(child.Note, note));
            if (node is not null)
                return node;

            node = FindNoteNode(note, folder.Children.Where(child => child.IsFolder));
            if (node is not null)
                return node;
        }

        return null;
    }
    private void RecalculateCounts()
    {
        foreach (NoteTreeNode root in TreeNodes)
            RecalculateCountsRecursive(root);
    }
    private static int RecalculateCountsRecursive(NoteTreeNode folder)
    {
        int own = folder.Notes.Count;
        int children = folder.Children.Where(child => child.IsFolder).Sum(RecalculateCountsRecursive);
        folder.NoteCount = own + children;
        return folder.NoteCount;
    }
    private static string CreateSprintReviewContent() =>
        "<h1>Sprint Review</h1>" +
        "<p>Completed Work</p>" +
        "<p>The team completed the planned Q4 deliverables, including the notes editor improvements and folder navigation updates.</p>" +
        "<p>Release Readiness</p>" +
        "<p>Review outstanding defects, confirm regression testing, and prepare release notes before the next rollout.</p>" +
        "<p>Next Steps</p>" +
        "<p>Prioritize customer feedback, assign owners to follow-up work, and agree on the goals for the next sprint.</p>";

    private static string CreateClientDiscussionContent() =>
        "<h1>Client Discussion</h1>" +
        "<p>Pricing Alignment</p>" +
        "<p>Review the proposed pricing tiers, included features, and expected usage so the model reflects the client's needs.</p>" +
        "<p>Open Questions</p>" +
        "<p>Confirm onboarding support, data migration expectations, and any requirements for future team expansion.</p>" +
        "<p>Next Steps</p>" +
        "<p>Share a revised proposal, collect stakeholder feedback, and schedule a follow-up discussion to finalize the agreement.</p>";

    private static string CreateDatabaseDesignContent() =>
        "<h1>Database Design</h1>" +
        "<p>Postgres Migration</p>" +
        "<p>Plan the migration of note and folder data to PostgreSQL while preserving existing relationships and timestamps.</p>" +
        "<p>Schema Considerations</p>" +
        "<p>Define tables for notes and folders, use stable identifiers, and index parent relationships and modified dates for common queries.</p>" +
        "<p>Migration Checks</p>" +
        "<p>Validate record counts, confirm content integrity, and keep a rollback path available until the migrated data is verified.</p>";

    private static string CreateTravelChecklistContent() =>
        "<h1>Travel Checklist</h1>" +
        "<p>Documents and Bookings</p>" +
        "<p>Check passports and identification, save flight and accommodation confirmations, and note any reservation reference numbers.</p>" +
        "<p>Packing</p>" +
        "<p>Pack weather-appropriate clothing, toiletries, chargers, medication, and a small travel adapter.</p>" +
        "<p>Before Leaving</p>" +
        "<p>Confirm transport to the airport, share the itinerary with family, and secure the home before departure.</p>";

    private static string CreateShoppingListContent() =>
        "<h1>Shopping List</h1>" +
        "<p>Groceries</p>" +
        "<p>Fresh fruit and vegetables, bread, eggs, milk, rice, and ingredients for the meals planned this week.</p>" +
        "<p>Household</p>" +
        "<p>Dish soap, laundry detergent, and reusable storage bags.</p>" +
        "<p>Stationery</p>" +
        "<p>Replace the notebook, sticky notes, and pens used for weekly planning.</p>";

    private static string CreateWorkspaceSetupContent() =>
        "<h1>Workspace Setup</h1>" +
        "<p>Desk Layout</p>" +
        "<p>Keep the primary work area clear, position frequently used items within reach, and leave space for writing and planning.</p>" +
        "<p>Monitor and Ergonomics</p>" +
        "<p>Install the monitor arm, set the screen near eye level, and adjust the chair and keyboard for a comfortable posture.</p>" +
        "<p>Finishing Touches</p>" +
        "<p>Route cables neatly, check task lighting, and keep a small area available for notes and daily priorities.</p>";

    private static string CreateWeeklyReviewContent() =>
        "<h1>Weekly Review</h1>" +
        "<p>Look Back</p>" +
        "<p>Capture the week's accomplishments, decisions, and anything that needs a follow-up.</p>" +
        "<p>Reset Priorities</p>" +
        "<p>Move unfinished work into the right list, remove outdated tasks, and choose the most important outcomes for next week.</p>" +
        "<p>Plan Ahead</p>" +
        "<p>Check upcoming meetings and deadlines, reserve focused work time, and note one thing to improve in the week ahead.</p>";

    private static string CreateProductIdeasContent() =>
        "<h1>Product Ideas</h1>" +
        "<p>Product Ideas</p>" +
        "<p>Overview</p>" +
        "<p>This document captures innovative product concepts that can improve productivity, collaboration, and digital workflows. The goal is to identify solutions that address real user needs while maintaining simplicity, usability, and scalability.</p>" +
        "<p>Featured Idea: Mac-Style Canvas Widget</p>" +
        "<p>A flexible canvas workspace that allows users to freely organize notes, images, tasks, and documents on an infinite board. Users can connect ideas visually, create mind maps, and manage projects through drag-and-drop interactions.</p>" +
        "<p>Key Features</p>" +
        "<p>Infinite zoomable canvas</p>" +
        "<p>Sticky notes and rich text support</p>" +
        "<p>Drag-and-drop organization</p>" +
        "<p>Mind map connections</p>" +
        "<p>Real-time collaboration</p>" +
        "<p>Cross-platform synchronization</p>" +
        "<p>AI-powered content suggestions</p>" +
        "<p>Export to PDF, image, and presentation formats</p>" +
        "<p>Benefits</p>" +
        "<p>A visual canvas reduces context switching, improves personal productivity, and gives collaborators a shared space to understand progress and build on one another's ideas.</p>";

    private static string CreateProjectPlanningHtml() =>
        "<h1>Project Planning</h1>" +
        "<p>Iteration Goals</p>" +
        "<p>Finalize sprint deliverables and align the team on the next macOS Notes web client iteration.</p>" +
        "<p>Planned Work</p>" +
        "<p>Prioritize improvements to note creation, canvas-based organization, and collaboration workflows.</p>" +
        "<p>Coordination</p>" +
        "<p>Confirm owners, identify dependencies, and review progress at the next planning checkpoint.</p>";
}
