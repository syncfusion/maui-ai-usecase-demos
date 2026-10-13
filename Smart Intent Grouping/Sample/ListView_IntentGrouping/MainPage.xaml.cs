using System.ComponentModel;
using ListView_IntentGrouping.Models;
using ListView_IntentGrouping.Services;
using ListView_IntentGrouping.ViewModels;
using Syncfusion.Maui.DataSource;
using Syncfusion.Maui.ListView;

namespace ListView_IntentGrouping;

/// <summary>
/// Main page code-behind. Pairs with <see cref="MainPageViewModel"/>:
///   * picks the correct <c>ItemsLayout</c> based on the running
///     platform (single-column list for phones, 2-column grid for
///     tablets / desktop / full-screen),
///   * wires the SfListView's <c>GroupDescriptor</c> against
///     <see cref="FeedbackItem.GroupKey"/>, and listens for the
///     view-model's <see cref="MainPageViewModel.IsGrouped"/>
///     changes so the Group button can refresh the groups without
///     rebuilding the data source,
///   * forwards the Enter / Done key on the prompt entry to
///     <see cref="MainPageViewModel.GroupAsync"/>.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        ConfigureItemsLayout();
        InstallGroupDescriptor();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.Items.Count == 0)
        {
            await _viewModel.LoadInitialItemsAsync();
        }
    }

    /// <summary>
    /// Pick the right SfListView layout for the running platform.
    /// Phones get a vertical linear list (one item per row);
    /// tablets / desktop / full-screen get a 2-column grid so the
    /// content reads nicely without scrolling sideways.
    /// </summary>
    private void ConfigureItemsLayout()
    {
        if (IntentListView is null) return;
        IntentListView.ItemsLayout = DeviceInfo.Current.Idiom == DeviceIdiom.Phone
            ? new LinearLayout()
            : new GridLayout { SpanCount = 2 };
    }

    /// <summary>
    /// Install or remove the single GroupDescriptor that powers the
    /// SfListView based on the view-model's current grouping mode.
    /// We always group by <see cref="FeedbackItem.GroupKey"/>; the
    /// view-model sets (or clears) that key based on the user's
    /// prompt. When the view-model is not grouped (initial load /
    /// after Reset) the descriptor is removed so the SfListView
    /// shows a single, flat list with no group headers.
    /// </summary>
    private void InstallGroupDescriptor()
    {
        if (IntentListView is null) return;

        // SfListView lazily creates its data source on first access
        // and may return null GroupDescriptors until then. Lazy-init
        // it here so the very first Group click is wired up correctly.
        if (IntentListView.DataSource is null)
        {
            IntentListView.DataSource = new Syncfusion.Maui.DataSource.DataSource();
        }
        var descriptors = IntentListView.DataSource.GroupDescriptors;
        if (descriptors is null) return;
        descriptors.Clear();
        if (_viewModel.IsGrouped)
        {
            descriptors.Add(new GroupDescriptor
            {
                PropertyName = nameof(FeedbackItem.GroupKey)
            });
        }
        // Force the SfListView to rebuild its groups/items projection
        // from the recently-populated GroupKey references.
        IntentListView.IsStickyHeader = _viewModel.IsGrouped;

        // Re-project the groups / items from the freshly-stamped
        // GroupKey references. Using DataSource.Refresh() is the
        // supported way to ask SfListView's projection to rebuild;
        // the older pattern of nulling and re-setting
        // ItemsSource is unreliable in MAUI when the collection
        // reference is identical between renders.
        try
        {
            IntentListView.DataSource.Refresh();
        }
        catch
        {
            // Fall back to a deliberate ItemsSource swap between
            // two non-null values so SfListView always re-projects.
            var src = IntentListView.ItemsSource;
            IntentListView.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<FeedbackItem>();
            IntentListView.ItemsSource = src;
        }
    }

    /// <summary>
    /// React to changes in the view-model's grouping state. Toggling
    /// between flat and grouped views requires us to refresh the
    /// GroupDescriptor list and the sticky-header toggle so the
    /// SfListView reflects the current mode without rebuilding
    /// the data source.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IntentListView is null) return;
        if (e.PropertyName == nameof(MainPageViewModel.IsGrouped) ||
            e.PropertyName == nameof(MainPageViewModel.CurrentGrouping))
        {
            InstallGroupDescriptor();
        }
    }

    /// <summary>
    /// Forward the Enter / Done key on the prompt entry to the
    /// view-model so the user always sees the list refresh when
    /// they commit a new prompt.
    /// </summary>
    private async void OnPromptCompleted(object? sender, EventArgs e)
    {
        if (sender is Entry entry)
        {
            _viewModel.Prompt = entry.Text ?? string.Empty;
            await _viewModel.GroupAsync();
        }
    }

    /// <summary>
    /// Toggle the expand / collapse state of a single
    /// <c>GroupResult</c> when the user taps the chevron in the
    /// group header's trailing tile.
    ///
    /// <para>
    /// The chevron <c>Label</c>'s <c>BindingContext</c> is the
    /// <c>GroupResult</c> projected by SfListView for that group
    /// (the same context the rest of the header template reads).
    /// We pull the <c>GroupResult</c> back out of the tapped
    /// element's <c>BindingContext</c> and call
    /// <c>IntentListView.ExpandGroup</c> /
    /// <c>IntentListView.CollapseGroup</c> accordingly. SfListView
    /// flips <c>IsExpand</c> for us — which the chevron glyph
    /// converter then reflects in the next layout pass.
    /// </para>
    /// </summary>
    private void OnGroupHeaderToggleTapped(object? sender, TappedEventArgs e)
    {
        if (IntentListView is null) return;

        // The tap can fire on the chevron Label itself, the wrapping
        // Border, or any of the parent containers. Walk up the visual
        // tree from the tap source until we find a BindingContext that
        // is the GroupResult we want to toggle. This keeps the handler
        // resilient to MAUI reparenting the visual nodes between
        // touch-down and tap-fire.
        Syncfusion.Maui.DataSource.Extensions.GroupResult? target = null;
        if (sender is VisualElement ve)
        {
            var node = (VisualElement?)ve;
            while (node is not null)
            {
                if (node.BindingContext is Syncfusion.Maui.DataSource.Extensions.GroupResult gr)
                {
                    target = gr;
                    break;
                }
                node = node.Parent as VisualElement;
            }
        }
        if (target is null) return;

        try
        {
            if (target.IsExpand)
            {
                IntentListView.CollapseGroup(target);
            }
            else
            {
                IntentListView.ExpandGroup(target);
            }
        }
        catch
        {
            // Swallow: in transient layouts (e.g. mid-rebuild right
            // after a Group click) the GroupResult we captured can
            // already be stale. The next tap will operate on the
            // freshly projected group surface.
        }
    }
}