using System.ComponentModel;
using eSIMDataManager.Models;
using eSIMDataManager.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Buttons;

namespace eSIMDataManager.Controls;

/// <summary>
/// Builds the Top-Up Data dialog content that mirrors the desktop reference design:
/// a centered dialog on wide layouts and a bottom sheet on compact widths.
/// </summary>
public sealed class TopUpPopupContent : IDisposable
{
    private const string InkColor = "#0B1C30";
    private const string MutedColor = "#434656";
    private const string PrimaryColor = "#0046CC";
    private const string PrimaryActionColor = "#0D5CFF";
    private const string SurfaceLow = "#EFF4FF";
    private const string SurfaceContainer = "#E5EEFF";
    private const string SurfaceHighest = "#D3E4FE";
    private const string OutlineVariant = "#C3C5D9";
    private const string UiFont = "Inter";
    private const string MonoFont = "monospace";
    private static readonly Style PopupLabelStyle = new(typeof(Label));

    private readonly MainViewModel viewModel;
    private readonly Grid root;
    private readonly Grid tierContainer;
    private readonly List<Border> tierCards = [];
    private readonly List<Border> tierRadios = [];
    private readonly List<Label> tierSizes = [];
    private readonly Label allowanceLabel;
    private readonly Label purchaseLabel;
    private readonly Label totalValueLabel;
    private readonly View accentBar;
    private readonly VerticalStackLayout chromeView;
    private readonly VerticalStackLayout sheetBody;
    private readonly ScrollView bodyScroll;
    private readonly VerticalStackLayout bannerHost;
    private readonly VerticalStackLayout summaryHost;
    private readonly VerticalStackLayout footerHost;
    private double viewportHeight;
    private bool isCompact;
    private bool isSheet;

    public TopUpPopupContent(MainViewModel viewModel)
    {
        this.viewModel = viewModel;
        // Three auto-sized rows: chrome, body and footer. The body is capped by the
        // scroll view, so the sheet is exactly as tall as the content needs and never
        // taller than the screen.
        root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            BackgroundColor = Colors.White,
        };
        root.Resources.Add(PopupLabelStyle);
        tierContainer = new Grid();
        allowanceLabel = BuildAllowanceLabel();
        purchaseLabel = BuildPurchaseLabel();
        totalValueLabel = BuildTotalValueLabel();

        accentBar = BuildAccentBar();
        chromeView = new VerticalStackLayout { Spacing = 0 };
        chromeView.Add(accentBar);
        chromeView.Add(BottomSheetChrome.BuildHandle());
        chromeView.Add(BuildHeader());
        root.Add(chromeView, 0, 0);

