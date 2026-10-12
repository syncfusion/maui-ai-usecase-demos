using Todo.ViewModels;

namespace Todo.Views.Authentication;

/// <summary>
/// Placeholder sign-up page.  The full XAML layout is added in Module 2
/// (Task 2-01).
/// </summary>
public sealed class SignUpPage : ContentPage
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SignUpPage"/> class.
    /// </summary>
    /// <param name="viewModel">ViewModel injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> is <see langword="null"/>.</exception>
    public SignUpPage(SignUpPageViewModel viewModel)
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
                    Text = "Create account",
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center,
                },
            },
        };
    }
}
