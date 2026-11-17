using eSIMDataManager.Models;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Buttons;

namespace eSIMDataManager.Controls;

/// <summary>
/// Builds the Purchase Plan dialog content that mirrors the reference design:
/// a secure-checkout header, plan summary banner, cost breakdown, payment method,
/// trust badges, and the confirm/cancel action footer. The body scrolls when the
/// content overflows the dialog so the footer stays visible.
/// </summary>
public sealed class PurchasePopupContent
{
    private const string InkColor = "#0B1C30";
    private const string MutedColor = "#434656";
    private const string PrimaryColor = "#0046CC";
    private const string PrimaryContainer = "#0D5CFF";
    private const string SurfaceLow = "#EFF4FF";
    private const string SurfaceContainer = "#E5EEFF";
    private const string SurfaceHighest = "#D3E4FE";
    private const string SurfaceLowest = "#FFFFFF";
    private const string OutlineVariant = "#C3C5D9";
    private const string OutlineColor = "#737688";
    private const string Tertiary = "#006141";
    private const string TertiaryContainer = "#007C55";
    private const string TertiaryFixed = "#6FFBBE";
    private const string OnTertiaryFixed = "#002113";
    private const string Secondary = "#00687A";
    private const string UiFont = "Inter";
    private const string MonoFont = "monospace";

    // Material icon codepoints available in the bundled MauiMaterialAssets font.
    private const string IconClose = "\uE70B";
    private const string IconCheckTick = "\uE70C";
    private const string IconSearch = "\uE715";
    private const string IconEmail = "\uE717";
    private const string IconPhone = "\uE718";
    private const string IconCreditCard = "\uE76E";
    private const string IconDataUsage = "\uE727";
    private const string IconCalendar = "\uE71D";
    private const string IconBolt = "\uE707";
    private const string IconInfo = "\uE7C6";
    private const string IconLock = "\uE77B";
    private const string IconShieldCheck = "\uE78B";
    private const string IconCircleCheck = "\uE78C";

    /// <summary>
    /// Shadows the app-level Label style. The app style sets TextColor through an AppThemeBinding,
    /// which MAUI resolves as a theme-specific value that overrides explicit colors and renders
    /// white text on the white dialog surface. A local implicit style takes precedence over it.
    /// </summary>
    private static readonly Style PopupLabelStyle = new(typeof(Label))
    {
        Setters =
        {
            new Setter { Property = Label.TextColorProperty, Value = Color.FromArgb(InkColor) },
            new Setter { Property = Label.FontFamilyProperty, Value = UiFont },
            new Setter { Property = Label.FontSizeProperty, Value = 11d },
            new Setter { Property = Label.BackgroundColorProperty, Value = Colors.Transparent }
        }
    };

    private readonly Grid root;
    private readonly ScrollView bodyScroll;
    private readonly VerticalStackLayout bodyStack;
    private readonly VerticalStackLayout chromeView;
    private readonly View footerView;
    private readonly View accentBar;
    private readonly Plan? plan;
    private bool isCompact;
    private bool isSheet;
    private double viewportHeight;

    public PurchasePopupContent(Plan? plan)
    {
        this.plan = plan;

        // Three rows: chrome (auto), body (star, scrolls when needed), footer (auto).
        // The body row fills any remaining vertical space so the action buttons stay pinned
        // to the bottom of the sheet.
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

        accentBar = BuildAccentBar();
        chromeView = new VerticalStackLayout { Spacing = 0 };
        chromeView.Add(accentBar);
        chromeView.Add(BottomSheetChrome.BuildHandle());
        chromeView.Add(BuildHeader());
        root.Add(chromeView, 0, 0);

        // The body holds the plan banner, the price summary, the payment method and the
        // trust badges. It is rebuilt when the sheet arrangement changes.
        bodyStack = new VerticalStackLayout { Spacing = 18 };
        bodyScroll = new ScrollView
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = bodyStack
        };

        root.Add(bodyScroll, 1, 0);
        footerView = BuildFooter(plan);
        root.Add(footerView, 2, 0);
        root.SizeChanged += (_, _) => OnRootSized();