        // The body is a scrollable list of sections. It only starts scrolling once its
        // MaximumHeightRequest is set, so a short sheet shows everything without any scroll
        // bar. When the sheet is taller than the screen, ApplyViewport clamps it.
        sheetBody = new VerticalStackLayout { Spacing = 14 };
        bannerHost = new VerticalStackLayout { Spacing = 0 };
        bannerHost.Add(BuildProfileBanner());
        sheetBody.Add(bannerHost);
        sheetBody.Add(BuildTierSection());
        summaryHost = new VerticalStackLayout { Spacing = 0 };
        summaryHost.Add(BuildSummarySection());
        sheetBody.Add(summaryHost);
        bodyScroll = new ScrollView
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = sheetBody
        };
        root.Add(bodyScroll, 1, 0);

        // The footer is rebuilt when the sheet mode changes, so it is hosted in a stack.
        footerHost = new VerticalStackLayout { Spacing = 0 };
        footerHost.Add(BuildFooter());
        root.Add(footerHost, 2, 0);

        root.SizeChanged += (_, _) => OnRootSized();

        BuildTiers();

        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    public View Content => root;

    public event EventHandler? CloseRequested;
    public event EventHandler? CancelRequested;
    public event EventHandler? PurchaseRequested;
    public event EventHandler? ChangePaymentRequested;

    public void Dispose() => viewModel.PropertyChanged -= ViewModelOnPropertyChanged;

    /// <summary>Switches between the wide three-column tier layout and the compact stacked sheet layout.</summary>
    public void SetCompact(bool compact)
    {
        if (isCompact == compact) return;
        isCompact = compact;
        BuildTiers();
        RebuildBanner();
        RebuildSummary();
        RebuildFooter();
    }

    /// <summary>Switches the dialog into the mobile bottom-sheet presentation.</summary>
    public void SetSheet(bool sheet)
    {
        if (isSheet == sheet) return;
        isSheet = sheet;
        accentBar.IsVisible = !sheet;
        isCompact = sheet || isCompact;
        BuildTiers();
        RebuildBanner();
        RebuildSummary();
        RebuildFooter();
    }

    /// <summary>Rebuilds the footer so it matches the current sheet arrangement.</summary>
    private void RebuildFooter()
    {
        footerHost.Children.Clear();
        footerHost.Add(BuildFooter());
    }

    /// <summary>Rebuilds the summary card so it matches the current sheet arrangement.</summary>
    private void RebuildSummary()
    {
        summaryHost.Children.Clear();
        summaryHost.Add(BuildSummarySection());
    }

    /// <summary>Rebuilds the profile banner so it matches the current sheet arrangement.</summary>
    private void RebuildBanner()
    {
        bannerHost.Children.Clear();
        bannerHost.Add(BuildProfileBanner());
    }

    /// <summary>Used by the popup host to tell the content about the available screen height.</summary>
    public void SetViewport(double viewportHeight)
    {
        this.viewportHeight = viewportHeight;
    }

    /// <summary>
    /// Caps the scrollable body so the sheet never grows past the visible screen. When the
    /// content fits, the body keeps its natural height and the sheet hugs it. When it does
    /// not fit, the body is capped and scrolls instead.
    /// </summary>
    public void ApplyViewport()
    {
        ApplyViewport(root.Width);
    }

    private void ApplyViewport(double width)
    {
        if (viewportHeight <= 0) return;

        // Natural height of the body content, measured without a height constraint.
        var constraint = width > 0 ? width : double.PositiveInfinity;
        var natural = sheetBody.Measure(constraint, double.PositiveInfinity).Height;

        // The sheet may not grow past the screen, so the body gets whatever is left after
        // the chrome and the footer.
        var chromeHeight = chromeView.Height > 0 ? chromeView.Height : 0;
        var footerHeight = footerHost.Height > 0 ? footerHost.Height : 0;
        var limit = viewportHeight - chromeHeight - footerHeight;

        // Only cap when the content really overflows, so a short sheet never scrolls.
        if (natural <= limit)
        {
            if (bodyScroll.MaximumHeightRequest != double.PositiveInfinity)
                bodyScroll.MaximumHeightRequest = double.PositiveInfinity;
            return;
        }

        var target = Math.Max(120, limit);
        if (Math.Abs(bodyScroll.MaximumHeightRequest - target) > 0.5)
            bodyScroll.MaximumHeightRequest = target;
    }

    private void BuildTiers()
    {
        tierContainer.Children.Clear();
        tierCards.Clear();
        tierRadios.Clear();
        tierSizes.Clear();

        if (isCompact)
        {
            var stack = new VerticalStackLayout { Spacing = 10 };
            foreach (var pack in viewModel.TopUpPacks)
            {
                stack.Add(BuildTierCard(pack));
            }
            tierContainer.Add(stack);
            return;
        }

        var grid = new Grid
        {
            ColumnSpacing = 12,
            Padding = new Thickness(3),
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };

        var index = 0;
        foreach (var pack in viewModel.TopUpPacks)
        {
            grid.Add(BuildTierCard(pack), index++);
        }
        tierContainer.Add(grid);
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.SelectedTopUpGb)) return;
        RefreshSelection();
        totalValueLabel.Text = viewModel.SelectedTopUpPack.PriceText;
    }

    private void RefreshSelection()
    {
        for (var index = 0; index < viewModel.TopUpPacks.Count; index++)
        {
            var pack = viewModel.TopUpPacks[index];
            var selected = viewModel.IsTopUpSelected(pack);
            var card = tierCards[index];
            var radio = tierRadios[index];
            var size = tierSizes[index];

            card.BackgroundColor = Color.FromArgb(selected ? "#FFFFFF" : SurfaceLow);
            card.Stroke = Color.FromArgb(selected ? PrimaryColor : SurfaceLow);
            card.StrokeThickness = selected ? 2 : 1;
            size.TextColor = Color.FromArgb(selected ? PrimaryColor : InkColor);

            radio.BackgroundColor = Color.FromArgb(selected ? PrimaryColor : SurfaceHighest);
            radio.Content = new Label
            {
                Text = selected ? "\u2713" : string.Empty,
                FontFamily = UiFont,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
        }

        allowanceLabel.Text = viewModel.TopUpAllowanceSummary;
        purchaseLabel.Text = viewModel.TopUpSubmitLabel;
    }

    private static View BuildAccentBar() => new Border
    {
        HeightRequest = 6,
        StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(3) },
        Background = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            GradientStops =
            {
                new GradientStop(Color.FromArgb(PrimaryColor), 0f),
                new GradientStop(Color.FromArgb(PrimaryActionColor), 0.55f),
                new GradientStop(Color.FromArgb("#00687A"), 1f)
            }
        }
    };

    private View BuildHeader()
    {
        // The reference sheet shows a lightning bolt next to the title.
        var boltIcon = new Label
        {
            Text = "\uE707",
            FontFamily = "MaterialAssets",
            FontSize = 20,
            TextColor = Color.FromArgb(PrimaryColor),
            VerticalOptions = LayoutOptions.Center
        };

        var title = new Label
        {
            Text = "Add More Data",
            FontFamily = UiFont,
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };

        var heading = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        heading.Add(title);
        heading.Add(boltIcon);

        var left = new VerticalStackLayout { Spacing = 4 };
        left.Add(heading);

        var close = new SfButton
        {
            Text = "\uE70B",
            FontFamily = "MaterialAssets",
            FontSize = 16,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(MutedColor),
            WidthRequest = 40,
            HeightRequest = 40,
            CornerRadius = 10,
            Padding = new Thickness(0)
        };
        close.Clicked += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        var header = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            Padding = new Thickness(28, 20, 20, 8)
        };
        header.Add(left);
        header.Add(close, 1);
        return header;
    }

    private View BuildProfileBanner()
    {
        var dot = new Ellipse { WidthRequest = 9, HeightRequest = 9, Fill = Color.FromArgb("#007A56"), VerticalOptions = LayoutOptions.Center };

        var caption = new Label
        {
            Text = "Current Profile:",
            FontFamily = UiFont,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };

        var name = new Label
        {
            Text = $"{viewModel.TopUpProfileName} ({viewModel.TopUpCarrierName})",
            FontFamily = UiFont,
            FontSize = 12.5,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };

        var left = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        left.Add(dot);
        left.Add(caption);
        left.Add(name);

        var remaining = new Label
        {
            Text = viewModel.TopUpRemainingText,
            FontFamily = MonoFont,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(PrimaryColor),
            VerticalOptions = LayoutOptions.Center
        };

        var separator = new Label
        {
            Text = "•",
            FontFamily = UiFont,
            FontSize = 13,
            TextColor = Color.FromArgb(OutlineVariant),
            VerticalOptions = LayoutOptions.Center
        };

        var days = new Label
        {
            Text = viewModel.TopUpDaysLeftText,
            FontFamily = MonoFont,
            FontSize = 13,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Center
        };

        var right = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        right.Add(remaining);
        right.Add(separator);
        right.Add(days);

        // On the phone the two halves do not fit on one line, so they stack.
        var banner = isCompact
            ? BuildStackedBanner(left, right)
            : BuildSideBySideBanner(left, right);

        var host = new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(16, 11)
        };
        host.Content = banner;

        var wrapper = new Border { Padding = new Thickness(28, 4, 28, 0), StrokeThickness = 0 };
        wrapper.Content = host;
        return wrapper;
    }

    private static View BuildSideBySideBanner(View left, View right)
    {
        var banner = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        banner.Add(left);
        banner.Add(right, 1);
        return banner;
    }

    private static View BuildStackedBanner(View left, View right)
    {
        var banner = new VerticalStackLayout { Spacing = 4 };
        banner.Add(left);
        banner.Add(right);
        return banner;
    }

    private View BuildTierSection()
    {
        var label = new Label
        {
            Text = "SELECT DATA EXTENSION TIER",
            FontFamily = UiFont,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(MutedColor)
        };

        var body = new VerticalStackLayout { Spacing = 10 };
        body.Add(label);
        body.Add(tierContainer);

        var wrapper = new Border { Padding = new Thickness(28, 18, 28, 0), StrokeThickness = 0 };
        wrapper.Content = body;
        return wrapper;
    }

    private View BuildTierCard(TopUpPack pack)
    {
        var selected = viewModel.IsTopUpSelected(pack);

        var size = new Label
        {
            Text = pack.DataLabel,
            FontFamily = UiFont,
            FontSize = 19,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(selected ? PrimaryColor : InkColor),
            VerticalOptions = LayoutOptions.Center
        };

        if (isCompact) return BuildCompactTierCard(pack, selected, size);

        var radio = new Border
        {
            WidthRequest = 22,
            HeightRequest = 22,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(11) },
            BackgroundColor = Color.FromArgb(selected ? PrimaryColor : SurfaceHighest),
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = selected ? "\u2713" : string.Empty,
                FontFamily = UiFont,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var heading = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        heading.Add(size);
        heading.Add(radio, 1);

        var extension = new Label
        {
            Text = pack.ExtensionLabel,
            FontFamily = MonoFont,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#006141"),
            Margin = new Thickness(0, 8, 0, 0)
        };

        var description = new Label
        {
            Text = pack.Description,
            FontFamily = UiFont,
            FontSize = 12,
            TextColor = Color.FromArgb(MutedColor),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var price = new Label
        {
            Text = pack.PriceText,
            FontFamily = UiFont,
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor)
        };

        var priceRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, VerticalOptions = LayoutOptions.End };
        priceRow.Add(price);

        if (pack.IsPopular)
        {
            var perGb = new Label
            {
                Text = pack.PricePerGbText,
                FontFamily = MonoFont,
                FontSize = 12,
                TextColor = Color.FromArgb(MutedColor),
                VerticalOptions = LayoutOptions.End
            };
            priceRow.Add(perGb, 1);
        }

        var body = new VerticalStackLayout { Spacing = 0 };
        body.Add(heading);
        body.Add(extension);
        body.Add(description);
        body.Add(new BoxView { HeightRequest = 1, Color = Colors.Transparent, Margin = new Thickness(0, 14, 0, 0) });
        body.Add(priceRow);

        var card = new Border
        {
            BackgroundColor = Color.FromArgb(selected ? "#FFFFFF" : SurfaceLow),
            Stroke = Color.FromArgb(selected ? PrimaryColor : SurfaceLow),
            StrokeThickness = selected ? 2 : 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Padding = new Thickness(16),
            Content = body
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => viewModel.SelectTopUpPackCommand.Execute(pack);
        card.GestureRecognizers.Add(tap);

        tierCards.Add(card);
        tierRadios.Add(radio);
        tierSizes.Add(size);

        if (pack.Tag is { } tag)
        {
            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(pack.IsPopular ? PrimaryColor : SurfaceHighest),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
                Padding = new Thickness(10, 3),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                TranslationY = -10,
                Content = new Label
                {
                    Text = tag.ToUpperInvariant(),
                    FontFamily = UiFont,
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb(pack.IsPopular ? "#FFFFFF" : PrimaryColor)
                }
            };

            var stack = new Grid { Padding = new Thickness(0, 10, 0, 0) };
            stack.Add(card);
            stack.Add(badge);
            return stack;
        }

        var unbadgedStack = new Grid { Padding = new Thickness(0, 10, 0, 0) };
        unbadgedStack.Add(card);
        return unbadgedStack;
    }

    private View BuildCompactTierCard(TopUpPack pack, bool selected, Label size)
    {
        var radio = new Border
        {
            WidthRequest = 22,
            HeightRequest = 22,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(11) },
            BackgroundColor = Color.FromArgb(selected ? PrimaryColor : "#FFFFFF"),
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = selected ? "\u2713" : string.Empty,
                FontFamily = UiFont,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var extension = new Label
        {
            Text = pack.ExtensionLabel.Replace(" extension", string.Empty),
            FontFamily = MonoFont,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#006141"),
            VerticalOptions = LayoutOptions.Center
        };

        var description = new Label
        {
            Text = pack.Description,
            FontFamily = UiFont,
            FontSize = 12,
            TextColor = Color.FromArgb(MutedColor),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var titleRow = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        titleRow.Add(size);
        titleRow.Add(extension);

        var text = new VerticalStackLayout { Spacing = 2 };
        text.Add(titleRow);
        text.Add(description);

        var left = new HorizontalStackLayout { Spacing = 12 };
        left.Add(radio);
        left.Add(text);

        var price = new Label
        {
            Text = pack.PriceText,
            FontFamily = UiFont,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(selected ? PrimaryColor : InkColor),
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Start
        };

        var perGb = new Label
        {
            Text = pack.PricePerGbText,
            FontFamily = MonoFont,
            FontSize = 11,
            TextColor = Color.FromArgb(MutedColor),
            HorizontalTextAlignment = TextAlignment.End
        };

        var right = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.End };
        right.Add(price);
        right.Add(perGb);

        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        row.Add(left);
        row.Add(right, 1);

        var card = new Border
        {
            BackgroundColor = Color.FromArgb(selected ? SurfaceContainer : SurfaceLow),
            Stroke = Color.FromArgb(selected ? PrimaryColor : SurfaceLow),
            StrokeThickness = selected ? 2 : 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Padding = new Thickness(16, 14),
            Content = row
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => viewModel.SelectTopUpPackCommand.Execute(pack);
        card.GestureRecognizers.Add(tap);

        tierCards.Add(card);
        tierRadios.Add(radio);
        tierSizes.Add(size);

        if (pack.Tag is not { } tag) return card;

        var badge = new Border
        {
            BackgroundColor = Color.FromArgb(pack.IsPopular ? PrimaryColor : SurfaceHighest),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(10, 3),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            TranslationY = -10,
            Content = new Label
            {
                Text = tag.ToUpperInvariant(),
                FontFamily = UiFont,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(pack.IsPopular ? "#FFFFFF" : PrimaryColor)
            }
        };

        var stack = new Grid { Padding = new Thickness(0, 10, 0, 0) };
        stack.Add(card);
        stack.Add(badge);
        return stack;
    }

    private Label BuildAllowanceLabel() => new Label
    {
        Text = viewModel.TopUpAllowanceSummary,
        FontFamily = MonoFont,
        FontSize = 13,
        FontAttributes = FontAttributes.Bold,
        TextColor = Color.FromArgb(InkColor),
        VerticalOptions = LayoutOptions.Center
    };

    private View BuildSummarySection()
    {
        var speedIcon = new Label
        {
            Text = "⚡️",
            FontSize = 15,
            TextColor = Color.FromArgb("#006141"),
            VerticalOptions = LayoutOptions.Center
        };
        var speedLabel = new Label
        {
            Text = "Provisioning Speed",
            FontFamily = UiFont,
            FontSize = 13,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Center
        };
        var speedLeft = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        speedLeft.Add(speedIcon);
        speedLeft.Add(speedLabel);
        var speedRight = new Label
        {
            Text = "Instant Activation Upon Payment",
            FontFamily = UiFont,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };
        var speedRow = BuildSummaryRow(speedLeft, speedRight);

        var allowanceLeft = new Label
        {
            Text = "Selected Top-Up Allowance",
            FontFamily = UiFont,
            FontSize = 13,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Center
        };
        var allowanceRow = BuildSummaryRow(allowanceLeft, allowanceLabel);

        var cardBadge = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(5) },
            Padding = new Thickness(8, 2),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = "VISA",
                FontFamily = UiFont,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(PrimaryColor)
            }
        };

        var cardLabel = new Label
        {
            Text = "Auto-applied card ending in 4242",
            FontFamily = UiFont,
            FontSize = 13,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };
        var cardLeft = new HorizontalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center };
        cardLeft.Add(cardBadge);
        cardLeft.Add(cardLabel);

        var change = new SfButton
        {
            Text = "Change",
            FontFamily = UiFont,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(PrimaryColor),
            Padding = new Thickness(8, 4),
            MinimumHeightRequest = 32
        };
        change.Clicked += (_, _) => ChangePaymentRequested?.Invoke(this, EventArgs.Empty);

        var cardRow = BuildSummaryRow(cardLeft, change);

        // Total due row, shown like the reference sheet.
        var totalLabel = new Label
        {
            Text = "Total Due",
            FontFamily = UiFont,
            FontSize = 13,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Center
        };
        var totalCurrency = new Label
        {
            Text = "USD",
            FontFamily = UiFont,
            FontSize = 12,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.End
        };
        var totalRight = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        totalRight.Add(totalCurrency);
        totalRight.Add(totalValueLabel);

        var totalRow = BuildSummaryRow(totalLabel, totalRight);

        var body = new VerticalStackLayout { Spacing = 10 };
        body.Add(speedRow);
        body.Add(allowanceRow);
        body.Add(cardRow);
        body.Add(totalRow);

        // Note below the summary card, as shown in the reference sheet.
        var noteDot = new Label
        {
            Text = "\u2713",
            FontFamily = UiFont,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(TertiaryContainer),
            VerticalOptions = LayoutOptions.Start
        };
        var noteText = new Label
        {
            Text = "No re-installation needed. Roaming continues automatically.",
            FontFamily = UiFont,
            FontSize = 11,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Start
        };
        var noteLeft = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Start };
        noteLeft.Add(noteDot);
        noteLeft.Add(noteText);

        var host = new Border
        {
            BackgroundColor = Color.FromArgb("#F5F8FF"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(16)
        };
        host.Content = body;

        var wrapperStack = new VerticalStackLayout { Spacing = 8 };
        wrapperStack.Add(host);
        wrapperStack.Add(noteLeft);

        var wrapper = new Border { Padding = new Thickness(28, 18, 28, 0), StrokeThickness = 0 };
        wrapper.Content = wrapperStack;
        return wrapper;
    }

    /// <summary>
    /// Builds one label/value row of the summary card. On the phone the value moves under
    /// the label so the row never squeezes the text.
    /// </summary>
    private View BuildSummaryRow(View label, View value)
    {
        if (isCompact)
        {
            var stacked = new VerticalStackLayout { Spacing = 2 };
            stacked.Add(label);
            stacked.Add(value);
            return stacked;
        }

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };
        row.Add(label, 0, 0);
        row.Add(value, 1, 0);
        return row;
    }

    private Label BuildTotalValueLabel() => new Label
    {
        Text = viewModel.SelectedTopUpPack.PriceText,
        FontFamily = UiFont,
        FontSize = 26,
        FontAttributes = FontAttributes.Bold,
        TextColor = Color.FromArgb(InkColor),
        VerticalOptions = LayoutOptions.Center
    };

    private Label BuildPurchaseLabel() => new Label
    {
        Text = viewModel.TopUpSubmitLabel,
        FontFamily = UiFont,
        FontSize = 15,
        FontAttributes = FontAttributes.Bold,
        TextColor = Colors.White,
        VerticalOptions = LayoutOptions.Center
    };

    private View BuildFooter()
    {
        var purchase = new SfButton
        {
            FontFamily = UiFont,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            Background = Color.FromArgb(PrimaryActionColor),
            TextColor = Colors.White,
            CornerRadius = 10,
            Padding = new Thickness(16, 6),
            HeightRequest = 44,
            MinimumHeightRequest = 44,
            Content = new DataTemplate(() =>
            {
                var row = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(8, 0), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                row.Add(new Label
                {
                    Text = "\uE77B",
                    FontFamily = "MaterialAssets",
                    FontSize = 15,
                    TextColor = Colors.White,
                    VerticalOptions = LayoutOptions.Center
                });
                row.Add(purchaseLabel);
                return row;
            })
        };
        purchase.Clicked += (_, _) => PurchaseRequested?.Invoke(this, EventArgs.Empty);

        var cancel = new SfButton
        {
            Text = "Cancel",
            FontFamily = UiFont,
            FontSize = 15,
            Background = Color.FromArgb(SurfaceLow),
            TextColor = Color.FromArgb(InkColor),
            CornerRadius = 10,
            Padding = new Thickness(20, 6),
            HeightRequest = 44,
            MinimumHeightRequest = 44
        };
        cancel.Clicked += (_, _) => CancelRequested?.Invoke(this, EventArgs.Empty);

        // On the phone the purchase button fills the width and Cancel sits below it, which
        // matches the reference sheet. The wide dialog keeps them on one row.
        var actions = new VerticalStackLayout { Spacing = 12 };
        if (isCompact)
        {
            purchase.HorizontalOptions = LayoutOptions.Fill;
            purchase.VerticalOptions = LayoutOptions.Fill;
            cancel.HorizontalOptions = LayoutOptions.Center;
            actions.Add(purchase);
            actions.Add(cancel);
        }
        else
        {
            var row = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
                ColumnSpacing = 12,
                HorizontalOptions = LayoutOptions.End
            };
            row.Add(cancel);
            row.Add(purchase, 1);
            actions.Add(row);
        }

        var footer = new Border { Padding = new Thickness(28, 16, 28, 22), StrokeThickness = 0 };
        footer.Content = actions;
        return footer;
    }
    /// <summary>Raised when the sheet content resizes, so the popup can be re-anchored.</summary>
    public event EventHandler? SheetResized;

    private void OnRootSized()
    {
        ApplyViewport();
        SheetResized?.Invoke(this, EventArgs.Empty);
    }

}
