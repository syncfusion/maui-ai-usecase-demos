using MauiNotes.Models;
using MauiNotes.ViewModels;
using Syncfusion.Maui.Popup;
using Microsoft.Maui.Controls;

namespace MauiNotes.Controls.Popups;

public partial class NodeActionPopups : ContentView
{
    public static readonly BindableProperty IsMobilePresentationProperty =
        BindableProperty.Create(
            nameof(IsMobilePresentation),
            typeof(bool),
            typeof(NodeActionPopups),
            false,
            propertyChanged: OnIsMobilePresentationChanged);

    private CollectionView? moveDestinationFoldersView;
    private IReadOnlyList<MoveDestinationMenuItem> moveDestinationMenuItems = Array.Empty<MoveDestinationMenuItem>();
    private NoteTreeNode? activeMenuNode;
    private View? moveToMenuAnchor;
    private bool suppressMoveToCloseCleanup;
    private bool preserveNodeMenuAfterClose;
    private bool preserveDeleteStateAfterClose;
    private bool mobileCloseHandlerAttached;
    private bool isMoveToMenuItemHovered;
    private bool isMoveDestinationPopupHovered;
    private bool isNodeActionsPopupHovered;

    public NodeActionPopups()
    {
        InitializeComponent();
    }

    public bool IsMobilePresentation
    {
        get => (bool)GetValue(IsMobilePresentationProperty);
        set => SetValue(IsMobilePresentationProperty, value);
    }

    public void ShowFor(View anchor, NoteTreeNode node)
    {
        if (BindingContext is not NotesViewModel viewModel)
            return;

        activeMenuNode = node;
        Window? window = anchor.Window;
        NodeActionsPopup.WidthRequest = IsMobilePresentation && window is not null
            ? Math.Min(320, Math.Max(1, window.Width - 32))
            : 160;
        if (IsMobilePresentation && window is not null)
            NodeActionsPopup.MaximumHeightRequest = Math.Max(1, window.Height - 48);

        viewModel.ToggleNodeMenuCommand.Execute(node);
        Dispatcher.Dispatch(() =>
        {
            if (window is null || NodeActionsPopup.Handler is null)
                return;

            if (IsMobilePresentation)
                NodeActionsPopup.Show();
            else
                NodeActionsPopup.ShowRelativeToView(anchor, PopupRelativePosition.AlignBottomRight, 4, 4);
        });
    }

