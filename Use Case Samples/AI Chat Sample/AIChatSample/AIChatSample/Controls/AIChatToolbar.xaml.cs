using System.Windows.Input;

namespace AIChatSample.Controls;

public partial class AIChatToolbar : ContentView
{

    public static readonly BindableProperty MessageCommandProperty =
        BindableProperty.Create(nameof(MessageCommand), typeof(ICommand), typeof(AIChatToolbar));

    public static readonly BindableProperty IsModelPopupVisibleProperty =
        BindableProperty.Create(nameof(IsModelPopupVisible), typeof(bool), typeof(AIChatToolbar), false);

    public static readonly BindableProperty IsNavigationMenuVisibleProperty =
        BindableProperty.Create(nameof(IsNavigationMenuVisible), typeof(bool), typeof(AIChatToolbar), false);

    public AIChatToolbar()
    {
        InitializeComponent();
    }

    public ICommand? MessageCommand
    {
        get => (ICommand?)GetValue(MessageCommandProperty);
        set => SetValue(MessageCommandProperty, value);
    }

    public bool IsModelPopupVisible
    {
        get => (bool)GetValue(IsModelPopupVisibleProperty);
        set => SetValue(IsModelPopupVisibleProperty, value);
    }

    public bool IsNavigationMenuVisible
    {
        get => (bool)GetValue(IsNavigationMenuVisibleProperty);
        set => SetValue(IsNavigationMenuVisibleProperty, value);
    }

    public event EventHandler? NavigationMenuTapped;
    public event EventHandler? TemporaryChatRequested;

    private void OnNavigationMenuTapped(object sender, TappedEventArgs e)
    {
        NavigationMenuTapped?.Invoke(this, EventArgs.Empty);
    }

    private void TempChatTapped(object sender, TappedEventArgs e)
    {
        TemporaryChatRequested?.Invoke(this, EventArgs.Empty);
    }
}