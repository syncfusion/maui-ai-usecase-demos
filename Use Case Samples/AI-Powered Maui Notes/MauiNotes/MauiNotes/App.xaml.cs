using Syncfusion.Maui.Themes;

namespace MauiNotes
{
    public partial class App : Application
    {
        private readonly AppShell appShell;
        private readonly SyncfusionThemeResourceDictionary syncfusionTheme;

        public App(AppShell appShell)
        {
            this.appShell = appShell;
            InitializeComponent();
            syncfusionTheme = Resources.MergedDictionaries
                .OfType<SyncfusionThemeResourceDictionary>()
                .Single();
            ApplySyncfusionTheme(RequestedTheme);
            RequestedThemeChanged += OnRequestedThemeChanged;
        }

        private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e) =>
            ApplySyncfusionTheme(e.RequestedTheme);

        private void ApplySyncfusionTheme(AppTheme theme) =>
            syncfusionTheme.VisualTheme = theme == AppTheme.Dark
                ? SfVisuals.CupertinoDark
                : SfVisuals.CupertinoLight;

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(appShell);
        }
    }
}