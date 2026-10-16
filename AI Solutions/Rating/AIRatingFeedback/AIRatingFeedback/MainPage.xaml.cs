using AIRatingFeedback.ViewModels;

namespace AIRatingFeedback;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnFeedbackTextChanged(object? sender, TextChangedEventArgs e)
        => _viewModel.OnFeedbackChanged(e.NewTextValue);
}