    private static void OnIsMobilePresentationChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is not NodeActionPopups popupHost)
            return;

        bool isMobile = (bool)newValue;
        popupHost.NodeActionsPopup.ShowOverlayAlways = isMobile;
        popupHost.MoveDestinationPopup.ShowOverlayAlways = isMobile;
        if (isMobile && !popupHost.mobileCloseHandlerAttached)
        {
            popupHost.NodeActionsPopup.Closed += popupHost.OnNodeActionsPopupClosed;
            popupHost.mobileCloseHandlerAttached = true;
        }
        else if (!isMobile && popupHost.mobileCloseHandlerAttached)
        {
            popupHost.NodeActionsPopup.Closed -= popupHost.OnNodeActionsPopupClosed;
            popupHost.mobileCloseHandlerAttached = false;
        }
    }

    private void OnRenameMenuOptionTapped(object? sender, TappedEventArgs e)
    {
        // On mobile, the inline-rename path that the desktop uses (which
        // toggles NoteTreeNode.IsRenaming and lets the TreeView template
        // swap in an Entry) does not work, so the desktop flow is replaced
        // by a dedicated popup that captures the new title.
        if (IsMobilePresentation)
        {
            if (activeMenuNode is null)
            {
                NodeActionsPopup.Dismiss();
                return;
            }

            preserveNodeMenuAfterClose = true;
            NodeActionsPopup.Dismiss();
            NodeActionsPopup.StaysOpen = false;
            RenameFolderPopupHost.ShowFor(activeMenuNode);
            return;
        }

        if (BindingContext is NotesViewModel viewModel)
            viewModel.BeginRenameCommand.Execute(null);

        NodeActionsPopup.Dismiss();
    }

    private void OnCreateNoteMenuOptionTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.CreateNoteFromMenuCommand.Execute(null);

        NodeActionsPopup.Dismiss();
    }

    private void OnCreateSubfolderMenuOptionTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.CreateSubfolderCommand.Execute(null);

        NodeActionsPopup.Dismiss();
    }

    private void OnMoveToMenuOptionTapped(object? sender, TappedEventArgs e)
    {
        if (sender is View anchor)
            OpenMoveDestinationMenu(anchor);
    }

    private void OnMoveToMenuOptionPointerEntered(object? sender, PointerEventArgs e)
    {
        if (IsMobilePresentation)
            return;

        isMoveToMenuItemHovered = true;
        isNodeActionsPopupHovered = true;
        if (sender is View anchor)
            OpenMoveDestinationMenu(anchor);
    }

    private void OnMoveToMenuOptionPointerExited(object? sender, PointerEventArgs e)
    {
        if (IsMobilePresentation)
            return;

        isMoveToMenuItemHovered = false;
        Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(100),
            CloseMoveDestinationMenuIfPointerLeftCascade);
    }

    private void OnMoveDestinationPointerEntered(object? sender, PointerEventArgs e)
    {
        if (IsMobilePresentation)
            return;

        isMoveDestinationPopupHovered = true;
    }

    private void OnMoveDestinationPointerExited(object? sender, PointerEventArgs e)
    {
        if (IsMobilePresentation)
            return;

        isMoveDestinationPopupHovered = false;
        Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(100),
            CloseMoveDestinationMenuIfPointerLeftCascade);
    }

    private void OnOtherMenuOptionPointerEntered(object? sender, PointerEventArgs e)
    {
        if (IsMobilePresentation)
            return;

        isNodeActionsPopupHovered = true;
        if (moveToMenuAnchor is null)
            return;

        isMoveToMenuItemHovered = false;
        isMoveDestinationPopupHovered = false;
        CloseMoveDestinationMenu();
    }

    private void OnNodeActionsPopupPointerEntered(object? sender, PointerEventArgs e)
    {
        if (IsMobilePresentation)
            return;

        isNodeActionsPopupHovered = true;
        NodeActionsPopup.StaysOpen = true;
    }

    private void OnNodeActionsPopupPointerExited(object? sender, PointerEventArgs e)
    {
        if (IsMobilePresentation)
            return;

        isNodeActionsPopupHovered = false;
        Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(150),
            RestoreNodeActionsPopupDismissal);
    }

    private void RestoreNodeActionsPopupDismissal()
    {
        if (isNodeActionsPopupHovered || isMoveToMenuItemHovered ||
            isMoveDestinationPopupHovered || moveToMenuAnchor is not null)
        {
            return;
        }

        NodeActionsPopup.StaysOpen = false;
        NodeActionsPopup.Dismiss();
    }

    private void CloseMoveDestinationMenuIfPointerLeftCascade()
    {
        if (isMoveToMenuItemHovered || isMoveDestinationPopupHovered ||
            moveToMenuAnchor is null)
        {
            return;
        }

        CloseMoveDestinationMenu();
        if (!isNodeActionsPopupHovered)
        {
            NodeActionsPopup.StaysOpen = false;
            NodeActionsPopup.Dismiss();
        }
    }

    private void CloseMoveDestinationMenu()
    {
        DismissMoveDestinationPopups();
        if (BindingContext is NotesViewModel viewModel)
            viewModel.CancelMoveToCommand.Execute(null);
    }

    private void OpenMoveDestinationMenu(View anchor)
    {
        if (BindingContext is not NotesViewModel viewModel ||
            anchor.Window is null ||
            MoveDestinationPopup.Handler is null)
        {
            return;
        }

        if (ReferenceEquals(moveToMenuAnchor, anchor) && viewModel.IsMoveToPromptOpen)
            return;

        DismissMoveDestinationPopups();
        moveToMenuAnchor = anchor;
        viewModel.BeginMoveToCommand.Execute(null);
        if (!viewModel.IsMoveToPromptOpen)
            return;

        HashSet<NoteTreeNode> destinations = viewModel.MoveDestinationFolders.ToHashSet();
        var menuItems = new List<MoveDestinationMenuItem>(destinations.Count);
        foreach (NoteTreeNode root in viewModel.TreeNodes)
            AddMoveDestinationMenuItems(root, destinations, 0, menuItems, viewModel);

        moveDestinationMenuItems = menuItems;
        if (moveDestinationMenuItems.Count == 0)
            return;

        if (moveDestinationFoldersView is not null)
            moveDestinationFoldersView.ItemsSource = moveDestinationMenuItems;

        UpdateMoveDestinationPopupSize(anchor.Window, moveDestinationMenuItems);
        if (IsMobilePresentation)
        {
            preserveNodeMenuAfterClose = true;
            NodeActionsPopup.Dismiss();
            NodeActionsPopup.StaysOpen = false;
            MoveDestinationPopup.StaysOpen = false;
            MoveDestinationPopup.Show();
        }
        else
        {
            NodeActionsPopup.StaysOpen = true;
            MoveDestinationPopup.StaysOpen = true;
            MoveDestinationPopup.ShowRelativeToView(
                anchor,
                PopupRelativePosition.AlignToRightOf,
                12,
                0);
        }
    }

    private void UpdateMoveDestinationPopupSize(
        Window window,
        IReadOnlyList<MoveDestinationMenuItem> menuItems)
    {
        const double rowHeight = 32;
        double verticalChrome = IsMobilePresentation ? 54 : 12;
        const double horizontalChrome = 28;
        const double maximumWidth = 360;

        double desiredWidth = menuItems
            .Select(item =>
            {
                var label = new Label { Text = item.Title, FontSize = 13 };
                return label.Measure(maximumWidth, rowHeight).Width +
                    item.Depth * 14 +
                    horizontalChrome;
            })
            .DefaultIfEmpty(180)
            .Max();

        double widthLimit = IsMobilePresentation
            ? Math.Max(1, Math.Min(maximumWidth, window.Width - 32))
            : Math.Max(120, Math.Min(maximumWidth, window.Width - NodeActionsPopup.Width - 24));
        MoveDestinationPopup.WidthRequest = Math.Min(widthLimit, Math.Max(180, desiredWidth));

        double availableHeight = IsMobilePresentation
            ? Math.Max(1, window.Height - 48)
            : Math.Max(120, window.Height - 24);
        double mainMenuHeight = NodeActionsPopup.Height;
        if (!double.IsFinite(mainMenuHeight) || mainMenuHeight <= 0)
            mainMenuHeight = availableHeight;

        double heightLimit = IsMobilePresentation
            ? availableHeight
            : Math.Min(mainMenuHeight, availableHeight);
        double contentHeight = menuItems.Count * rowHeight + verticalChrome;
        MoveDestinationPopup.HeightRequest = Math.Min(contentHeight, heightLimit);
    }

    private static void AddMoveDestinationMenuItems(
        NoteTreeNode folder,
        HashSet<NoteTreeNode> destinations,
        int depth,
        List<MoveDestinationMenuItem> items,
        NotesViewModel viewModel)
    {
        if (destinations.Contains(folder))
        {
            bool isEnabled = viewModel.SelectMoveDestinationCommand.CanExecute(folder);
            items.Add(new MoveDestinationMenuItem(folder, depth, isEnabled));
        }

        foreach (NoteTreeNode child in folder.Children.Where(child => child.IsFolder))
            AddMoveDestinationMenuItems(child, destinations, depth + 1, items, viewModel);
    }

    private void OnMoveDestinationFoldersViewLoaded(object? sender, EventArgs e)
    {
        if (sender is not CollectionView collectionView)
            return;

        moveDestinationFoldersView = collectionView;
        collectionView.ItemsSource = moveDestinationMenuItems;
    }

    private void DismissMoveDestinationPopups()
    {
        bool wasSuppressingCleanup = suppressMoveToCloseCleanup;
        suppressMoveToCloseCleanup = true;
        try
        {
            MoveDestinationPopup.Dismiss();
            moveToMenuAnchor = null;
        }
        finally
        {
            suppressMoveToCloseCleanup = wasSuppressingCleanup;
        }
    }

    private void SelectMoveDestination(NoteTreeNode destination)
    {
        if (BindingContext is not NotesViewModel viewModel)
            return;

        bool wasSuppressingCleanup = suppressMoveToCloseCleanup;
        suppressMoveToCloseCleanup = true;
        try
        {
            viewModel.SelectMoveDestinationCommand.Execute(destination);
            DismissMoveDestinationPopups();
            NodeActionsPopup.StaysOpen = false;
            NodeActionsPopup.Dismiss();
        }
        finally
        {
            suppressMoveToCloseCleanup = wasSuppressingCleanup;
        }
    }

    private void OnMoveDestinationPopupClosed(object? sender, EventArgs e)
    {
        if (suppressMoveToCloseCleanup)
            return;

        isMoveToMenuItemHovered = false;
        isMoveDestinationPopupHovered = false;
        moveToMenuAnchor = null;
        if (BindingContext is NotesViewModel viewModel)
            viewModel.CancelMoveToCommand.Execute(null);
    }

    private void OnMoveDestinationSelected(object? sender, EventArgs e)
    {
        if (sender is Element { BindingContext: MoveDestinationMenuItem { IsEnabled: true } menuItem })
            SelectMoveDestination(menuItem.Folder);
    }

    private void OnNodeActionsPopupClosed(object? sender, EventArgs e)
    {
        if (preserveNodeMenuAfterClose)
        {
            preserveNodeMenuAfterClose = false;
            return;
        }

        isMoveToMenuItemHovered = false;
        isMoveDestinationPopupHovered = false;
        isNodeActionsPopupHovered = false;
        NodeActionsPopup.StaysOpen = false;
        DismissMoveDestinationPopups();

        if (BindingContext is NotesViewModel viewModel)
        {
            viewModel.CancelMoveToCommand.Execute(null);
            viewModel.IsNodeMenuOpen = false;
        }
    }

    private void OnToggleNoteLockMenuOptionTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.ToggleNoteLockCommand.Execute(null);

        NodeActionsPopup.Dismiss();
    }

    private void OnDuplicateNoteMenuOptionTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.DuplicateNoteCommand.Execute(null);

        NodeActionsPopup.Dismiss();
    }

    private void OnDeleteMenuOptionTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not NotesViewModel viewModel)
        {
            NodeActionsPopup.Dismiss();
            return;
        }

        bool deletingNote = viewModel.IsNoteMenuTarget;
        bool deletingFolder = viewModel.IsFolderMenuTarget;
        if (!deletingNote && !deletingFolder)
        {
            NodeActionsPopup.Dismiss();
            viewModel.IsNodeMenuOpen = false;
            return;
        }

        NodeActionsPopup.Dismiss();
        viewModel.IsNodeMenuOpen = false;
        Dispatcher.Dispatch(() =>
        {
            if (DeleteConfirmationPopup.Handler is not null)
            {
                if (IsMobilePresentation && Window is not null)
                {
                    DeleteConfirmationPopup.WidthRequest = Math.Min(420, Math.Max(1, Window.Width - 32));
                    DeleteConfirmationPopup.MaximumHeightRequest = Math.Max(1, Window.Height - 48);
                    DeleteConfirmationPopup.StaysOpen = false;
                }

                DeleteConfirmationPopup.Show();
            }
        });
    }

    private void OnConfirmDeleteTapped(object? sender, EventArgs e)
    {
        if (BindingContext is not NotesViewModel viewModel ||
            !viewModel.DeleteNodeCommand.CanExecute(null))
        {
            return;
        }

        viewModel.DeleteNodeCommand.Execute(null);
        DeleteConfirmationPopup.Dismiss();
    }

    private void OnCancelDeleteTapped(object? sender, EventArgs e)
    {
        if (IsMobilePresentation)
            ReturnToMainMenuFromDelete();
        else
            DeleteConfirmationPopup.Dismiss();
    }

    private void OnDeleteConfirmationBackTapped(object? sender, EventArgs e)
    {
        ReturnToMainMenuFromDelete();
    }

    private void ReturnToMainMenuFromDelete()
    {
        preserveDeleteStateAfterClose = true;
        DeleteConfirmationPopup.Dismiss();
        ShowMainMenu();
    }

    private void OnDeleteConfirmationPopupClosed(object? sender, EventArgs e)
    {
        if (preserveDeleteStateAfterClose)
        {
            preserveDeleteStateAfterClose = false;
            return;
        }

        if (IsMobilePresentation && BindingContext is NotesViewModel viewModel)
            viewModel.IsNodeMenuOpen = false;
    }

    private void OnMoveDestinationBackTapped(object? sender, EventArgs e)
    {
        suppressMoveToCloseCleanup = true;
        MoveDestinationPopup.Dismiss();
        suppressMoveToCloseCleanup = false;

        if (BindingContext is NotesViewModel viewModel)
            viewModel.CancelMoveToCommand.Execute(null);

        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        if (!IsMobilePresentation || BindingContext is not NotesViewModel viewModel ||
            activeMenuNode is null)
        {
            return;
        }

        viewModel.ToggleNodeMenuCommand.Execute(activeMenuNode);
        if (Window is null || NodeActionsPopup.Handler is null)
            return;

        NodeActionsPopup.WidthRequest = Math.Min(320, Math.Max(1, Window.Width - 32));
        NodeActionsPopup.MaximumHeightRequest = Math.Max(1, Window.Height - 48);
        NodeActionsPopup.ShowOverlayAlways = true;
        NodeActionsPopup.Show();
    }
}

public sealed class MoveDestinationMenuItem
{
    public MoveDestinationMenuItem(NoteTreeNode folder, int depth, bool isEnabled)
    {
        Folder = folder;
        Title = folder.Title;
        Depth = depth;
        IsEnabled = isEnabled;
        Indent = new Thickness(8 + depth * 14, 0, 6, 0);
    }

    public NoteTreeNode Folder { get; }
    public string Title { get; }
    public int Depth { get; }
    public bool IsEnabled { get; }
    public Thickness Indent { get; }
}
