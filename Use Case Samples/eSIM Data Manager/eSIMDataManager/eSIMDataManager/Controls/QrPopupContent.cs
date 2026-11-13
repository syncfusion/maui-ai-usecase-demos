using eSIMDataManager.Models;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Barcode;
using Syncfusion.Maui.Buttons;

namespace eSIMDataManager.Controls;

/// <summary>
/// Builds the Install &amp; Activate eSIM dialog content that matches the reference design:
/// a QR scan panel on the left, activation steps and manual parameters on the right, and a footer
/// bar with ICCID + email + scanned actions. The body scrolls when content overflows the dialog.
/// </summary>
public sealed class QrPopupContent
{
    private const string InkColor = "#0B1C30";
    private const string MutedColor = "#434656";
    private const string PrimaryColor = "#0046CC";
    private const string PrimaryActionColor = "#0D5CFF";
    private const string SurfaceLow = "#EFF4FF";
    private const string OutlineVariant = "#C3C5D9";
    private const string UiFont = "Inter";
    private const string MonoFont = "monospace";

    // Material icon codepoints available in the bundled MauiMaterialAssets font.
    private const string IconTable = "\uE712";
    private const string IconClose = "\uE70B";
    private const string IconDot = "\uE73F";
    private const string IconCheckTick = "\uE70C";
    private const string IconCheckLarge = "\uE75B";
    private const string IconSearch = "\uE715";
    private const string IconCopy = "\uE7A0";
    private const string IconEmail = "\uE717";
    private const string IconCircleCheck = "\uE78C";
    private const string IconUser = "\uE71B";
    private const string IconPhone = "\uE718";

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

    private readonly VerticalStackLayout root;
    private readonly Grid bodyGrid;
    private readonly VerticalStackLayout compactStack;
    private readonly ScrollView bodyScroll;
    private readonly View scanCardHost;
    private readonly View stepsHost;
    private bool isCompact;
    private double availableHeight;

    public QrPopupContent(EsimProfile? profile)
    {
        var activationCode = profile?.ActivationCode ?? "LPA:1$rsp.global-esim.net$SAMPLE";

        root = new VerticalStackLayout { Spacing = 0, BackgroundColor = Colors.White };
        root.Resources.Add(PopupLabelStyle);
        root.SizeChanged += RootOnSizeChanged;

        root.Add(BuildAccentBar());
        root.Add(BuildHeader(profile));
        root.Add(BuildProfileBanner(profile));

        scanCardHost = BuildScanCard(profile, activationCode);
        stepsHost = BuildSteps(profile, activationCode);

        compactStack = new VerticalStackLayout { Spacing = 12 };
        compactStack.Add(scanCardHost);
        compactStack.Add(stepsHost);

        bodyGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 12,
            Padding = new Thickness(14, 0, 14, 12)
        };
        bodyGrid.Add(scanCardHost, 0, 0);
        bodyGrid.Add(stepsHost, 1, 0);

