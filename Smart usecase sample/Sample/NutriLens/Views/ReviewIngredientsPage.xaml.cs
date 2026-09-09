using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class ReviewIngredientsPage : ContentPage
{
    private const double DesktopWidthBreakpoint = 900;

    private bool isDesktopLayout;

    public ReviewIngredientsPage()
    {
        InitializeComponent();
        BindingContext = new ReviewIngredientsViewModel();

        ApplyResponsiveLayout();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ReviewIngredientsViewModel vm)
            vm.HydrateFromPendingReview();

        ApplyResponsiveLayout();
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        if (ReviewContentGrid is null)
            return;

        var useDesktopLayout =
            DeviceInfo.Platform == DevicePlatform.WinUI ||
            DeviceInfo.Platform == DevicePlatform.MacCatalyst ||
            Width > DesktopWidthBreakpoint;

        if (useDesktopLayout == isDesktopLayout)
            return;

        isDesktopLayout = useDesktopLayout;

        if (useDesktopLayout)
        {
            ApplyDesktopLayout();
        }
        else
        {
            ApplyMobileLayout();
        }
    }

    private void ApplyDesktopLayout()
    {
        ReviewContentGrid.ColumnDefinitions.Clear();
        ReviewContentGrid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(2, GridUnitType.Star)
            });
        ReviewContentGrid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(2, GridUnitType.Star)
            });

        Grid.SetColumn(ScannedImagePanel, 0);
        Grid.SetColumn(IngredientContentScrollView, 1);

        ScannedImagePanel.IsVisible = true;
        IngredientContentScrollView.VerticalScrollBarVisibility =
            ScrollBarVisibility.Always;
    }

    private void ApplyMobileLayout()
    {
        ReviewContentGrid.ColumnDefinitions.Clear();
        ReviewContentGrid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = GridLength.Star
            });

        Grid.SetColumn(ScannedImagePanel, 0);
        Grid.SetColumn(IngredientContentScrollView, 0);

        ScannedImagePanel.IsVisible = false;
        IngredientContentScrollView.VerticalScrollBarVisibility =
            ScrollBarVisibility.Never;
    }
}