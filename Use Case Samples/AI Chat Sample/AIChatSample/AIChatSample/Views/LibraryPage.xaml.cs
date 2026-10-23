using AIChatSample.Controls;

namespace AIChatSample;

public partial class LibraryPage : ContentPage
{
	public LibraryPage()
	{
		InitializeComponent();
        this.BindingContext = ServiceHelper.GetService<AIMainLayoutViewModel>() ?? new AIMainLayoutViewModel();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is null)
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
        }
        else
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
            AIChatNavigationPanel.NavigationRequested += OnNavigationRequested;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private async void OnBackRequested(object? sender, EventArgs e)
    {
        if (Navigation?.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync(animated: true);
        }
    }

    private void OnNavigationRequested(object? sender, NavigationRequestEventArgs e)
    {
        ChatNavigationService.Handle(this, e.Key, e.Parameter);
    }

    private void OnNavigationMenuTapped(object sender, EventArgs e)
    {
#if ANDROID
        AIChatNavigationPanel.ToggleCompact();
#endif
    }
}