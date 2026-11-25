namespace MauiNotes.Services.Navigation;

/// <summary>
/// Shell navigation used by view models so pages do not subscribe to events
/// just to push a route.
/// </summary>
public interface INavigationService
{
    Task GoToEditorAsync();

    Task GoBackAsync();
}
