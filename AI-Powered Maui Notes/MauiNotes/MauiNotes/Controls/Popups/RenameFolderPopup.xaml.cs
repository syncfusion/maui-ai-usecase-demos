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
    /// folder's current title. The popup is modal on mobile; pressing Save
    /// commits the rename through the ViewModel, Cancel just dismisses.
    /// </summary>
    public void ShowFor(NoteTreeNode folder)
    {
        if (BindingContext is not NotesViewModel viewModel || folder is null)
            return;

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
        RenamePopup.StaysOpen = true;
        RenamePopup.Show();

        Dispatcher.Dispatch(() => RenameInput.Focus());
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

    private void OnRenameBackTapped(object? sender, EventArgs e)
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
        RenamePopup.Dismiss();
    }
}
