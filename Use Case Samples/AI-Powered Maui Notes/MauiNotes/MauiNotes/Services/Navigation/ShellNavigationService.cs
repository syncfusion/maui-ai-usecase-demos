using MauiNotes.Views.Editor;

namespace MauiNotes.Services.Navigation;

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToEditorAsync() =>
        Shell.Current.GoToAsync(nameof(EditorPageMobileUI));

    public Task GoBackAsync() =>
        Shell.Current.GoToAsync("..");
}
