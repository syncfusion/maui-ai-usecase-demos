using MauiNotes.Services.Theming;

namespace MauiNotes
{
    public partial class App : Application
    {
        private readonly IThemeService themeService;

        public App(IThemeService themeService)
        {
            this.themeService = themeService;
            InitializeComponent();
            themeService.Apply(RequestedTheme);
            RequestedThemeChanged += OnRequestedThemeChanged;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(Handler!.MauiContext!.Services.GetRequiredService<AppShell>());
        }

        private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e) =>
            themeService.Apply(e.RequestedTheme);
    }
}
