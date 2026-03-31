namespace RichTextEditorAssistViewSample
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AssistViewPage), typeof(AssistViewPage));
        }
    }
}
