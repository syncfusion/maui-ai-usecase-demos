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
    private const string IconShare = "\uE80D";
    private const string IconArrowLeft = "\uE72D";   // MaterialAssets "arrow-left"
    private const string IconChevronDown = "\uE706";  // MaterialAssets "chevron-down"

    /// <summary>Fallback width used before the sheet has been measured.</summary>
    private const double DefaultWidth = 390;

    /// <summary>Strip kept free above a bottom sheet so the rounded corners stay visible.</summary>
    private const double SheetTopInset = 36;

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
    private readonly Grid bodyGrid;
    private readonly VerticalStackLayout compactStack;
    private readonly ScrollView bodyScroll;
    private readonly View scanCardHost;
    private readonly View stepsHost;
    private readonly View accentBar;
    private readonly VerticalStackLayout chromeView;
    private readonly VerticalStackLayout headerHost;
    private readonly VerticalStackLayout bannerHost;
    private readonly VerticalStackLayout footerHost;
    private readonly EsimProfile? profile;
    private readonly string activationCode;
    private bool isCompact;
    private bool isSheet;
    private double availableHeight;

    public QrPopupContent(EsimProfile? profile)
    {
        this.profile = profile;
        activationCode = profile?.ActivationCode ?? "LPA:1$rsp.global-esim.net$SAMPLE";

        // Three rows: chrome (auto), body (star) and footer (auto). The chrome and the
        // footer live outside the scroll view, so they stay pinned to the top and to the
        // bottom of the sheet while the body scrolls in the space between them.
        root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            BackgroundColor = Colors.White,
        };
        root.Resources.Add(PopupLabelStyle);
        root.SizeChanged += (_, _) => OnRootSized();

        accentBar = BuildAccentBar();
        chromeView = new VerticalStackLayout { Spacing = 0 };
        chromeView.Add(accentBar);
        chromeView.Add(BottomSheetChrome.BuildHandle());
        headerHost = new VerticalStackLayout { Spacing = 0 };
        headerHost.Add(BuildHeader(profile));
        chromeView.Add(headerHost);
        bannerHost = new VerticalStackLayout { Spacing = 0 };
        bannerHost.Add(BuildProfileBanner(profile));
        chromeView.Add(bannerHost);
        // Grid.Add(view, column, row): the chrome, body and footer are rows, not columns.
        root.Add(chromeView, 0, 0);

        scanCardHost = BuildScanCard(profile, activationCode);
        stepsHost = BuildSteps(profile, activationCode);

        // The two hosts are built once and then moved between the stacked and the
        // side-by-side arrangement. A view can only have one parent, so the host that
        // is not in use is detached before the other one is filled.
        compactStack = new VerticalStackLayout { Spacing = 12 };

        // The grid only splits into two columns on the wide desktop dialog. On the phone the
        // hosts are stacked in a single full-width column.
        bodyGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 12,
            Padding = new Thickness(14, 0, 14, 12)
        };

        bodyScroll = new ScrollView
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = bodyGrid
        };

        root.Add(bodyScroll, 0, 1);
        footerHost = new VerticalStackLayout { Spacing = 0 };
        footerHost.Add(BuildFooter(profile, activationCode));
        root.Add(footerHost, 0, 2);

        ApplyLayout(isCompact);
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
        ApplyLayout(compact);
    }

    /// <summary>
    /// Moves the scan card and the steps into the requested arrangement. Each view is
    /// detached from its current parent before it is added to the new one, because a
    /// native view cannot have two parents.
    /// </summary>
    private void ApplyLayout(bool compact)
    {
        bodyGrid.Children.Clear();
        compactStack.Children.Clear();

        if (compact)
        {
            // One full-width column on the phone so the stacked content is not squeezed
            // into half of the sheet. The sheet body is built to match the phone reference.
            bodyGrid.ColumnDefinitions.Clear();
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            compactStack.Children.Clear();
            compactStack.Add(BuildSheetBody(profile, activationCode));
            bodyGrid.Children.Add(compactStack);
            return;
        }

        bodyGrid.ColumnDefinitions.Clear();
        bodyGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        bodyGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        bodyGrid.Add(scanCardHost, 0, 0);
        bodyGrid.Add(stepsHost, 1, 0);
    }

    /// <summary>Switches the dialog into the mobile bottom-sheet presentation.</summary>
    public void SetSheet(bool sheet)
    {
        if (isSheet == sheet) return;
        isSheet = sheet;
        accentBar.IsVisible = !sheet;
        if (sheet) SetCompact(true);
        RebuildChrome();
    }

    /// <summary>Rebuilds the header, banner and footer for the current sheet arrangement.</summary>
    private void RebuildChrome()
    {
        headerHost.Children.Clear();
        headerHost.Add(BuildHeader(profile));
        bannerHost.Children.Clear();
        bannerHost.Add(BuildProfileBanner(profile));
        footerHost.Children.Clear();
        footerHost.Add(BuildFooter(profile, activationCode));
    }

    /// <summary>Used by the popup host to tell the content about the available screen height.</summary>
    public void SetViewport(double availableHeight)
    {
        this.availableHeight = availableHeight;
        ApplyViewport();
    }

    /// <summary>The sheet height the content currently occupies, in device independent units.</summary>
    public double SheetHeight => currentSheetHeight;

    private double currentSheetHeight;

    private void RootOnSizeChanged(object? sender, EventArgs e) => OnRootSized();

    /// <summary>
    /// Sizes the sheet so the chrome stays pinned to the top and the footer stays pinned to
    /// the bottom, while the body scrolls in between. When the whole sheet fits on screen
    /// it hugs its content and nothing scrolls. When it does not fit, the sheet is capped to
    /// the viewport and only the body scrolls.
    /// </summary>
    public void ApplyViewport()
    {
        if (availableHeight <= 0) return;

        var width = root.Width > 0 ? root.Width : DefaultWidth;

        // Measure the chrome and the footer at their natural height. The measured value is
        // used instead of View.Height, because Height is -1 until the view is arranged.
        var chromeHeight = chromeView.Measure(width, double.PositiveInfinity).Height;
        if (chromeHeight <= 0 && chromeView.Height > 0) chromeHeight = chromeView.Height;

        var footerHeight = footerHost.Measure(width, double.PositiveInfinity).Height;
        if (footerHeight <= 0 && footerHost.Height > 0) footerHeight = footerHost.Height;

        // Natural height of the body content, measured without a height constraint.
        var body = bodyScroll.Content;
        var naturalBody = body is null ? 0 : body.Measure(width, double.PositiveInfinity).Height;
        var totalNatural = chromeHeight + naturalBody + footerHeight;

        // The sheet never grows past the screen. On a bottom sheet a small strip is kept
        // free at the top so the rounded corners stay visible above the status bar.
        var maxSheetHeight = isSheet
            ? Math.Max(320, availableHeight - SheetTopInset)
            : Math.Max(320, availableHeight - 24);
        var maxBody = Math.Max(120, maxSheetHeight - chromeHeight - footerHeight);

        if (totalNatural <= maxSheetHeight)
        {
            // Everything fits on screen: hug the content and let the body breathe.
            if (bodyScroll.HeightRequest != -1) bodyScroll.HeightRequest = -1;
            if (bodyScroll.MaximumHeightRequest != double.PositiveInfinity)
                bodyScroll.MaximumHeightRequest = double.PositiveInfinity;
            if (root.HeightRequest != -1) root.HeightRequest = -1;
            if (root.MaximumHeightRequest != double.PositiveInfinity)
                root.MaximumHeightRequest = double.PositiveInfinity;

            currentSheetHeight = totalNatural;
            return;
        }

        // The content overflows the screen. The sheet is pinned to the viewport height and
        // the body is capped to the remaining space, so the body scrolls while the chrome
        // and the footer stay in place.
        if (Math.Abs(bodyScroll.HeightRequest - maxBody) > 0.5)
            bodyScroll.HeightRequest = maxBody;
        if (Math.Abs(bodyScroll.MaximumHeightRequest - maxBody) > 0.5)
            bodyScroll.MaximumHeightRequest = maxBody;
        if (Math.Abs(root.HeightRequest - maxSheetHeight) > 0.5)
            root.HeightRequest = maxSheetHeight;
        if (Math.Abs(root.MaximumHeightRequest - maxSheetHeight) > 0.5)
            root.MaximumHeightRequest = maxSheetHeight;

        currentSheetHeight = maxSheetHeight;
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

    /// <summary>
    /// A circular back/close button used in the sticky sheet header. The button is a
    /// non-interactive <see cref="Border"/> wrapped in a tap gesture because <see cref="SfButton"/>
    /// reserves extra padding that doesn't fit a 36x36 icon-only target.
    /// </summary>
    private static View BuildSheetNavButton(string glyph, double size, Action onTapped)
    {
        var icon = Icon(glyph, size, Color.FromArgb(MutedColor), "MaterialAssets");
        var tile = new Border
        {
            WidthRequest = 36,
            HeightRequest = 36,
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) },
            Content = icon
        };
        var grid = new Grid();
        grid.Add(tile);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => onTapped();
        grid.GestureRecognizers.Add(tap);
        return grid;
    }

    /// <summary>
    /// The "PROFILE READY" pill shown in the centred cell of the sticky sheet nav row.
    /// The pill sits in its own light-cyan background and contains a green status dot plus
    /// the bold uppercase label.
    /// </summary>
    private static View BuildProfileReadyPill()
    {
        var dot = new Ellipse
        {
            WidthRequest = 7,
            HeightRequest = 7,
            Fill = Color.FromArgb("#007A56"),
            VerticalOptions = LayoutOptions.Center
        };

        var text = Text("PROFILE READY", 10.5, Color.FromArgb(MutedColor), FontAttributes.Bold);

        var row = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        row.Add(dot);
        row.Add(text);

        return new Border
        {
            BackgroundColor = Color.FromArgb("#DCEDFE"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(10, 4),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = row
        };
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

        if (isSheet)
        {
            // The phone sheet is split into two stacked rows inside the sticky chrome:
            //   1) navigation row (back arrow, PROFILE READY pill, close)
            //   2) centered icon, title and subtitle (the "Activate Your eSIM" block)
            // Both rows live above the scrollable body, so the title stays pinned to the
            // top while the QR card, activation code and setup guide are scrolled.

            var navBack = BuildSheetNavButton(IconArrowLeft, 16, () => CloseRequested?.Invoke(this, EventArgs.Empty));
            var navClose = BuildSheetNavButton(IconClose, 16, () => CloseRequested?.Invoke(this, EventArgs.Empty));
            var profileReady = BuildProfileReadyPill();

            var navRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),  // 0 back
                    new ColumnDefinition(GridLength.Star),  // 1 centered pill
                    new ColumnDefinition(GridLength.Auto)   // 2 close
                },
                ColumnSpacing = 8,
                Padding = new Thickness(16, 12, 12, 0)
            };
            navRow.Add(navBack, 0, 0);
            navRow.Add(profileReady, 1, 0);
            navRow.Add(navClose, 2, 0);

            // Centre the title and subtitle below the nav row, as in the reference.
            var sheetTitle = Text("Activate Your eSIM", 21, Color.FromArgb(InkColor), FontAttributes.Bold);
            sheetTitle.HorizontalTextAlignment = TextAlignment.Center;
            var sheetSubtitle = Text(
                "Scan this QR code from your device settings or print/share to another screen.",
                11.5, Color.FromArgb(MutedColor));
            sheetSubtitle.HorizontalTextAlignment = TextAlignment.Center;

            var centeredTitles = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center };
            centeredTitles.Add(sheetTitle);
            centeredTitles.Add(sheetSubtitle);

            var centeredBlock = new VerticalStackLayout { Spacing = 12, HorizontalOptions = LayoutOptions.Center };
            centeredBlock.Add(iconTile);
            centeredBlock.Add(centeredTitles);

            var centeredHeader = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 12,
                Padding = new Thickness(16, 14, 16, 12)
            };
            centeredHeader.Add(new Label(), 0, 0);
            centeredHeader.Add(centeredBlock, 1, 0);
            centeredHeader.Add(new Label(), 2, 0);

            var sheetHeader = new VerticalStackLayout { Spacing = 0 };
            sheetHeader.Add(navRow);
            sheetHeader.Add(centeredHeader);
            return sheetHeader;
        }

        return header;
    }

    private View BuildProfileBanner(EsimProfile? profile)
    {
        if (isSheet)
        {
            // On the mobile bottom sheet, the profile readiness pill is embedded directly
            // inside the QR card at the top of the scrollable body, so the banner host in
            // the sticky chrome stays empty.
            return new BoxView { HeightRequest = 0, Color = Colors.Transparent, IsVisible = false };
        }

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
        if (isSheet)
        {
            // The phone sheet shows a single readiness line, as in the reference.
            var readyDot = Icon(IconCircleCheck, 11, Color.FromArgb("#006172"), "MaterialAssets");
            var readyText = Text($"Ready to Install  •  {profile?.Carrier ?? "SoftBank SG"}",
                11, Color.FromArgb("#006172"), FontAttributes.Bold);
            var readyRow = new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };
            readyRow.Add(readyDot);
            readyRow.Add(readyText);

            var readyPill = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb(OutlineVariant),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
                Padding = new Thickness(9, 4),
                HorizontalOptions = LayoutOptions.Start,
                Content = readyRow
            };

            stack.Add(readyPill);
        }
        else
        {
            stack.Add(titleRow);
            stack.Add(tier);
        }

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

        // QR code rendered with dark ink modules on a white quiet zone.
        // ForegroundColor drives the module fill used by QRCode.DrawBarcode.
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

        var captionStack = new VerticalStackLayout { Spacing = 14 };
        captionStack.Add(caption1);
        captionStack.Add(caption2);
        captionStack.Add(caption3Stack);

        // Heading on top, QR centered in the flexible middle row, caption pinned to the bottom.
        // The Star row absorbs the leftover height so the QR stays optically centered in the card.
        var cardGrid = new Grid
        {
            RowSpacing = 14,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        cardGrid.Add(heading, 0, 0);
        cardGrid.Add(qrFrame, 0, 1);
        cardGrid.Add(captionStack, 0, 2);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(14, 14),
            Content = cardGrid
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

    /// <summary>
    /// Builds the phone sheet body: the QR card, the activation code block, the quick setup
    /// guide and the collapsible manual activation section, all stacked vertically.
    /// </summary>
    private static View BuildSheetBody(EsimProfile? profile, string activationCode)
    {
        var body = new VerticalStackLayout { Spacing = 14 };

        body.Add(BuildSheetQrCard(profile, activationCode));
        body.Add(BuildSheetActivationCode(activationCode));
        body.Add(BuildSheetSetupGuide());
        body.Add(BuildSheetManualActivation());

        return body;
    }

    /// <summary>The QR card with the readiness pill, the code and the caption.</summary>
    private static View BuildSheetQrCard(EsimProfile? profile, string activationCode)
    {
        var barcode = new SfBarcodeGenerator
        {
            ShowText = false,
            HeightRequest = 190,
            WidthRequest = 190,
            BackgroundColor = Colors.White,
            ForegroundColor = Colors.Black,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Symbology = new QRCode(),
            Value = activationCode
        };

        // Centre badge — a small blue square with rounded corners and a white check mark,
        // matching the desktop QR card. The badge sits inside the QR code's centre square
        // by anchoring it to all four edges of the cell.
        var checkBadge = new Border
        {
            WidthRequest = 34,
            HeightRequest = 34,
            BackgroundColor = Color.FromArgb(PrimaryActionColor),
            Stroke = Colors.White,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = Icon(IconCheckTick, 14, Colors.White, "MaterialAssets")
        };

        var qrStack = new Grid
        {
            WidthRequest = 190,
            HeightRequest = 190,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        qrStack.Add(barcode, 0, 0);
        qrStack.Add(checkBadge, 0, 0);

        // Four L-shaped corner brackets frame the QR code, matching the desktop card and
        // the phone reference.
        var qrFrame = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        qrFrame.Add(BuildCornerBracket(Corner.TopLeft), 0, 0);
        qrFrame.Add(BuildCornerBracket(Corner.TopRight), 2, 0);
        qrFrame.Add(BuildCornerBracket(Corner.BottomLeft), 0, 2);
        qrFrame.Add(BuildCornerBracket(Corner.BottomRight), 2, 2);
        qrFrame.Add(qrStack, 1, 1);

        var caption = new HorizontalStackLayout { Spacing = 5, HorizontalOptions = LayoutOptions.Center };
        caption.Add(Icon(IconCircleCheck, 10, Color.FromArgb("#006141"), "MaterialAssets"));
        caption.Add(Text("Single-use eSIM cryptographic key", 9.5, Color.FromArgb(MutedColor)));

        var stack = new VerticalStackLayout { Spacing = 14 };
        stack.Add(BuildSheetReadyPill(profile));
        stack.Add(qrFrame);
        stack.Add(caption);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(14, 16),
            Content = stack
        };
    }

    /// <summary>
    /// The white readiness pill at the top of the mobile QR card: "Ready to Install • Carrier".
    /// The pill is the only place the carrier is named inside the scrollable body, so the
    /// user can confirm the install target without scrolling back to the chrome banner.
    /// </summary>
    private static View BuildSheetReadyPill(EsimProfile? profile)
    {
        var dot = Icon(IconCircleCheck, 11, Color.FromArgb("#007A56"), "MaterialAssets");

        var labelText = new FormattedString
        {
            Spans =
            {
                new Span
                {
                    Text = "Ready to Install  \u2022  ",
                    FontFamily = UiFont,
                    FontSize = 11.5,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb(InkColor)
                },
                new Span
                {
                    Text = profile?.Carrier ?? "SoftBank 5G",
                    FontFamily = UiFont,
                    FontSize = 11.5,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb(PrimaryColor)
                }
            }
        };

        var label = new Label { FormattedText = labelText, VerticalOptions = LayoutOptions.Center };

        var row = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        row.Add(dot);
        row.Add(label);

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(10, 5),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = row
        };
    }

    /// <summary>The activation code block with the code, the copy button and the two actions.</summary>
    private static View BuildSheetActivationCode(string activationCode)
    {
        var labelRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            VerticalOptions = LayoutOptions.Center
        };
        labelRow.Add(Text("Activation Code", 12, Color.FromArgb(InkColor), FontAttributes.Bold), 0, 0);

        var lpaBadge = new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Padding = new Thickness(6, 2),
            VerticalOptions = LayoutOptions.Center,
            Content = Text("LPA STRING", 8.5, Color.FromArgb(MutedColor), FontAttributes.Bold)
        };
        labelRow.Add(lpaBadge, 1, 0);

        var codeRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };
        var codeText = MonoText(activationCode, 10, Color.FromArgb(InkColor), truncate: false);
        codeText.LineBreakMode = LineBreakMode.TailTruncation;
        codeRow.Add(codeText, 0, 0);
        codeRow.Add(BuildSheetCopyButton(), 1, 0);

        var codeBlock = new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Padding = new Thickness(10, 7),
            Content = codeRow
        };

        // Two equal-width actions under the code.
        var copyCode = BuildSheetActionButton("Copy Code", IconCopy, Color.FromArgb(PrimaryColor), Colors.White);
        var saveShare = BuildSheetActionButton("Save / Share", IconShare, Colors.White, Color.FromArgb(InkColor));
        var actions = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };
        actions.Add(copyCode, 0, 0);
        actions.Add(saveShare, 1, 0);

        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Add(labelRow);
        stack.Add(codeBlock);
        stack.Add(actions);

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

    private static View BuildSheetCopyButton()
    {
        var row = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        row.Add(Icon(IconCopy, 11, Color.FromArgb(PrimaryColor), "MaterialAssets"));
        row.Add(Text("Copy", 10.5, Color.FromArgb(PrimaryColor), FontAttributes.Bold));

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Padding = new Thickness(8, 3),
            Content = row
        };
    }

    /// <summary>An outlined or filled action button used by the phone sheet.</summary>
    private static View BuildSheetActionButton(string label, string glyph, Color background, Color foreground)
    {
        var row = new HorizontalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        row.Add(Icon(glyph, 12, foreground, "MaterialAssets"));
        row.Add(Text(label, 11.5, foreground, FontAttributes.Bold));

        return new Border
        {
            BackgroundColor = background,
            Stroke = background == Colors.White ? Color.FromArgb(OutlineVariant) : background,
            StrokeThickness = background == Colors.White ? 1 : 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(12, 10),
            HorizontalOptions = LayoutOptions.Fill,
            Content = row
        };
    }

    /// <summary>The quick setup guide with the three numbered steps.</summary>
    private static View BuildSheetSetupGuide()
    {
        var header = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            VerticalOptions = LayoutOptions.Center
        };
        header.Add(Text("Quick Setup Guide", 13, Color.FromArgb(InkColor), FontAttributes.Bold), 0, 0);
        header.Add(Text("iOS & Android", 10.5, Color.FromArgb(MutedColor)), 1, 0);

        var stack = new VerticalStackLayout { Spacing = 10 };
        stack.Add(header);
        stack.Add(ActivationStep("1", "Open Mobile Network Settings",
            "Navigate to Settings > Cellular > Mobile Service."));
        stack.Add(ActivationStep("2", "Tap \"Add eSIM\" & Scan QR",
            "Use your phone camera or select \"Use QR Code\" from the cellular prompt."));
        stack.Add(ActivationStep("3", "Turn On Data Roaming",
            "Switch this profile on and enable Data Roaming when entering Japan."));

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

    /// <summary>The collapsed manual activation section shown at the bottom of the sheet.</summary>
    private static View BuildSheetManualActivation()
    {
        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };
        row.Add(Icon(IconTable, 13, Color.FromArgb(MutedColor), "MaterialAssets"), 0, 0);
        row.Add(Text("Need Manual SM-DP+ Address?", 12, Color.FromArgb(InkColor), FontAttributes.Bold), 1, 0);
        row.Add(Icon("\uE5CF", 14, Color.FromArgb(MutedColor), "MaterialAssets"), 2, 0);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(12, 12),
            Content = row
        };
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

        // Footer actions — fill equal-width columns and match the desktop reference.
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
        emailRow.Add(Icon(IconEmail, 16, Color.FromArgb("#006172"), "MaterialAssets"), 0, 0);
        emailRow.Add(Text("Send QR to Email", 12, Color.FromArgb(InkColor)), 1, 0);

        var sendEmail = new SfButton
        {
            Background = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 0,
            CornerRadius = 8,
            Padding = new Thickness(16, 8),
            HorizontalOptions = LayoutOptions.Fill,
            HeightRequest = 40,
            Shadow = new Shadow { Brush = Color.FromArgb("#24324A"), Opacity = 0.06f, Radius = 6, Offset = new Point(0, 2) },
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
        scannedRow.Add(Icon(IconCircleCheck, 16, Colors.White, "MaterialAssets"), 0, 0);
        scannedRow.Add(Text("I've Scanned the QR Code", 12, Colors.White, FontAttributes.Bold), 1, 0);

        var scanned = new SfButton
        {
            Background = Color.FromArgb(PrimaryColor),
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            CornerRadius = 8,
            Padding = new Thickness(20, 8),
            HorizontalOptions = LayoutOptions.Fill,
            HeightRequest = 40,
            Shadow = new Shadow { Brush = Color.FromArgb("#24324A"), Opacity = 0.16f, Radius = 14, Offset = new Point(0, 6) },
            Content = new DataTemplate(() => scannedRow)
        };

        // Footer has NO background fill — just border-top separator
        var footer = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(3, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(5, GridUnitType.Star))
            },
            ColumnSpacing = 8,
            Padding = new Thickness(16, 14, 16, 16)
        };
        footer.Add(iccidRow, 0, 0);
        footer.Add(sendEmail, 1, 0);
        footer.Add(scanned, 2, 0);

        if (isSheet)
        {
            // The phone sheet uses a single full-width primary button with a text link
            // underneath, as in the reference.
            var completedRow = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            completedRow.Add(Icon(IconCircleCheck, 16, Colors.White, "MaterialAssets"));
            completedRow.Add(Text("I've Completed Setup", 14, Colors.White, FontAttributes.Bold));

            var completed = new SfButton
            {
                Background = Color.FromArgb(PrimaryColor),
                Stroke = Colors.Transparent,
                StrokeThickness = 0,
                CornerRadius = 10,
                Padding = new Thickness(20, 10),
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = 48,
                Content = new DataTemplate(() => completedRow)
            };
            completed.Clicked += (_, _) => ScannedRequested?.Invoke(this, EventArgs.Empty);

            var troubleshoot = new SfButton
            {
                Text = "Troubleshoot Connection",
                FontFamily = UiFont,
                FontSize = 13,
                Background = Colors.Transparent,
                TextColor = Color.FromArgb(MutedColor),
                CornerRadius = 8,
                Padding = new Thickness(12, 6),
                HorizontalOptions = LayoutOptions.Center,
                HeightRequest = 40
            };
            troubleshoot.Clicked += (_, _) => ScannedRequested?.Invoke(this, EventArgs.Empty);

            var sheetFooter = new VerticalStackLayout { Spacing = 6, Padding = new Thickness(16, 14, 16, 16) };
            sheetFooter.Add(completed);
            sheetFooter.Add(troubleshoot);

            return new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                Stroke = Color.FromArgb(OutlineVariant),
                Content = sheetFooter
            };
        }

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

    /// <summary>Raised when the sheet content resizes, so the popup can be re-anchored.</summary>
    public event EventHandler? SheetResized;

    private void OnRootSized()
    {
        ApplyViewport();
        SheetResized?.Invoke(this, EventArgs.Empty);
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
