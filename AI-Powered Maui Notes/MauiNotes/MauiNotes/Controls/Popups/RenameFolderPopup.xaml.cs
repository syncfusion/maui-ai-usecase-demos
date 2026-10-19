using MauiNotes.Controls;
using MauiNotes.Models;
using MauiNotes.ViewModels;
using Syncfusion.Maui.Popup;

namespace MauiNotes.Controls.Popups;

public partial class RenameFolderPopup : ContentView
{
    private NoteTreeNode? activeRenameFolder;

    public RenameFolderPopup()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Mobile-only entry point: shows the rename popup prefilled with the
    /// folder's current title. Pressing Save commits the rename through the
    /// ViewModel; Cancel or tapping outside dismisses without renaming.
    /// </summary>
    public void ShowFor(NoteTreeNode folder, NotesViewModel viewModel)
    {
        if (folder is null || !folder.IsFolder)
            return;

        BindingContext = viewModel;
        RenamePopup.BindingContext = viewModel;
        activeRenameFolder = folder;
        viewModel.RenamePopupText = folder.Title;
        viewModel.RenamePopupValidationMessage = string.Empty;
        viewModel.RenamePopupIsInvalid = false;
        viewModel.RenamePopupIsOpen = true;

        if (Window is not null)
        {
            RenamePopup.WidthRequest = Math.Min(420, Math.Max(1, Window.Width - 32));
            RenamePopup.MaximumHeightRequest = Math.Max(1, Window.Height - 48);
        }
        RenamePopup.StaysOpen = false;
        RenamePopup.Show();

        // The rename entry lives inside SfPopup.ContentTemplate (a DataTemplate),
        // so its x:Name is not promoted to a partial-class field. Resolve it via
        // the popup's inflated visual tree after Show() has materialized it.
        Dispatcher.Dispatch(() => FindRenameInput()?.Focus());
    }

    private BorderlessEntry? FindRenameInput() =>
        RenamePopup.GetVisualTreeDescendants()
            .OfType<BorderlessEntry>()
            .FirstOrDefault();

    private void OnRenameTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (RenamePopup.BindingContext is NotesViewModel viewModel)
            viewModel.RenamePopupText = e.NewTextValue ?? string.Empty;
    }

    private void OnRenameSaveTapped(object? sender, EventArgs e)
    {
        if (BindingContext is not NotesViewModel viewModel || activeRenameFolder is null)
        {
            Dismiss();
            return;
        }

        viewModel.CommitFolderRenameFromPopup(activeRenameFolder);
        if (viewModel.RenamePopupIsInvalid)
            return;

        activeRenameFolder = null;
        Dismiss();
    }

    private void OnRenameCancelTapped(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.CancelFolderRenameFromPopup();

        activeRenameFolder = null;
        Dismiss();
    }

    private void OnRenameCompleted(object? sender, EventArgs e)
    {
        if (BindingContext is not NotesViewModel viewModel || activeRenameFolder is null)
            return;

        if (sender is BorderlessEntry renameInput)
            viewModel.RenamePopupText = renameInput.Text ?? string.Empty;

        viewModel.CommitFolderRenameFromPopup(activeRenameFolder);
        if (viewModel.RenamePopupIsInvalid)
            return;

        activeRenameFolder = null;
        Dismiss();
    }

    private void OnRenamePopupClosed(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel && viewModel.RenamePopupIsOpen)
        {
            viewModel.CancelFolderRenameFromPopup();
        }

        activeRenameFolder = null;
    }

    private void Dismiss()
    {
        RenamePopup.StaysOpen = false;
        RenamePopup.IsOpen = false;
    }
}
