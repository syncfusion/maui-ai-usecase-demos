using AIChatSample.Controls;
using System.Diagnostics;

namespace AIChatSample;

public partial class AgentLogoPage : ContentPage
{
    private AgentLogoPopupViewModel _vm;

    public AgentLogoPage() : this(null) { }

    public AgentLogoPage(AgentLogoPopupViewModel? viewModel)
    {
        InitializeComponent();
        _vm = viewModel ?? new AgentLogoPopupViewModel();

        // Assign the BindingContext unconditionally. The previous
        // `if (BindingContext is null)` guard meant that whenever MAUI
        // propagated a non-null BindingContext into the modal page (which
        // happens on some platforms, e.g. Android when the page is pushed
        // as a modal), the supplied AgentLogoPopupViewModel was never
        // wired up — AvailableIcons / AvailableColors stayed empty and
        // the icon row rendered blank, so nothing was selectable.
        BindingContext = _vm;
        _vm.RequestClose += OnRequestClose;
        _vm.ResetForOpen();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Task.Delay(500);
        LogoPopup.Show();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is AgentLogoPopupViewModel incoming && incoming != _vm)
        {
            _vm.RequestClose -= OnRequestClose;
            _vm = incoming;
            _vm.RequestClose += OnRequestClose;
            _vm.ResetForOpen();
        }
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        LogoPopup.IsOpen = false;
        if (Navigation?.NavigationStack.Count > 1)
        {
            Navigation.PopAsync(animated: true);
        }
    }

    /// <summary>
    /// Handles a tap on an icon in the "Icons" row. <c>SfListView</c>'s
    /// internal tap-to-select is unreliable on Android when the list is
    /// horizontally scrolled inside a modal popup — the tap is consumed
    /// by the scroll gesture and <c>SelectedItem</c> never updates, so
    /// the icon never appears selected. Handle <c>ItemTapped</c>
    /// explicitly and assign <see cref="AgentLogoPopupViewModel.SelectedIcon"/>
    /// directly; the view-model already syncs the per-item
    /// <c>IsSelected</c> flags and the live preview.
    /// </summary>
    private void OnIconTapped(object? sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (BindingContext is AgentLogoPopupViewModel vm && e.DataItem is LogoIconModel icon)
        {
            vm.SelectedIcon = icon;
        }
    }

    /// <summary>
    /// Handles a tap on a color swatch in the "Colors" row. Same
    /// Android scroll-gesture reliability issue as
    /// <see cref="OnIconTapped"/> — assign
    /// <see cref="AgentLogoPopupViewModel.SelectedColor"/> directly so
    /// the preview background updates.
    /// </summary>
    private void OnColorTapped(object? sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (BindingContext is AgentLogoPopupViewModel vm && e.DataItem is LogoColorModel color)
        {
            vm.SelectedColor = color;
        }
    }
}
