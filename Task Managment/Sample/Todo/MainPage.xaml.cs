namespace Todo;

/// <summary>
/// Default landing page used while the full module-based screen flow
/// (Authentication → Main → TaskList) is being built across later tasks.
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>
    /// Number of times the "Click me" button has been tapped.
    /// </summary>
    private int _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Increments <see cref="_count"/> and renders the new value on the
    /// <see cref="Button"/> so that the XAML <c>Clicked</c> event handler
    /// can be verified manually.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnCounterClicked(object? sender, EventArgs e)
    {
        _count++;
        if (sender is Button button)
        {
            button.Text = _count == 1 ? $"Clicked {_count} time" : $"Clicked {_count} times";
        }

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