        RebuildBody();
    }

    public View Content => root;

    /// <summary>Raised when the user closes the dialog.</summary>
    public event EventHandler? CloseRequested;

    /// <summary>Raised when the user cancels the purchase.</summary>
    public event EventHandler? CancelRequested;

    /// <summary>Raised when the user confirms the simulated purchase.</summary>
    public event EventHandler? PurchaseRequested;

    /// <summary>Raised when the user asks to change the payment method.</summary>
    public event EventHandler? ChangePaymentRequested;

    /// <summary>Switches between the wide and compact arrangements of the metric cards.</summary>
    public void SetCompact(bool compact)
    {
        if (isCompact == compact) return;
        isCompact = compact;
        RebuildBody();
    }

    /// <summary>Switches the dialog into the mobile bottom-sheet presentation.</summary>
    public void SetSheet(bool sheet)
    {
        if (isSheet == sheet) return;
        isSheet = sheet;
        accentBar.IsVisible = !sheet;
        isCompact = sheet || isCompact;
        RebuildBody();
    }

    /// <summary>Rebuilds the body so its sections use the current sheet arrangement.</summary>
    private void RebuildBody()
    {
        bodyStack.Children.Clear();
        bodyStack.Add(BuildPlanBanner(plan));
        bodyStack.Add(BuildCostBreakdown(plan));
        bodyStack.Add(BuildPaymentMethod());
        bodyStack.Add(BuildTrustBadges());
    }

    /// <summary>Used by the popup host to tell the content about the available screen height.</summary>
    public void SetViewport(double viewportHeight)
    {
        this.viewportHeight = viewportHeight;
    }

    /// <summary>
    /// Clamps the scrollable body so the footer stays pinned to the bottom of the sheet. The
    /// body only starts scrolling once the sheet reaches the viewport height.
    /// </summary>
    public void ApplyViewport()
    {
        if (viewportHeight <= 0) return;

        // Natural height of the body content, measured without a height constraint.
        var constraint = root.Width > 0 ? root.Width : double.PositiveInfinity;
        var body = bodyScroll.Content;
        var natural = body is null ? 0 : body.Measure(constraint, double.PositiveInfinity).Height;

        // The sheet may not grow past the screen, so the body gets whatever is left after
        // the chrome and the footer.
        var chromeHeight = chromeView.Height > 0 ? chromeView.Height : 0;
        var footerHeight = footerView.Height > 0 ? footerView.Height : 0;
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

    // -- helpers ----------------------------------------------------------

    /// <summary>Label with explicit color (stable across app themes).</summary>
    private static Label Text(string text, double size, Color color, FontAttributes attributes = FontAttributes.None)
    {
        var label = new Label
        {
            Text = text,
            FontFamily = UiFont,
            FontSize = size,
            FontAttributes = attributes,
            LineBreakMode = LineBreakMode.WordWrap
        };
        label.SetAppThemeColor(Label.TextColorProperty, color, color);
        return label;
    }

    /// <summary>Monospaced label for telemetry identifiers.</summary>
    private static Label MonoText(string text, double size, Color color, FontAttributes attributes = FontAttributes.None)
    {
        var label = new Label
        {
            Text = text,
            FontFamily = MonoFont,
            FontSize = size,
            FontAttributes = attributes,
            LineBreakMode = LineBreakMode.WordWrap
        };
        label.SetAppThemeColor(Label.TextColorProperty, color, color);
        return label;
    }

    /// <summary>Material icon label (must use MaterialAssets font).</summary>
    private static Label Icon(string glyph, double size, Color color)
    {
        var label = new Label
        {
            Text = glyph,
            FontFamily = "MaterialAssets",
            FontSize = size,
            TextColor = color,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        label.SetAppThemeColor(Label.TextColorProperty, color, color);
        return label;
    }

    /// <summary>Rounded surface card used for the banner, cost breakdown, and payment sections.</summary>
    private static Border Card(Color background, View content, double radius = 12, Thickness? padding = null)
    {
        return new Border
        {
            BackgroundColor = background,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(radius) },
            Padding = padding ?? new Thickness(16),
            Content = content
        };
    }

    /// <summary>Illustrative roaming tax: 6% of the package price, rounded to the nearest 5 cents
    /// (regulatory fees are billed in 5-cent steps).
    /// </summary>
    private static decimal TaxFor(decimal packageCost)
    {
        const decimal step = 0.05m;
        var raw = packageCost * 0.06m;
        return decimal.Round(raw / step, MidpointRounding.AwayFromZero) * step;
    }

    /// <summary>Validity caption, title-cased to match the reference design.</summary>
    private static string ValidityCaption(Plan? plan)
    {
        var note = plan?.ValidityNote ?? "From activation";
        var words = note.Split(' ');
        for (var i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..];
        }
        return string.Join(' ', words);
    }

    // -- sections --------------------------------------------------------

    /// <summary>Thin gradient highlight strip across the top of the dialog.</summary>
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
                new GradientStop(Color.FromArgb("#58CFFD"), 0.5f),
                new GradientStop(Color.FromArgb(TertiaryFixed), 1f)
            }
        }
    };

    /// <summary>Header: secure-checkout pill, live gateway, title, subtitle, and close button.</summary>
    private View BuildHeader()
    {
        var securePill = new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceContainer),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(8, 3),
            VerticalOptions = LayoutOptions.Center,
            Content = MonoText("SECURE CHECKOUT", 10, Color.FromArgb(PrimaryColor), FontAttributes.Bold)
        };

        var gatewayDot = new Ellipse
        {
            WidthRequest = 6,
            HeightRequest = 6,
            Fill = Color.FromArgb(TertiaryContainer),
            VerticalOptions = LayoutOptions.Center
        };
        var gatewayText = MonoText("Live Gateway", 10, Color.FromArgb(TertiaryContainer), FontAttributes.Bold);
        gatewayText.VerticalOptions = LayoutOptions.Center;

        var gateway = new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };
        gateway.Add(gatewayDot);
        gateway.Add(gatewayText);

        var badges = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        badges.Add(securePill);
        badges.Add(gateway);

        var title = Text("Purchase Plan", 28, Color.FromArgb(InkColor), FontAttributes.Bold);
        var subtitle = Text("Review your travel eSIM plan details and payment summary.", 13, Color.FromArgb(MutedColor));

        var left = new VerticalStackLayout { Spacing = 4 };
        left.Add(badges);
        left.Add(title);
        left.Add(subtitle);

        var close = new Border
        {
            WidthRequest = 36,
            HeightRequest = 36,
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            VerticalOptions = LayoutOptions.Start,
            Content = Icon(IconClose, 18, Color.FromArgb(MutedColor))
        };
        var closeTap = new Grid();
        closeTap.Add(close, 0, 0);
        var closeGesture = new TapGestureRecognizer();
        closeGesture.Tapped += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        closeTap.GestureRecognizers.Add(closeGesture);

        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12,
            Padding = new Thickness(20, 18, 20, 10)
        };
        header.Add(left, 0, 0);
        header.Add(closeTap, 1, 0);
        return header;
    }

    /// <summary>Destination banner: country tile, plan name, network, and the ready-to-flash pill.</summary>
    private View BuildPlanBanner(Plan? plan)
    {
        var countryCode = plan?.CountryCode ?? "JP";
        var countryTile = new Border
        {
            WidthRequest = 44,
            HeightRequest = 44,
            BackgroundColor = Color.FromArgb(SurfaceLowest),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            VerticalOptions = LayoutOptions.Center,
            Content = Text(countryCode, 15, Color.FromArgb(InkColor), FontAttributes.Bold)
        };
        ((Label)((Border)countryTile).Content!).HorizontalTextAlignment = TextAlignment.Center;
        ((Label)((Border)countryTile).Content!).VerticalTextAlignment = TextAlignment.Center;

        var nameRow = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        nameRow.Add(Text(plan?.Region ?? "Japan 5G Traveler", 15, Color.FromArgb(InkColor), FontAttributes.Bold));
        nameRow.Add(Icon(IconCircleCheck, 14, Color.FromArgb(PrimaryColor)));

        var networkRow = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        networkRow.Add(Text(plan?.Provider ?? "SoftBank 5G Network", 11, Color.FromArgb(MutedColor)));
        networkRow.Add(Text("•", 11, Color.FromArgb(MutedColor)));
        networkRow.Add(Text("Tier-1 Roaming", 11, Color.FromArgb(TertiaryContainer), FontAttributes.Bold));

        var planText = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        planText.Add(nameRow);
        planText.Add(networkRow);

        var identity = new HorizontalStackLayout { Spacing = 12, VerticalOptions = LayoutOptions.Center };
        identity.Add(countryTile);
        identity.Add(planText);

        var readyPill = new Border
        {
            BackgroundColor = Color.FromArgb(TertiaryFixed),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(10, 4),
            VerticalOptions = LayoutOptions.Center,
            Content = MonoText("READY TO FLASH", 10, Color.FromArgb(OnTertiaryFixed), FontAttributes.Bold)
        };

        var top = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };
        top.Add(identity, 0, 0);
        top.Add(readyPill, 1, 0);

        var metrics = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };
        metrics.Add(BuildMetricCard(IconDataUsage, "Data Allocation",
            plan?.DataLabel ?? "5 GB", plan?.AllocationNote ?? "High-Speed 5G", false), 0, 0);
        metrics.Add(BuildMetricCard(IconCalendar, "Plan Validity",
            $"{plan?.ValidityDays ?? 15} Days", ValidityCaption(plan), false), 1, 0);
        metrics.Add(BuildMetricCard(IconBolt, "Dispatch Type",
            "Instant", "Digital QR code", true), 2, 0);

        var stack = new VerticalStackLayout { Spacing = 12 };
        stack.Add(top);
        stack.Add(metrics);

        return Card(Color.FromArgb(SurfaceLow), stack, 12, new Thickness(16));
    }

    /// <summary>One of the three plan metric tiles (allocation, validity, dispatch).</summary>
    private static View BuildMetricCard(string icon, string label, string value, string detail, bool highlightValue)
    {
        var labelRow = new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };
        labelRow.Add(Icon(icon, 13, Color.FromArgb(PrimaryColor)));
        labelRow.Add(Text(label, 11, Color.FromArgb(MutedColor)));

        var valueColor = highlightValue ? Color.FromArgb(TertiaryContainer) : Color.FromArgb(InkColor);
        var stack = new VerticalStackLayout { Spacing = 1 };
        stack.Add(labelRow);
        stack.Add(Text(value, 16, valueColor, FontAttributes.Bold));
        stack.Add(MonoText(detail, 10, Color.FromArgb(MutedColor)));

        return Card(Color.FromArgb(SurfaceLowest), stack, 8, new Thickness(10));
    }

    /// <summary>Cost breakdown card with the line items and the total due.</summary>
    private View BuildCostBreakdown(Plan? plan)
    {
        var packageCost = plan?.Price ?? 8.50m;
        var tax = TaxFor(packageCost);
        var total = packageCost + tax;
        var dataLabel = plan?.DataLabel ?? "5 GB";
        var provider = plan?.Provider ?? "SoftBank 5G";

        var stack = new VerticalStackLayout { Spacing = 10 };

        // Quota bar: used versus allocated high-speed roaming data.
        var quotaLabelRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };
        quotaLabelRow.Add(Text("High-Speed Roaming Quota", 11.5, Color.FromArgb(MutedColor)), 0, 0);
        quotaLabelRow.Add(MonoText("5.120 MB  /  5.0 GB", 11, Color.FromArgb(PrimaryColor), FontAttributes.Bold), 1, 0);

        // The track is a Grid so the fill can sit at the start of the bar; a Border has
        // no StrokeShape/StrokeThickness of its own for this purpose.
        var quotaTrack = new Grid
        {
            HeightRequest = 5,
            BackgroundColor = Color.FromArgb(SurfaceHighest)
        };
        var quotaFill = new Border
        {
            WidthRequest = 36,
            BackgroundColor = Color.FromArgb(PrimaryColor),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(3) },
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill
        };
        quotaTrack.Add(quotaFill, 0, 0);

        var guaranteeRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };
        var guaranteeLeft = new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };
        guaranteeLeft.Add(Icon(IconCircleCheck, 12, Color.FromArgb(TertiaryContainer)));
        guaranteeLeft.Add(Text("Zero-throttle guarantee", 11.5, Color.FromArgb(TertiaryContainer)));
        guaranteeRow.Add(guaranteeLeft, 0, 0);
        guaranteeRow.Add(MonoText("Band n77/n78/n257", 11, Color.FromArgb(MutedColor)), 1, 0);

        stack.Add(quotaLabelRow);
        stack.Add(quotaTrack);
        stack.Add(guaranteeRow);

        stack.Add(Text("Price Summary", 14, Color.FromArgb(InkColor), FontAttributes.Bold));

        stack.Add(BuildCostRow(
            Text($"eSIM Package Cost ({provider} • {dataLabel})", 11.5, Color.FromArgb(MutedColor)),
            MonoText($"${packageCost:0.00}", 11.5, Color.FromArgb(InkColor), FontAttributes.Bold)));

        var taxLabelRow = new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };
        taxLabelRow.Add(Text("Telecom Regulatory & Roaming Tax", 11.5, Color.FromArgb(MutedColor)));
        taxLabelRow.Add(Icon(IconInfo, 13, Color.FromArgb(OutlineColor)));
        stack.Add(BuildCostRow(taxLabelRow, MonoText($"${tax:0.00}", 11.5, Color.FromArgb(InkColor), FontAttributes.Bold)));

        var deliveryLabelRow = new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };
        deliveryLabelRow.Add(Icon(IconBolt, 14, Color.FromArgb(TertiaryContainer)));
        deliveryLabelRow.Add(Text("Digital Delivery & Provisioning Fee", 11.5, Color.FromArgb(TertiaryContainer)));
        stack.Add(BuildCostRow(deliveryLabelRow, MonoText("FREE", 11.5, Color.FromArgb(TertiaryContainer), FontAttributes.Bold)));

        stack.Add(new BoxView
        {
            HeightRequest = 1,
            Color = Color.FromArgb(SurfaceHighest),
            VerticalOptions = LayoutOptions.Fill
        });

        var totalLeft = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        totalLeft.Add(Text("Total Due", 15, Color.FromArgb(InkColor), FontAttributes.Bold));
        totalLeft.Add(Text("Includes all local Japanese telecom charges", 10.5, Color.FromArgb(MutedColor)));

        var totalValue = Text($"${total:0.00}", 26, Color.FromArgb(PrimaryColor), FontAttributes.Bold);
        totalValue.VerticalOptions = LayoutOptions.End;
        var currency = MonoText("USD", 11, Color.FromArgb(MutedColor), FontAttributes.Bold);
        currency.VerticalOptions = LayoutOptions.End;
        currency.Margin = new Thickness(0, 0, 0, 3);

        var totalRight = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        totalRight.Add(totalValue);
        totalRight.Add(currency);

        var totalRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };
        totalRow.Add(totalLeft, 0, 0);
        totalRow.Add(totalRight, 1, 0);

        stack.Add(totalRow);

        return Card(Color.FromArgb(SurfaceLowest), stack, 12, new Thickness(16));
    }

    /// <summary>A label/value pair inside the cost breakdown.</summary>
    private static View BuildCostRow(View label, View value)
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };
        label.VerticalOptions = LayoutOptions.Center;
        value.VerticalOptions = LayoutOptions.Center;
        row.Add(label, 0, 0);
        row.Add(value, 1, 0);
        return row;
    }

    /// <summary>Payment method card with the demo card and the change-method action.</summary>
    private View BuildPaymentMethod()
    {
        var headerRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8
        };

        var titleRow = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        titleRow.Add(Icon(IconCreditCard, 16, Color.FromArgb(PrimaryColor)));
        titleRow.Add(Text("Payment Method", 12.5, Color.FromArgb(InkColor), FontAttributes.Bold));
        headerRow.Add(titleRow, 0, 0);

        var change = Text("Change Method", 10.5, Color.FromArgb(PrimaryColor), FontAttributes.Bold);
        change.VerticalOptions = LayoutOptions.Center;
        var changeTap = new Grid();
        changeTap.Add(change, 0, 0);
        var changeGesture = new TapGestureRecognizer();
        changeGesture.Tapped += (_, _) => ChangePaymentRequested?.Invoke(this, EventArgs.Empty);
        changeTap.GestureRecognizers.Add(changeGesture);
        headerRow.Add(changeTap, 1, 0);

        var visa = new Border
        {
            WidthRequest = 40,
            HeightRequest = 28,
            BackgroundColor = Color.FromArgb(InkColor),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
            VerticalOptions = LayoutOptions.Center,
            Content = MonoText("VISA", 10, Colors.White, FontAttributes.Bold)
        };
        var visaLabel = (Label)((Border)visa).Content!;
        visaLabel.HorizontalTextAlignment = TextAlignment.Center;
        visaLabel.VerticalTextAlignment = TextAlignment.Center;

        var cardText = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        cardText.Add(Text("Apple Pay / Visa", 12, Color.FromArgb(InkColor), FontAttributes.Bold));
        cardText.Add(MonoText("Ending in •••• 4242", 10.5, Color.FromArgb(MutedColor)));

        var cardLeft = new HorizontalStackLayout { Spacing = 12, VerticalOptions = LayoutOptions.Center };
        cardLeft.Add(visa);
        cardLeft.Add(cardText);

        var defaultDot = new Ellipse
        {
            WidthRequest = 8,
            HeightRequest = 8,
            Fill = Color.FromArgb(TertiaryContainer),
            VerticalOptions = LayoutOptions.Center
        };
        var defaultText = Text("Default", 10.5, Color.FromArgb(TertiaryContainer), FontAttributes.Bold);
        defaultText.VerticalOptions = LayoutOptions.Center;
        var defaultRow = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        defaultRow.Add(defaultDot);
        defaultRow.Add(defaultText);

        var cardRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };
        cardRow.Add(cardLeft, 0, 0);
        cardRow.Add(defaultRow, 1, 0);

        var stack = new VerticalStackLayout { Spacing = 10 };
        stack.Add(headerRow);
        stack.Add(Card(Color.FromArgb(SurfaceLowest), cardRow, 8, new Thickness(12)));

        return Card(Color.FromArgb(SurfaceLow), stack, 12, new Thickness(14));
    }

    /// <summary>Three trust badges: SSL encryption, compatibility shield, and no hidden fees.</summary>
    private static View BuildTrustBadges()
    {
        var grid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };

        grid.Add(BuildTrustBadge(IconShieldCheck, "256-Bit SSL", "Encrypted gateway", Color.FromArgb(PrimaryColor)), 0, 0);
        grid.Add(BuildTrustBadge(IconShieldCheck, "Compatibility Shield", "100% money-back", Color.FromArgb(TertiaryContainer)), 1, 0);
        grid.Add(BuildTrustBadge(IconCircleCheck, "Zero Roam Shock", "No hidden fees", Color.FromArgb(Secondary)), 2, 0);

        return grid;
    }

    /// <summary>A single centered trust badge with icon, title, and caption.</summary>
    private static View BuildTrustBadge(string icon, string title, string caption, Color iconColor)
    {
        var titleLabel = Text(title, 10.5, Color.FromArgb(InkColor), FontAttributes.Bold);
        titleLabel.HorizontalTextAlignment = TextAlignment.Center;
        var captionLabel = MonoText(caption, 9.5, Color.FromArgb(MutedColor));
        captionLabel.HorizontalTextAlignment = TextAlignment.Center;

        var stack = new VerticalStackLayout { Spacing = 3, HorizontalOptions = LayoutOptions.Center };
        stack.Add(Icon(icon, 18, iconColor));
        stack.Add(titleLabel);
        stack.Add(captionLabel);
        return stack;
    }

    /// <summary>Footer with the cancel/confirm buttons and the delivery note.</summary>
    private View BuildFooter(Plan? plan)
    {
        var packageCost = plan?.Price ?? 8.50m;
        var total = packageCost + TaxFor(packageCost);

        var cancel = new SfButton
        {
            Text = "Cancel",
            FontFamily = UiFont,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            Background = Color.FromArgb(SurfaceLow),
            TextColor = Color.FromArgb(InkColor),
            CornerRadius = 12,
            HeightRequest = 46,
            VerticalOptions = LayoutOptions.Fill
        };
        cancel.Clicked += (_, _) => CancelRequested?.Invoke(this, EventArgs.Empty);

        var confirmText = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        confirmText.Add(Icon(IconLock, 15, Colors.White));
        confirmText.Add(Text($"Confirm Purchase • ${total:0.00}", 14, Colors.White, FontAttributes.Bold));

        var confirm = new SfButton
        {
            Background = Color.FromArgb(PrimaryColor),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 46,
            VerticalOptions = LayoutOptions.Fill,
            Content = new DataTemplate(() => confirmText)
        };
        confirm.Clicked += (_, _) => PurchaseRequested?.Invoke(this, EventArgs.Empty);

        var buttons = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(2, GridUnitType.Star))
            }
        };
        buttons.Add(cancel, 0, 0);
        buttons.Add(confirm, 1, 0);

        var note = MonoText("Instant delivery to your dashboard. Scan QR or push directly via iOS & Android settings.", 10.5, Color.FromArgb(MutedColor));
        note.HorizontalTextAlignment = TextAlignment.Center;

        var stack = new VerticalStackLayout { Spacing = 12, Padding = new Thickness(20, 14, 20, 18) };
        stack.Add(buttons);
        stack.Add(note);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLowest),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0) },
            Content = stack
        };
    }
    /// <summary>Raised when the sheet content resizes, so the popup can be re-anchored.</summary>
    public event EventHandler? SheetResized;

    private void OnRootSized()
    {
        ApplyViewport();
        SheetResized?.Invoke(this, EventArgs.Empty);
    }

}
