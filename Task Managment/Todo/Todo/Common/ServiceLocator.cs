using Todo.Features.Notes.Services;

namespace Todo.Common;

/// <summary>
/// Tiny service-locator used only by parameter-less constructors (e.g.
/// design-time tooling and XAML previews). The runtime path resolves
/// services through the DI container registered in <c>MauiProgram</c>.
/// </summary>
public static class ServiceLocator
{
    private static INoteService? _noteService;
    private static INavigationService? _navigation;

    public static INoteService Current => _noteService ??= new InMemoryNoteService();
    public static INavigationService CurrentNavigation => _navigation ??= new MauiNavigationService();

    public static void Register(INoteService noteService) => _noteService = noteService;
    public static void RegisterNavigation(INavigationService navigation) => _navigation = navigation;
}
