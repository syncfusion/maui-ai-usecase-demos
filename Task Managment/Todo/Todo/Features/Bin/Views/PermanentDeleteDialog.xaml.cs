namespace Todo.Features.Bin.Views;

/// <summary>
/// Modal confirmation page that matches <c>deletepermently.png</c>.
/// Hosts the dialog card over a scrim and reports the user's choice
/// through the standard <see cref="TaskCompletionSource{TResult}"/>
/// pattern. The page has no ViewModel: the result is a transient
/// user input, not a piece of UI state.
/// </summary>
public partial class PermanentDeleteDialog : ContentPage
{
    private readonly TaskCompletionSource<bool> _completion = new();

    public PermanentDeleteDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Shows the dialog and waits for the user to tap either
    /// <b>Cancel</b> (returns <c>false</c>) or <b>Delete</b> (returns
    /// <c>true</c>). Tapping outside the dialog dismisses it as a
    /// cancel, matching the spec note in deletepermently.png.
    /// </summary>
    public static Task<bool> ShowAsync(INavigation navigation)
    {
        var dialog = new PermanentDeleteDialog();
        var task = dialog._completion.Task;
        navigation.PushModalAsync(dialog);
        return task;
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        _completion.TrySetResult(false);
        await Navigation.PopModalAsync();
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        _completion.TrySetResult(true);
        await Navigation.PopModalAsync();
    }

    /// <summary>
    /// Tap-outside dismisses the dialog and counts as a cancel.
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        _completion.TrySetResult(false);
        return base.OnBackButtonPressed();
    }
}
