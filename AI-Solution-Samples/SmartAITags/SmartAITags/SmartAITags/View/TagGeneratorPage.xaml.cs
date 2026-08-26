using System.Globalization;

namespace SmartAITags;

/// <summary>
/// Tag generator page. Wires the Entry clear icon, the Generate Tags button,
/// the four-state result panel (empty / loading / chips / error) and the
/// staggered chip-appearance animation.
/// </summary>
public partial class TagGeneratorPage : ContentPage
{
    public TagGeneratorPage()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is ViewModel.TagViewModel vm)
        {
            vm.TagsReceived -= OnTagsReceived;
            vm.TagsReceived += OnTagsReceived;
        }
    }

    private void OnQueryTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue ?? string.Empty;
        queryInputLayout.ShowHint = string.IsNullOrEmpty(text);
        //ClearIconButton.IsVisible = !string.IsNullOrEmpty(text);
    }

    /// <summary>Enter / Return while editing → fire Generate.</summary>
    private void OnQueryCompleted(object? sender, EventArgs e) => RunAISearch();

    /// <summary>Clear (×) icon tapped → reset input + chips.</summary>
    private void OnClearClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not ViewModel.TagViewModel vm)
            return;

        vm.Clear();                              // clears Tags + HasSearched
        queryInputLayout.ShowHint = true;
        queryEditor.Text = string.Empty;         // re-fires TextChanged → hides Clear icon
    }

    /// <summary>"Try again" button on the error state.</summary>
    private void OnRetryClicked(object? sender, EventArgs e) => RunAISearch();

    private void RunAISearch()
    {
        if (BindingContext is not ViewModel.TagViewModel vm || vm.IsBusy)
            return;

        string query = queryEditor.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query)) return;

        _ = vm.GenerateTagsAsync(query);
    }

    /// <summary>
    /// Adds the AI-returned tags one-by-one with a 120 ms stagger so each chip
    /// visibly appears. <c>IsBusy</c> is already false by the time this fires,
    /// so the loading indicator is gone and the chip panel is ready.
    /// </summary>
    private void OnTagsReceived(object? sender, IEnumerable<string> tags)
    {
        if (BindingContext is not ViewModel.TagViewModel vm)
            return;

        var clean = tags
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            foreach (var t in clean)
            {
                vm.Tags.Add(new Model.TagModel { Name = t });
                await Task.Delay(120); // stagger chip appearance
            }
        });
    }
}