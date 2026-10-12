using Todo.ViewModels;

namespace Todo.Views.Authentication;

/// <summary>
/// Placeholder sign-in page.  The full XAML layout (e-mail / password
/// fields, sign-in command binding to
/// <see cref="Todo.Services.Interfaces.IAuthenticationService"/>) is added
/// in Module 2 (Task 2-01).
/// </summary>
public sealed class SignInPage : ContentPage
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SignInPage"/> class.
    /// </summary>
    /// <param name="viewModel">ViewModel injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> is <see langword="null"/>.</exception>
    public SignInPage(SignInPageViewModel viewModel)
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
                    Text = "Sign in",
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center,
                },
            },
        };
    }
}
