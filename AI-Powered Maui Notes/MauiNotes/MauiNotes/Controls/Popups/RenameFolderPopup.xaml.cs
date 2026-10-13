using MauiNotes.Models;
using MauiNotes.ViewModels;

namespace MauiNotes.Controls.Popups;

public partial class RenameFolderPopup : ContentView
{
    private NoteTreeNode? activeRenameFolder;

    public RenameFolderPopup()
    {
        InitializeComponent();
    }

    public void ShowFor(NoteTreeNode folder)
    {
        if (BindingContext is not NotesViewModel viewModel || folder is null)
            return;

        activeRenameFolder = folder;
        viewModel.PrepareFolderRenameFromPopup(folder);

        if (RenameFolderDialog.Handler is null)
            return;

        if (Window is not null)
        {
            RenameFolderDialog.WidthRequest = Math.Min(360, Math.Max(1, Window.Width - 32));
            RenameFolderDialog.MaximumHeightRequest = Math.Max(1, Window.Height - 48);
        }

        Dispatcher.Dispatch(() =>
        {
            if (RenameFolderDialog.Handler is null)
                return;

            RenameFolderDialog.Show();
            Dispatcher.Dispatch(FocusRenameEntry);
        });
    }

    private void FocusRenameEntry()
    {
        if (RenameEntry is null)
            return;

        RenameEntry.Focus();
        RenameEntry.CursorPosition = 0;
        RenameEntry.SelectionLength = RenameEntry.Text?.Length ?? 0;
    }

    private void OnConfirmRenameTapped(object? sender, EventArgs e)
    {
        if (BindingContext is not NotesViewModel viewModel || activeRenameFolder is null)
        {
            ClosePopup();
            return;
        }

        if (!viewModel.CommitFolderRenameFromPopup(activeRenameFolder))
            return;

        activeRenameFolder = null;
        ClosePopup();
    }

    private void OnCancelRenameTapped(object? sender, EventArgs e)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.CancelFolderRenameFromPopup();

        ClosePopup();
    }

    private void OnRenameEntryCompleted(object? sender, EventArgs e)
    {
        // Treat the keyboard "Done" key as Save.
        OnConfirmRenameTapped(sender, e);
    }

    private void OnRenameFolderPopupClosed(object? sender, EventArgs e)
    {
        // Guard: Closing the popup by tapping the overlay counts as Cancel.
        if (BindingContext is NotesViewModel viewModel)
            viewModel.CancelFolderRenameFromPopup();

        activeRenameFolder = null;
    }

    private void ClosePopup()
    {
        RenameFolderDialog.Dismiss();
    }
}
