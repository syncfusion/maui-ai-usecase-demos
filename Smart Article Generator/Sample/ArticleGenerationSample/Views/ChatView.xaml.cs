using Microsoft.Maui.Controls;

namespace ArticleGenerationSample;

/// <summary>
/// Displays the AI-generated HTML content, including a shimmer placeholder while loading
/// and actions like copy and retry.
/// </summary>
public partial class ChatView : ContentView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatView"/> class.
    /// </summary>
    public ChatView()
    {
        InitializeComponent();
    }
}
