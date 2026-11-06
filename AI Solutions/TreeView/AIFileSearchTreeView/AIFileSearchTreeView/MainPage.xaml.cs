using AIFileSearchTreeView.ViewModel;

namespace AIFileSearchTreeView;

public partial class MainPage : ContentPage
{
    private readonly SalesFileViewModel _viewModel;
    private CancellationTokenSource? _searchCancellationTokenSource;
    private int _searchRequestVersion;

    public MainPage(SalesFileViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        searchEntry.TextChanged += OnSearchEntryTextChanged;
    }

    /// <summary>
    /// Handle search entry text changes with AI semantic search.
    /// </summary>
    private async void OnSearchEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.Trim() ?? string.Empty;

        int requestVersion = Interlocked.Increment(ref _searchRequestVersion);

        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource?.Dispose();
        _searchCancellationTokenSource = new CancellationTokenSource();
        var token = _searchCancellationTokenSource.Token;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            SetSearchBusy(false);
            try
            {
                await _viewModel.PerformSemanticSearchAsync(searchText, token);
            }
            catch (OperationCanceledException)
            {
            }

            if (requestVersion == _searchRequestVersion)
            {
                _viewModel.ClearSearch();
                SwapToFullTree();
            }

            return;
        }

        try
        {
            // Show the loading indicator immediately and only allow the current
            // request to hide it. A request version stamp guards against stale
            // responses overwriting newer ones.
            SetSearchBusy(true);
            await Task.Delay(350, token);

            if (requestVersion != _searchRequestVersion)
            {
                return;
            }

            var result = await _viewModel.PerformSemanticSearchAsync(searchText, token);

            if (requestVersion != _searchRequestVersion)
            {
                return;
            }

            ApplyResult(result);
        }
        catch (OperationCanceledException)
        {
            // Newer keystroke superseded us — leave the UI to the newer request.
        }
        finally
        {
            if (requestVersion == _searchRequestVersion)
            {
                SetSearchBusy(false);
            }
        }
    }

    private void SetSearchBusy(bool isBusy)
    {
        searchBusyIndicator.IsVisible = isBusy;
        searchBusyIndicator.IsRunning = isBusy;
    }

    /// <summary>
    /// Apply the search result to display ONLY matched files in the TreeView.
    /// </summary>
    private void ApplyResult(SearchResult result)
    {
        _viewModel.ExpandAndHighlightMatches(result);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            treeView.ItemsSource = _viewModel.FilteredSalesFileNodes;
            treeView.ExpandAll();
        });
    }

    /// <summary>
    /// Restore the full tree view with all files and clear highlights.
    /// </summary>
    private void SwapToFullTree()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            treeView.ItemsSource = _viewModel.SalesFileNodes;
        });
    }

    private void OnClearSearch(object sender, EventArgs e)
    {
        searchEntry.Text = string.Empty;

        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource?.Dispose();
        _searchCancellationTokenSource = new CancellationTokenSource();

        _viewModel.ClearSearch();
        SwapToFullTree();
    }
}