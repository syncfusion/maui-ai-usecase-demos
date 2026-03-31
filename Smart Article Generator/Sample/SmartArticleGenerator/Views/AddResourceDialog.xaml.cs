namespace SmartArticleGenerator;

/// <summary>
/// Modal dialog to collect a resource URL, title, and description.
/// Returns a tuple when closed via <see cref="ShowAsync"/>.
/// </summary>
public partial class AddResourceDialog : ContentPage
{
    #region Fields

    /// <summary>
    /// Task completion source used to return dialog results.
    /// </summary>
    private TaskCompletionSource<(string Url, string Title, string Description)>? _tcs;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="AddResourceDialog"/> class.
    /// </summary>
    public AddResourceDialog()
    {
        InitializeComponent();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handles the Cancel button click, closing the dialog and returning empty values.
    /// </summary>
    private void OnCancelClicked(object sender, EventArgs e)
    {
        _tcs?.TrySetResult((string.Empty, string.Empty, string.Empty));
        _ = Navigation.PopModalAsync();
    }

    /// <summary>
    /// Handles the Add button click, validating and returning the resource details.
    /// </summary>
    private void OnAddClicked(object sender, EventArgs e)
    {
        var url = UrlEntry.Text?.Trim() ?? string.Empty;
        var title = TitleEntry.Text?.Trim() ?? string.Empty;
        var description = DescriptionEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(title))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlertAsync("Validation", "Please enter a resource title", "OK");
            });
            return;
        }

        _tcs?.TrySetResult((url, title, description));
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Navigation.PopModalAsync();
        });
    }

    /// <summary>
    /// Shows the dialog modally and returns the entered values when closed.
    /// </summary>
    /// <returns>A tuple containing Url, Title, and Description.</returns>
    public static async Task<(string Url, string Title, string Description)> ShowAsync()
    {
        var dialog = new AddResourceDialog();
        dialog._tcs = new TaskCompletionSource<(string, string, string)>();
        var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (mainPage == null)
        {
            return (string.Empty, string.Empty, string.Empty);
        }

        var navPage = new NavigationPage(dialog);
        await mainPage.Navigation.PushModalAsync(navPage);
        return await dialog._tcs.Task;
    }

    #endregion
}