        bodyScroll = new ScrollView
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = bodyGrid
        };

        root.Add(bodyGrid);
        root.Add(BuildFooter(profile, activationCode));
    }

    public View Content => root;

    /// <summary>Raised when the user confirms the scan in the demo flow.</summary>
    public event EventHandler? ScannedRequested;

    /// <summary>Raised when the user taps the close button.</summary>
    public event EventHandler? CloseRequested;

    /// <summary>Switches between the side-by-side layout and the stacked sheet layout.</summary>
    public void SetCompact(bool compact)
    {
        if (isCompact == compact) return;
        isCompact = compact;

        bodyGrid.Children.Clear();
        if (compact)
            bodyGrid.Children.Add(compactStack);
        else
        {
            bodyGrid.Add(scanCardHost, 0, 0);
            bodyGrid.Add(stepsHost, 1, 0);
        }
    }

    /// <summary>Caps the scrollable body so the footer stays on screen.</summary>
    public void SetViewport(double availableHeight)
    {
        this.availableHeight = availableHeight;
    }

    private void RootOnSizeChanged(object? sender, EventArgs e)
    {
        if (availableHeight <= 0) return;
        var chrome = root.Height - bodyScroll.Height;
        if (chrome <= 0) return;
        var limit = Math.Max(200, availableHeight - 24 - chrome);
        if (Math.Abs(bodyScroll.MaximumHeightRequest - limit) > 0.5)
            bodyScroll.MaximumHeightRequest = limit;
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
    private static Label MonoText(string text, double size, Color color, bool truncate = true)
    {
        var label = new Label
        {
            Text = text,
            FontFamily = MonoFont,
            FontSize = size,
            LineBreakMode = truncate ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap
        };
        label.SetAppThemeColor(Label.TextColorProperty, color, color);
        return label;
    }

    /// <summary>Material icon label (must use MaterialAssets font).</summary>
    private static Label Icon(string glyph, double size, Color color, string fontFamily)
    {
        var label = new Label
        {
            Text = glyph,
            FontFamily = fontFamily,
            FontSize = size,
            TextColor = color,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        label.SetAppThemeColor(Label.TextColorProperty, color, color);
        return label;
    }

    // -- sections --------------------------------------------------------

    private static View BuildAccentBar() => new BoxView
    {
        HeightRequest = 4,
        Color = Color.FromArgb(PrimaryActionColor),
        HorizontalOptions = LayoutOptions.Fill
    };

    private View BuildHeader(EsimProfile? profile)
    {
        // 40px square icon tile
        var iconTile = new Border
        {
            WidthRequest = 40,
            HeightRequest = 40,
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Content = new Grid
            {
                Children =
                {
                    Icon(IconTable, 22, Color.FromArgb(PrimaryColor), "UiKitIcons")
                }
            }
        };

        // Title + subtitle stacked, title centered vertically with the icon and badge
        var titles = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        titles.Add(Text("Install & Activate eSIM", 19, Color.FromArgb(InkColor), FontAttributes.Bold));
        titles.Add(Text("Quick setup configuration for instant cross-border high-speed routing", 10.5, Color.FromArgb(MutedColor)));

        // "Ready to Install" green pill
        var readyPill = new Border
        {
            BackgroundColor = Color.FromArgb("#ECFDF5"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(10, 4),
            VerticalOptions = LayoutOptions.Center,
            Content = new Grid
            {
                ColumnSpacing = 5,
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto)
                }
            }
        };
        var readyIcon = Icon(IconCheckTick, 10, Color.FromArgb("#065F46"), "MaterialAssets");
        var readyText = Text("Ready to Install", 10, Color.FromArgb("#065F46"), FontAttributes.Bold);
        ((Grid)readyPill.Content).Add(readyIcon, 0, 0);
        ((Grid)readyPill.Content).Add(readyText, 1, 0);

        // Close button
        var close = new SfButton
        {
            Text = IconClose,
            FontFamily = "MaterialAssets",
            FontSize = 16,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(MutedColor),
            WidthRequest = 32,
            HeightRequest = 32,
            CornerRadius = 8,
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };
        close.Clicked += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        // icon | titles | pill | star (close) — pill stays next to the title on wide layouts
        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),  // 0 icon
                new ColumnDefinition(GridLength.Star),  // 1 titles
                new ColumnDefinition(GridLength.Auto),  // 2 pill
                new ColumnDefinition(GridLength.Auto)   // 3 close
            },
            ColumnSpacing = 12,
            Padding = new Thickness(16, 12, 12, 8)
        };
        header.Add(iconTile, 0, 0);
        header.Add(titles, 1, 0);
        header.Add(readyPill, 2, 0);
        header.Add(close, 3, 0);
        return header;
    }

    private static View BuildProfileBanner(EsimProfile? profile)
    {
        var country = profile?.Country ?? "Japan";
        var countryCode = country.Equals("Japan", StringComparison.OrdinalIgnoreCase) ? "JP" : country[..2].ToUpperInvariant();
        var network = profile?.Network == NetworkType.FiveG ? "5G" : "4G / LTE";

        // Row 1: JP  ·  Japan 5G Explorer  ·  ●  SoftBank Crop  (5G Multi-IMSI)
        var titleRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };
        titleRow.Add(Text(countryCode, 14, Color.FromArgb(InkColor), FontAttributes.Bold), 0, 0);
        titleRow.Add(Text(profile?.ProfileName ?? "Japan 5G Explorer", 14, Color.FromArgb(InkColor), FontAttributes.Bold), 1, 0);
        titleRow.Add(Icon(IconDot, 6, Color.FromArgb(OutlineVariant), "MaterialAssets"), 2, 0);
        titleRow.Add(Text($"{profile?.Carrier ?? "SoftBank Corp."}", 11, Color.FromArgb(MutedColor)), 3, 0);
        titleRow.Add(Text($"({network} Multi-IMSI)", 11, Color.FromArgb(MutedColor)), 4, 0);
        titleRow.Add(new Label(), 5, 0);

        // Row 2: 10 GB · 30 Days Global Tier pill (white, outlined, on light-blue background)
        var tierDot = Icon(IconCheckTick, 11, Color.FromArgb("#006172"), "MaterialAssets");
        var tierText = Text($"{profile?.AllocatedGb ?? 10:0.#} GB  ·  {Math.Max(1, profile?.DaysRemaining ?? 30)} Days Global Tier",
            11, Color.FromArgb("#006172"), FontAttributes.Bold);
        var tierRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 5,
            VerticalOptions = LayoutOptions.Center
        };
        tierRow.Add(tierDot, 0, 0);
        tierRow.Add(tierText, 1, 0);

        var tier = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(9, 4),
            HorizontalOptions = LayoutOptions.Start,
            Content = tierRow
        };

        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Add(titleRow);
        stack.Add(tier);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(14, 12),
            Margin = new Thickness(14, 0, 14, 12),
            Content = stack
        };
    }

    private static View BuildScanCard(EsimProfile? profile, string activationCode)
    {
        // Heading: 🔍 Instant Scan (left) ............ EID: 890490832... (right)
        var heading = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };
        var instantRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 6,
            VerticalOptions = LayoutOptions.Center
        };
        instantRow.Add(Icon(IconSearch, 12, Color.FromArgb(PrimaryColor), "MaterialAssets"), 0, 0);
        instantRow.Add(Text("Instant Scan", 13, Color.FromArgb(InkColor), FontAttributes.Bold), 1, 0);
        heading.Add(instantRow, 0, 0);
        heading.Add(MonoText($"EID: {TruncateEid(profile?.Iccid)}", 10, Color.FromArgb(MutedColor)), 1, 0);

        // QR code rendered in primary blue on a white quiet zone
        var barcode = new SfBarcodeGenerator
        {
            ShowText = false,
            HeightRequest = 220,
            WidthRequest = 220,
            BackgroundColor = Colors.White,
            ForegroundColor = Colors.Black,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Symbology = new QRCode(),
            Value = activationCode
        };

        // Small white circle with primary-blue check-tick glyph, centered on the QR
        var checkBadge = new Border
        {
            WidthRequest = 30,
            HeightRequest = 30,
            BackgroundColor = Color.FromArgb(PrimaryColor),
            Stroke = Colors.White,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(15) },
            Content = Icon(IconCheckTick, 13, Colors.White, "MaterialAssets")
        };

        var qrStack = new Grid
        {
            WidthRequest = 220,
            HeightRequest = 220,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        qrStack.Add(barcode, 0, 0);
        qrStack.Add(checkBadge, 0, 0);
        qrStack.HorizontalOptions = LayoutOptions.Center;
        qrStack.VerticalOptions = LayoutOptions.Center;

        // Four L-shaped corner brackets framing the QR code in primary blue.
        // A 3x3 grid places each bracket in its own corner cell so the positions are deterministic.
        var qrFrame = new Grid
        {
            WidthRequest = 260,
            HeightRequest = 260,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        qrFrame.Add(qrStack, 1, 1);
        qrFrame.Add(BuildCornerBracket(Corner.TopLeft), 0, 0);
        qrFrame.Add(BuildCornerBracket(Corner.TopRight), 2, 0);
        qrFrame.Add(BuildCornerBracket(Corner.BottomLeft), 0, 2);
        qrFrame.Add(BuildCornerBracket(Corner.BottomRight), 2, 2);
        var stack = new VerticalStackLayout { Spacing = 14 };
        stack.Add(heading);
        stack.Add(qrFrame);

        // Caption block: "Scan this QR code with your device camera or" (muted)
        //                "in Settings > Cellular > Add eSIM." (primary, bold)
        //                "●  Single-use cryptographic profile token" (muted, small)
        var caption1 = Text("Scan this QR code with your device camera or", 11, Color.FromArgb(InkColor));
        caption1.HorizontalTextAlignment = TextAlignment.Center;
        var caption2 = Text("in Settings > Cellular > Add eSIM.", 11, Color.FromArgb(PrimaryColor), FontAttributes.Bold);
        caption2.HorizontalTextAlignment = TextAlignment.Center;
        var caption3Stack = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 6,
            HorizontalOptions = LayoutOptions.Center
        };
        var dot = new Ellipse
        {
            WidthRequest = 7,
            HeightRequest = 7,
            Fill = Color.FromArgb("#006141"),
            VerticalOptions = LayoutOptions.Center
        };
        caption3Stack.Add(dot, 0, 0);
        var caption3 = Text("Single-use cryptographic profile token", 10, Color.FromArgb(MutedColor));
        caption3.HorizontalOptions = LayoutOptions.Start;
        caption3Stack.Add(caption3, 1, 0);
        stack.Add(caption1);
        stack.Add(caption2);
        stack.Add(caption3Stack);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(14, 14),
            Content = stack
        };
    }

    private enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

    /// <summary>L-shaped bracket composed of two short blue rectangles framing one corner.</summary>
    private static View BuildCornerBracket(Corner corner)
    {
        const double arm = 18;   // length of each leg
        const double thick = 3;  // stroke thickness of the leg

        var horizontal = new BoxView
        {
            HeightRequest = thick,
            WidthRequest = arm,
            BackgroundColor = Color.FromArgb(PrimaryColor)
        };
        var vertical = new BoxView
        {
            WidthRequest = thick,
            HeightRequest = arm,
            BackgroundColor = Color.FromArgb(PrimaryColor)
        };

        // The two legs share one cell; alignment decides which corner of the cell they hug.
        var grid = new Grid
        {
            WidthRequest = arm,
            HeightRequest = arm
        };

        switch (corner)
        {
            case Corner.TopLeft:
                grid.HorizontalOptions = LayoutOptions.Start;
                grid.VerticalOptions = LayoutOptions.Start;
                horizontal.HorizontalOptions = LayoutOptions.Start;
                horizontal.VerticalOptions = LayoutOptions.Start;
                vertical.HorizontalOptions = LayoutOptions.Start;
                vertical.VerticalOptions = LayoutOptions.Start;
                break;
            case Corner.TopRight:
                grid.HorizontalOptions = LayoutOptions.End;
                grid.VerticalOptions = LayoutOptions.Start;
                horizontal.HorizontalOptions = LayoutOptions.End;
                horizontal.VerticalOptions = LayoutOptions.Start;
                vertical.HorizontalOptions = LayoutOptions.End;
                vertical.VerticalOptions = LayoutOptions.Start;
                break;
            case Corner.BottomLeft:
                grid.HorizontalOptions = LayoutOptions.Start;
                grid.VerticalOptions = LayoutOptions.End;
                horizontal.HorizontalOptions = LayoutOptions.Start;
                horizontal.VerticalOptions = LayoutOptions.End;
                vertical.HorizontalOptions = LayoutOptions.Start;
                vertical.VerticalOptions = LayoutOptions.End;
                break;
            case Corner.BottomRight:
                grid.HorizontalOptions = LayoutOptions.End;
                grid.VerticalOptions = LayoutOptions.End;
                horizontal.HorizontalOptions = LayoutOptions.End;
                horizontal.VerticalOptions = LayoutOptions.End;
                vertical.HorizontalOptions = LayoutOptions.End;
                vertical.VerticalOptions = LayoutOptions.End;
                break;
        }

        grid.Add(horizontal, 0, 0);
        grid.Add(vertical, 0, 0);
        return grid;
    }

    private static View BuildSteps(EsimProfile? profile, string activationCode)
    {
        var outer = new VerticalStackLayout { Spacing = 10 };

        outer.Add(Text("ACTIVATION STEPS", 11, Color.FromArgb(MutedColor), FontAttributes.Bold));

        outer.Add(ActivationStep("1", "Scan QR Code in Device Settings",
            "Navigate to Settings > Cellular (iOS) or SIM Manager (Android) and add plan."));
        outer.Add(ActivationStep("2", "Enable 'Data Roaming' upon Landing",
            "Turn on roaming toggles specifically for this profile once in Japan."));
        outer.Add(ActivationStep("3", "Set Profile as Cellular Data Carrier",
            "Keep your primary home SIM for SMS 2FA while using this eSIM for all data."));

        outer.Add(BuildManualActivation(activationCode));
        outer.Add(BuildCompatibility());

        return outer;
    }

    private static View ActivationStep(string number, string title, string description)
    {
        // Light-blue surface card, number badge left, text right
        var badge = new Border
        {
            WidthRequest = 28,
            HeightRequest = 28,
            BackgroundColor = Color.FromArgb(PrimaryColor),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Content = Text(number, 12, Colors.White, FontAttributes.Bold)
                .Also(l =>
                {
                    l.HorizontalTextAlignment = TextAlignment.Center;
                    l.VerticalTextAlignment = TextAlignment.Center;
                })
        };

        var text = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        text.Add(Text(title, 12, Color.FromArgb(InkColor), FontAttributes.Bold));
        text.Add(Text(description, 10.5, Color.FromArgb(MutedColor)));

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 12,
            VerticalOptions = LayoutOptions.Center
        };
        row.Add(badge, 0, 0);
        row.Add(text, 1, 0);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(12),
            Content = row
        };
    }

    private static View BuildManualActivation(string activationCode)
    {
        // Header row: [table icon] Manual Activation Parameters (2 lines) ........ RSP Protocol v2.2 pill
        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };

        var titleStack = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        titleStack.Add(Text("Manual Activation", 13, Color.FromArgb(InkColor), FontAttributes.Bold));
        titleStack.Add(Text("Parameters", 13, Color.FromArgb(InkColor), FontAttributes.Bold));
        headerGrid.Add(Icon(IconTable, 16, Color.FromArgb(InkColor), "MaterialAssets"), 0, 0);
        headerGrid.Add(titleStack, 1, 0);

        var rspPill = new Border
        {
            BackgroundColor = Color.FromArgb("#ECFEFF"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Padding = new Thickness(8, 5),
            VerticalOptions = LayoutOptions.Center,
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 0
            }
        };
        var rspText = Text("RSP Protocol\nv2.2", 10, Color.FromArgb("#006172"));
        rspText.HorizontalTextAlignment = TextAlignment.End;
        ((Grid)rspPill.Content).Add(rspText, 0, 0);
        headerGrid.Add(rspPill, 2, 0);

        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Add(headerGrid);

        stack.Add(ActivationParameter("SM-DP+ Address", "rsp.global-esim.net", Color.FromArgb(InkColor)));
        stack.Add(ActivationParameter("Activation Code (LPA String)", activationCode, Color.FromArgb(PrimaryColor)));

        stack.Add(CopyButton());

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 0.1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(14, 12),
            Content = stack
        };
    }

    private static View ActivationParameter(string label, string value, Color valueColor)
    {
        var valueRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };
        valueRow.Add(MonoText(value, 10.5, valueColor, truncate: false), 0, 0);
        valueRow.Add(CopyIconButton(), 1, 0);

        var stack = new VerticalStackLayout { Spacing = 3 };
        stack.Add(Text(label, 10, Color.FromArgb(MutedColor)));
        var inner = new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(7) },
            Padding = new Thickness(10, 7),
            Content = valueRow
        };
        stack.Add(inner);
        return stack;
    }

    private static View CopyIconButton()
    {
        var copy = new SfButton
        {
            Text = IconCopy,
            FontFamily = "MaterialAssets",
            FontSize = 12,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(MutedColor),
            WidthRequest = 24,
            HeightRequest = 24,
            Padding = new Thickness(0),
            CornerRadius = 6
        };
        copy.SetAppThemeColor(SfButton.TextColorProperty, Color.FromArgb(MutedColor), Color.FromArgb(MutedColor));
        return copy;
    }

    private static View CopyButton()
    {
        var copyIcon = Icon(IconCopy, 12, Color.FromArgb(PrimaryColor), "MaterialAssets");
        var copyText = Text("Copy Full Activation String", 11, Color.FromArgb(PrimaryColor), FontAttributes.Bold);

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 6,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        row.Add(copyIcon, 0, 0);
        row.Add(copyText, 1, 0);

        var border = new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Padding = new Thickness(12, 8),
            HorizontalOptions = LayoutOptions.Fill,
            Content = row
        };
        var tap = new TapGestureRecognizer();
        // No handler wired — this is a visual demo only.
        border.GestureRecognizers.Add(tap);
        return border;
    }

    private static View BuildCompatibility()
    {
        // White card with subtle outline, user icon + multi-line text
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10,
            VerticalOptions = LayoutOptions.Center
        };
        row.Add(Icon(IconPhone, 14, Color.FromArgb(MutedColor), "MaterialAssets"), 0, 0);

        var text = new Label
        {
            Text = "Compatibility: Unlocked iPhone XS or newer, Pixel 3 or later, Galaxy S20+ and compatible dual-SIM devices.",
            FontFamily = UiFont,
            FontSize = 10.5,
            LineBreakMode = LineBreakMode.WordWrap
        };
        text.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb(MutedColor), Color.FromArgb(MutedColor));
        row.Add(text, 1, 0);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 0.1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(12),
            Content = row
        };
    }

    private View BuildFooter(EsimProfile? profile, string activationCode)
    {
        // ICCID: check icon + label + formatted groups
        var iccidText = MonoText(FormatIccid(profile?.Iccid ?? "890490832"), 10.5, Color.FromArgb(MutedColor));
        iccidText.HorizontalOptions = LayoutOptions.Start;
        iccidText.VerticalOptions = LayoutOptions.Center;

        var iccidIcon = Icon(IconCircleCheck, 14, Color.FromArgb("#007A56"), "MaterialAssets");
        iccidIcon.VerticalOptions = LayoutOptions.Center;

        var iccidLabel = Text("ICCID:", 10.5, Color.FromArgb(MutedColor), FontAttributes.Bold);
        iccidLabel.VerticalOptions = LayoutOptions.Center;

        var iccidRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 6,
            VerticalOptions = LayoutOptions.Center
        };
        iccidRow.Add(iccidIcon, 0, 0);
        iccidRow.Add(iccidLabel, 1, 0);
        iccidRow.Add(iccidText, 2, 0);

        // Send QR to Email — ghost button (white, outlined)
        var emailRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 6,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        emailRow.Add(Icon(IconEmail, 13, Color.FromArgb(InkColor), "MaterialAssets"), 0, 0);
        emailRow.Add(Text("Send QR to Email", 12, Color.FromArgb(InkColor)), 1, 0);

        var sendEmail = new SfButton
        {
            Background = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            CornerRadius = 8,
            Padding = new Thickness(14, 8),
            Content = new DataTemplate(() => emailRow)
        };

        // I've Scanned the QR Code — primary, white text
        var scannedRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 6,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        scannedRow.Add(Icon(IconCircleCheck, 13, Colors.White, "MaterialAssets"), 0, 0);
        scannedRow.Add(Text("I've Scanned the QR Code", 12, Colors.White, FontAttributes.Bold), 1, 0);

        var scanned = new SfButton
        {
            Background = Color.FromArgb(PrimaryColor),
            Stroke = Color.FromArgb("#007A56"),
            StrokeThickness = 1,
            CornerRadius = 8,
            Padding = new Thickness(16, 8),
            Content = new DataTemplate(() => scannedRow)
        };

        // Footer has NO background fill — just border-top separator
        var footer = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12,
            Padding = new Thickness(16, 14, 16, 16)
        };
        footer.Add(iccidRow, 0, 0);
        footer.Add(sendEmail, 1, 0);
        footer.Add(scanned, 2, 0);

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Stroke = Color.FromArgb(OutlineVariant),
            Content = footer
        };
    }

    // -- utilities -------------------------------------------------------

    private static string TruncateEid(string? iccid)
    {
        if (string.IsNullOrEmpty(iccid)) return "DEMO-8931-02...";
        return iccid.Length > 12 ? iccid.Substring(0, 12) + "..." : iccid;
    }

    private static string FormatIccid(string iccid)
    {
        var digits = new string(iccid.Where(char.IsLetterOrDigit).ToArray());
        if (digits.Length <= 4) return digits;
        return string.Join(' ', Enumerable.Range(0, (digits.Length + 3) / 4)
            .Select(i => digits.Substring(i * 4, Math.Min(4, digits.Length - i * 4))));
    }
}

internal static class LabelExtensions
{
    /// <summary>Fluent helper to chain configuration on a Label.</summary>
    public static T Also<T>(this T obj, Action<T> configure) where T : Label
    {
        configure(obj);
        return obj;
    }
}
