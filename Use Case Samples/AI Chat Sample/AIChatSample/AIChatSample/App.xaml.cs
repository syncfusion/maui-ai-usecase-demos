namespace AIChatSample;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Register the Syncfusion license before any Syncfusion control is
        // inflated. Replace the placeholder with a real key (or pull it
        // from a secure store) to silence the runtime license banner.
        // TODO: Replace with your Syncfusion license key, or load it from
        //       a secure store (e.g. environment variable, user-secrets).
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");

        // SfAIAssistView resolves its text-color and background keys from
        // Application.Current.Resources (not from page-level Resources).
        // Add theme-aware defaults here so the AI response and the user
        // prompt are legible in both light and dark mode. Pages can still
        // override the per-page background via SfAIAssistViewBackground.
        InitializeThemeDefaults();
    }

    private void InitializeThemeDefaults()
    {
        ApplyThemeDefaults(Current?.UserAppTheme == AppTheme.Dark);
        Current!.RequestedThemeChanged += (_, _) =>
            ApplyThemeDefaults(Current?.UserAppTheme == AppTheme.Dark);
    }

    private void ApplyThemeDefaults(bool isDark)
    {
        // White in dark mode, near-black in light — contrasts with whatever
        // background the page sets via SfAIAssistViewBackground.
        var textColor = isDark ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#1C1B1D");
        var hintColor = isDark ? Color.FromArgb("#9BA1A6") : Color.FromArgb("#5F5E60");
        var editorBg = isDark ? Color.FromArgb("#2A2A2C") : Color.FromArgb("#FFFFFF");
        var responseBg = isDark ? Color.FromArgb("#1C1B1D") : Color.FromArgb("#FCF8FA");
        var separator = isDark ? Color.FromArgb("#3A3A3C") : Color.FromArgb("#E5E1E3");

        if (Resources is not ResourceDictionary dict) return;

        // Editor (prompt) area
        dict["SfAIAssistViewEditorBackground"] = editorBg;
        dict["SfAIAssistViewEditorTextColor"] = textColor;
        dict["SfAIAssistViewEditorPlaceholderColor"] = hintColor;
        dict["SfAIAssistViewEditorBorderColor"] = separator;
        dict["SfAIAssistViewEditorFocusedBorderColor"] = textColor;

        // Response (AI answer) area
        dict["SfAIAssistViewResponseBackground"] = responseBg;
        dict["SfAIAssistViewResponseTextColor"] = textColor;
        dict["SfAIAssistViewResponseTimeStampColor"] = hintColor;
        dict["SfAIAssistViewResponseSeparatorColor"] = separator;

        // Suggestion cards on the welcome screen
        dict["SfAIAssistViewSuggestionItemBackground"] = editorBg;
        dict["SfAIAssistViewSuggestionItemTextColor"] = textColor;

        // Header suggestion background (welcome pill)
        dict["SfAIAssistViewHeaderSuggestionBackground"] = responseBg;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Initial route: NewChat wrapped in a NavigationPage so Profile,
        // Library, CreateAgent, etc. can be pushed onto the stack.
        return new Window(new AppShell());
    }
}
