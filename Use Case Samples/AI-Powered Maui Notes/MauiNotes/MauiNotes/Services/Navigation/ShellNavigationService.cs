using MauiNotes.Views.Editor;

namespace MauiNotes.Services.Navigation;

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToEditorAsync()
    {
        if (DeviceInfo.Idiom != DeviceIdiom.Phone || Shell.Current is null)
            return Task.CompletedTask;

        return Shell.Current.GoToAsync(nameof(EditorPageMobileUI));
    }
}
