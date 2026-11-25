using MauiNotes.Models;
using MauiNotes.ViewModels;
using Syncfusion.Maui.TreeView;
#if WINDOWS
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
#elif ANDROID
using GravityFlags = Android.Views.GravityFlags;
#elif IOS || MACCATALYST
using UIKit;
#endif

namespace MauiNotes.Controls.Notes;

public partial class NotesTreeDesktopUI : ContentView
{
    private readonly Dictionary<NoteTreeNode, Entry> inlineRenameEntries = new();
    private NotesViewModel? subscribedViewModel;
#if WINDOWS
    private readonly Dictionary<Entry, TextBox> inlineRenameTextBoxes = new();
#endif

    // TreeView section - Connect filtering and drag/drop interactions.
    public NotesTreeDesktopUI()
    {
        InitializeComponent();
        FoldersTreeView.Filtered += OnTreeViewFiltered;
        FoldersTreeView.ItemDragging += OnTreeViewItemDragging;
    }

    protected override void OnBindingContextChanged()
    {
        if (subscribedViewModel is not null)
            subscribedViewModel.FolderRenameRequested -= OnFolderRenameRequested;

        base.OnBindingContextChanged();
        subscribedViewModel = BindingContext as NotesViewModel;
        if (subscribedViewModel is not null)
            subscribedViewModel.FolderRenameRequested += OnFolderRenameRequested;
    }

    // TreeView section - Keep row sizing stable and select the first filtered note.
    private void TreeView_QueryNodeSize(object? sender, QueryNodeSizeEventArgs e)
    {
        if (e.Node?.Content is not NoteTreeNode item)
            return;

        e.Height = item.IsFolder ? (item.IsRenameInvalid ? 44 : 32) : 48;
        e.Handled = true;
    }

