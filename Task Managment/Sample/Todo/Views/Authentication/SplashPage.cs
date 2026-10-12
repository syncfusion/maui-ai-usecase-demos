using Todo.ViewModels;

namespace Todo.Views.Authentication;

/// <summary>
/// Placeholder splash page.  Shown at app startup while the
/// <see cref="SplashPageViewModel"/> decides where to navigate (sign-in or
/// main).  The full XAML layout is added in Task 1-06 (Navigation Setup).
/// </summary>
public sealed partial class SplashPage : ContentPage
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SplashPage"/> class.
    /// </summary>
    /// <param name="viewModel">ViewModel injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> is <see langword="null"/>.</exception>
    public SplashPage(SplashPageViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        Title = viewModel.Title;
        Content = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "Todo",
                    FontSize = 32,
                    HorizontalOptions = LayoutOptions.Center,
                },
            },
        };
    }
}
