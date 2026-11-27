namespace SupportReplyAIDraft.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => global::SupportReplyAIDraft.MauiProgram.CreateMauiApp();
}
