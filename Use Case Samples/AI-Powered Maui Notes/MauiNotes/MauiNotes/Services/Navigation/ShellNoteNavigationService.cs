using MauiNotes.Views.Editor;

namespace MauiNotes.Services.Navigation;

public sealed class ShellNoteNavigationService : INoteNavigationService
{
    public Task NavigateToEditorAsync()
    {
        Shell shell = Shell.Current
            ?? throw new InvalidOperationException("The app shell is not available.");
        return shell.GoToAsync(nameof(EditorPageMobileUI));
    }
}