    private void OnTreeViewFiltered(object? sender, FilteredEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.FilterText) || BindingContext is not NotesViewModel viewModel)
            return;

        NoteTreeNode? firstFilteredNote = FoldersTreeView.FilteredItems?
            .OfType<NoteTreeNode>()
            .FirstOrDefault(node => node.IsNote);

        if (firstFilteredNote is not null)
            viewModel.SelectedTreeNode = firstFilteredNote;
    }

    // TreeView section - Validate and synchronize drag/drop operations.
    private void OnTreeViewItemDragging(object? sender, ItemDraggingEventArgs e)
    {
        if (BindingContext is not NotesViewModel viewModel ||
            e.DraggingNodes is not { Count: 1 } draggingNodes ||
            draggingNodes[0].Content is not NoteTreeNode source)
        {
            if (e.Action == DragAction.Dropping)
                e.Cancel = true;

            return;
        }

        if (e.Action == DragAction.Dropping)
        {
            if (e.TargetNode?.Content is not NoteTreeNode target || e.DropPosition is null or DropPosition.None)
            {
                e.Cancel = true;
                return;
            }

            if (e.DropPosition == DropPosition.DropAsChild)
            {
                if (target.IsFolder)
                    viewModel.MoveTreeNodeToFolder(source, target);

                e.Cancel = true;
                return;
            }

            NoteTreeNode? destination = target.Parent;
            if ((source.IsNote && destination is null) ||
                (source.IsFolder && (ReferenceEquals(source, target) ||
                 (destination is not null && IsWithinFolder(destination, source)))))
            {
                e.Cancel = true;
            }
        }
        else if (e.Action == DragAction.Drop)
        {
            viewModel.SynchronizeAfterTreeDrag(source);
        }
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

    // TreeView section - Track hover state and open the note/folder action menu.
    private void OnTreeItemPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Element { BindingContext: NoteTreeNode node })
            node.IsHovered = true;
    }

    private void OnTreeItemPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Element { BindingContext: NoteTreeNode node })
            node.IsHovered = false;
    }

    private void OnNodeMenuClicked(object? sender, EventArgs e)
    {
        if (sender is View anchor && anchor.BindingContext is NoteTreeNode node)
            NodeActionPopups.ShowFor(anchor, node);
    }

    private void OnFolderRenameRequested(NoteTreeNode node)
    {
        Dispatcher.Dispatch(() =>
        {
            if (!node.IsRenaming || !inlineRenameEntries.TryGetValue(node, out Entry? entry))
                return;

            entry.Focus();
            SelectFolderName(entry);
        });
    }

    private void OnFolderRenameEntryBindingContextChanged(object? sender, EventArgs e)
    {
        if (sender is not Entry entry)
            return;

        foreach (NoteTreeNode node in inlineRenameEntries
            .Where(pair => ReferenceEquals(pair.Value, entry))
            .Select(pair => pair.Key)
            .ToArray())
        {
            inlineRenameEntries.Remove(node);
        }

        if (entry.BindingContext is NoteTreeNode boundNode)
            inlineRenameEntries[boundNode] = entry;
    }

    private void OnFolderRenameEntryLoaded(object? sender, EventArgs e)
    {
        OnFolderRenameEntryBindingContextChanged(sender, EventArgs.Empty);
        UpdateFolderRenameEditorWidth(sender);
    }

    private void OnFolderRenameEntryUnloaded(object? sender, EventArgs e)
    {
        if (sender is not Entry entry)
            return;

        foreach (NoteTreeNode node in inlineRenameEntries
            .Where(pair => ReferenceEquals(pair.Value, entry))
            .Select(pair => pair.Key)
            .ToArray())
        {
            inlineRenameEntries.Remove(node);
        }

#if WINDOWS
        DetachInlineRenameTextBox(entry);
#endif
    }

    private void OnFolderRenameFocused(object? sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
            Dispatcher.Dispatch(() =>
            {
                UpdateFolderRenameEditorWidth(entry);
                SelectFolderName(entry);
            });
    }

    private void OnFolderRenameTextChanged(
        object? sender,
        Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        UpdateFolderRenameEditorWidth(sender);
    }

    private void OnFolderRenameCellSizeChanged(object? sender, EventArgs e)
    {
        if (sender is not Microsoft.Maui.Controls.Grid grid)
            return;

        Entry? entry = grid.Children.OfType<Entry>().FirstOrDefault(candidate => candidate.IsVisible);
        UpdateFolderRenameEditorWidth(entry);
    }

    private static void UpdateFolderRenameEditorWidth(object? sender)
    {
        if (sender is not Entry entry || entry.Parent is not Microsoft.Maui.Controls.Grid grid)
            return;

        Label? measureLabel = grid.Children
            .OfType<Label>()
            .FirstOrDefault(label => label.AutomationId == "FolderRenameTextMeasure");
        Microsoft.Maui.Controls.Border? editBackground = grid.Children
            .OfType<Microsoft.Maui.Controls.Border>()
            .FirstOrDefault(border => border.InputTransparent);
        if (measureLabel is null || editBackground is null)
            return;

        measureLabel.Text = entry.Text ?? string.Empty;
        double measuredWidth = measureLabel.Measure(double.PositiveInfinity, 22).Width;
        double availableWidth = Math.Max(24, grid.Width - 72);
        double editorWidth = Math.Clamp(Math.Ceiling(measuredWidth + 8), 24, availableWidth);
        entry.WidthRequest = editorWidth;
        editBackground.WidthRequest = editorWidth;
    }

    private static void SelectFolderName(Entry entry)
    {
        if (!entry.IsFocused)
            return;

        entry.CursorPosition = 0;
        entry.SelectionLength = entry.Text?.Length ?? 0;
    }

    private void OnFolderRenameCompleted(object? sender, EventArgs e)
    {
        CommitFolderRename(sender);
    }

    private void OnFolderRenameUnfocused(object? sender, FocusEventArgs e)
    {
        CommitFolderRename(sender);
    }

    private void CommitFolderRename(object? sender)
    {
        if (sender is Entry { BindingContext: NoteTreeNode node } &&
            BindingContext is NotesViewModel viewModel)
        {
            viewModel.CommitFolderRename(node);
        }
    }

    private void OnFolderRenameEntryHandlerChanged(object? sender, EventArgs e)
    {
#if WINDOWS
        if (sender is not Entry entry)
            return;

        DetachInlineRenameTextBox(entry);
        if (entry.Handler?.PlatformView is not TextBox textBox)
            return;

        textBox.Padding = new Microsoft.UI.Xaml.Thickness(0);
        textBox.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
        inlineRenameTextBoxes[entry] = textBox;
        textBox.KeyDown += OnInlineRenameTextBoxKeyDown;
#elif ANDROID
        if (sender is Entry { Handler.PlatformView: Android.Widget.EditText editText })
        {
            editText.SetPadding(0, 0, 0, 0);
            editText.Gravity = GravityFlags.CenterVertical;
        }
#elif IOS || MACCATALYST
        if (sender is Entry { Handler.PlatformView: UITextField textField })
        {
            textField.BorderStyle = UITextBorderStyle.None;
            textField.VerticalAlignment = UIControlContentVerticalAlignment.Center;
        }
#endif
    }

#if WINDOWS
    private void DetachInlineRenameTextBox(Entry entry)
    {
        if (!inlineRenameTextBoxes.Remove(entry, out TextBox? textBox))
            return;

        textBox.KeyDown -= OnInlineRenameTextBoxKeyDown;
    }

    private void OnInlineRenameTextBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Escape || sender is not TextBox textBox)
            return;

        Entry? entry = inlineRenameTextBoxes.FirstOrDefault(pair => ReferenceEquals(pair.Value, textBox)).Key;
        if (entry?.BindingContext is not NoteTreeNode node ||
            BindingContext is not NotesViewModel viewModel)
        {
            return;
        }

        e.Handled = true;
        viewModel.CancelFolderRename(node);
    }
#endif
}
