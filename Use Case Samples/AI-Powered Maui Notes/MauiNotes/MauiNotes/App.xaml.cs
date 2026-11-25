using MauiNotes.Services.Theming;
using MauiNotes.ViewModels;

namespace MauiNotes;

public partial class App : Application
{
    private readonly NotesViewModel notesViewModel;
    private readonly IThemeService themeService;

    public App(NotesViewModel notesViewModel, AppShell appShell, IThemeService themeService)
    {
        this.notesViewModel = notesViewModel;
        this.themeService = themeService;
        InitializeComponent();
        themeService.Apply(RequestedTheme);
        RequestedThemeChanged += (_, args) => themeService.Apply(args.RequestedTheme);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(Handler?.MauiContext?.Services.GetService<AppShell>()
            ?? new AppShell(notesViewModel));
    }
}
