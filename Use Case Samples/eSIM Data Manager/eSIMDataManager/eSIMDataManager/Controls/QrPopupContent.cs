using eSIMDataManager.Models;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Barcode;
using Syncfusion.Maui.Buttons;

namespace eSIMDataManager.Controls;

/// <summary>
/// Builds the Install &amp; Activate eSIM dialog content that mirrors the reference design:
/// a scan panel on the left, activation steps and manual parameters on the right, and a footer bar.
/// The body scrolls so long content is never clipped on small screens.
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
    private const string IconTable = "\uE7AF";      // table
    private const string IconClose = "\uE70B";      // close
    private const string IconDot = "\uE73F";       // circle
    private const string IconCheckTick = "\uE70C"; // check-tick
    private const string IconSearch = "\uE715";     // search
    private const string IconCopy = "\uE7A0";       // copy
    private const string IconEmail = "\uE717";      // email
    private const string IconCircleCheck = "\uE78C";// circle-check
    private const string IconUser = "\uE71B";       // user

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

    public QrPopupContent(EsimProfile? profile)
    {
        var activationCode = profile?.ActivationCode ?? "LPA:1$demo.carrier.example$SAMPLE";

        // A plain stack keeps every section at its natural height: the dialog hugs its content
        // instead of stretching, so no empty space appears under the footer. The scrolling body
        // is capped separately so long content never overflows.
        root = new VerticalStackLayout { Spacing = 0, BackgroundColor = Colors.White };
        root.Resources.Add(PopupLabelStyle);

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
            Padding = new Thickness(14, 0, 14, 0)
        };
        bodyGrid.Add(scanCardHost, 0, 0);
        bodyGrid.Add(stepsHost, 1, 0);

        bodyScroll = new ScrollView
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = bodyGrid
        };

        root.Add(bodyScroll);
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
        {
            bodyGrid.Add(compactStack, 0, 0);
        }
        else
        {
            bodyGrid.Add(scanCardHost, 0, 0);
            bodyGrid.Add(stepsHost, 1, 0);
        }
    }

    /// <summary>Caps the scrollable body so long content scrolls instead of overflowing the dialog.</summary>
    public void SetViewport(double availableHeight)
    {
        if (availableHeight <= 0) return;
        var limit = Math.Max(200, availableHeight - 24);
        if (Math.Abs(bodyScroll.MaximumHeightRequest - limit) > 0.5)
            bodyScroll.MaximumHeightRequest = limit;
    }

    /// <summary>
    /// Creates a label whose color is stable across app themes. Binding the same value for light
    /// and dark keeps the text readable on the white dialog surface regardless of the app theme.
    /// </summary>
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

    /// <summary>Monospaced variant used for telemetry identifiers such as ICCID and LPA strings.</summary>
    private static Label MonoText(string text, double size, Color color)
    {
        var label = new Label
        {
            Text = text,
            FontFamily = MonoFont,
            FontSize = size,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        label.SetAppThemeColor(Label.TextColorProperty, color, color);
        return label;
    }

    /// <summary>
    /// Creates an icon glyph. The icon font is applied to the glyph alone; mixing a Material
    /// codepoint into a text run renders a fallback bar instead of the icon.
    /// </summary>
    private static Label Icon(string glyph, double size, Color color)
    {
        var label = new Label
        {
            Text = glyph,
            FontFamily = "MaterialAssets",
            FontSize = size,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        label.SetAppThemeColor(Label.TextColorProperty, color, color);
        return label;
    }

    private static View BuildAccentBar() => new BoxView
    {
        HeightRequest = 4,
        Color = Color.FromArgb(PrimaryActionColor),
        HorizontalOptions = LayoutOptions.Fill
    };

    private View BuildHeader(EsimProfile? profile)
    {
        var iconTile = new Border
        {
            WidthRequest = 34,
            HeightRequest = 34,
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
            Content = Icon(IconTable, 17, Color.FromArgb(PrimaryColor))
        };

        // The badge sits on the same line as the title, matching the reference header.
        var titleRow = new HorizontalStackLayout { Spacing = 7, VerticalOptions = LayoutOptions.Center };
        titleRow.Add(Text("Install & Activate eSIM", 17, Color.FromArgb(InkColor), FontAttributes.Bold));

        var badge = new Border
        {
            BackgroundColor = Color.FromArgb("#ECFDF5"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(8, 3),
            VerticalOptions = LayoutOptions.Center,
            Content = Text("●  Ready to Install", 9, Color.FromArgb("#065F46"), FontAttributes.Bold)
        };
        titleRow.Add(badge);

        var titles = new VerticalStackLayout { Spacing = 1 };
        titles.Add(titleRow);
        titles.Add(Text("Quick setup configuration for instant cross-border high-speed routing", 10, Color.FromArgb(MutedColor)));

        var close = new SfButton
        {
            Text = IconClose,
            FontFamily = "MaterialAssets",
            FontSize = 15,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(MutedColor),
            WidthRequest = 32,
            HeightRequest = 32,
            CornerRadius = 9,
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Center
        };
        close.Clicked += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 9,
            Padding = new Thickness(14, 10, 12, 8)
        };
        header.Add(iconTile, 0, 0);
        header.Add(titles, 1, 0);
        header.Add(close, 2, 0);
        return header;
    }

    private static View BuildProfileBanner(EsimProfile? profile)
    {
        var country = profile?.Country ?? "Japan";
        var countryCode = country.Equals("Japan", StringComparison.OrdinalIgnoreCase) ? "JP" : country[..2].ToUpperInvariant();
        var network = profile?.Network == NetworkType.FiveG ? "5G" : "4G / LTE";

        var titleRow = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        titleRow.Add(Text(countryCode, 13.5, Color.FromArgb(InkColor), FontAttributes.Bold));
        titleRow.Add(Text(profile?.ProfileName ?? "Japan 5G Explorer", 13.5, Color.FromArgb(InkColor), FontAttributes.Bold));
        titleRow.Add(Icon(IconDot, 6, Color.FromArgb(OutlineVariant)));
        titleRow.Add(Text($"{profile?.Carrier ?? "SoftBank Corp."}  ({network} Multi-IMSI)", 11, Color.FromArgb(MutedColor)));

        var tier = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(8, 3),
            HorizontalOptions = LayoutOptions.Start,
            Content = Text($"◉  {profile?.AllocatedGb ?? 10:0.#} GB  ·  {Math.Max(1, profile?.DaysRemaining ?? 30)} Days Global Tier", 10.5, Color.FromArgb("#006172"))
        };

        var stack = new VerticalStackLayout { Spacing = 6 };
        stack.Add(titleRow);
        stack.Add(tier);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(13, 10),
            Margin = new Thickness(14, 0, 14, 12),
            Content = stack
        };
    }

    private static View BuildScanCard(EsimProfile? profile, string activationCode)
    {
        var heading = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 7
        };
        heading.Add(Icon(IconSearch, 12, Color.FromArgb(PrimaryColor)), 0, 0);
        heading.Add(Text("Instant Scan", 12, Color.FromArgb(InkColor), FontAttributes.Bold), 1, 0);
        heading.Add(MonoText($"EID: {Truncate(profile?.Iccid ?? "8904908320000000000", 12)}", 9.5, Color.FromArgb(MutedColor)), 2, 0);

        var barcode = new SfBarcodeGenerator
        {
            ShowText = false,
            HeightRequest = 196,
            WidthRequest = 196,
            HorizontalOptions = LayoutOptions.Center,
            Symbology = new QRCode(),
            Value = activationCode
        };

        var stack = new VerticalStackLayout { Spacing = 10 };
        stack.Add(heading);
        stack.Add(new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(12),
            HorizontalOptions = LayoutOptions.Center,
            Content = barcode
        });
        stack.Add(Text("Scan this QR code with your device camera or", 10, Color.FromArgb(InkColor)));
        stack.Add(Text("in Settings > Cellular > Add eSIM.", 10, Color.FromArgb(PrimaryColor), FontAttributes.Bold));
        stack.Add(Text("●  Single-use cryptographic profile token", 9.5, Color.FromArgb(MutedColor)));

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(13),
            Content = stack
        };
    }

    private static View BuildSteps(EsimProfile? profile, string activationCode)
    {
        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Add(Text("ACTIVATION STEPS", 11.5, Color.FromArgb(InkColor), FontAttributes.Bold));
        stack.Add(ActivationStep("1", "Scan QR Code in Device Settings", "Navigate to Settings > Cellular (iOS) or SIM Manager (Android) and add plan."));
        stack.Add(ActivationStep("2", "Enable 'Data Roaming' upon Landing", "Turn on roaming toggles specifically for this profile once in Japan."));
        stack.Add(ActivationStep("3", "Set Profile as Cellular Data Carrier", "Keep your primary home SIM for SMS 2FA while using this eSIM for all data."));
        stack.Add(BuildManualActivation(activationCode));
        stack.Add(BuildCompatibility());
        return stack;
    }

    private static View BuildManualActivation(string activationCode)
    {
        var header = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        header.Add(Icon(IconTable, 13, Color.FromArgb(PrimaryColor)), 0, 0);
        header.Add(Text("Manual Activation\nParameters", 11.5, Color.FromArgb(InkColor), FontAttributes.Bold), 1, 0);
        header.Add(new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Padding = new Thickness(7, 3),
            VerticalOptions = LayoutOptions.Center,
            Content = MonoText("RSP Protocol\nv2.2", 9, Color.FromArgb("#006141"))
        }, 2, 0);

        var stack = new VerticalStackLayout { Spacing = 7 };
        stack.Add(header);
        stack.Add(ActivationParameter("SM-DP+ Address", "rsp.global-esim.net", Color.FromArgb(InkColor)));
        stack.Add(ActivationParameter("Activation Code (LPA String)", activationCode, Color.FromArgb(PrimaryColor)));
        stack.Add(CopyButton("Copy Full Activation String", activationCode));

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(11),
            Content = stack
        };
    }

    private static View BuildCompatibility()
    {
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 8
        };
        grid.Add(Icon(IconUser, 13, Color.FromArgb(MutedColor)), 0, 0);
        grid.Add(Text("Compatibility: Unlocked iPhone XS or newer, Pixel 3 or later, Galaxy S20+ and compatible dual-SIM devices.", 10, Color.FromArgb(MutedColor)), 1, 0);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(11),
            Content = grid
        };
    }

    private static View ActivationStep(string number, string title, string description)
    {
        var badge = new Border
        {
            WidthRequest = 22,
            HeightRequest = 22,
            BackgroundColor = Color.FromArgb(PrimaryColor),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Content = Text(number, 10, Colors.White, FontAttributes.Bold)
        };

        var text = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        text.Add(Text(title, 11, Color.FromArgb(InkColor), FontAttributes.Bold));
        text.Add(Text(description, 9.5, Color.FromArgb(MutedColor)));

        // The badge and the text must sit in separate columns; a Grid children initializer would
        // place both in column 0 and the badge would overlap the text.
        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 8
        };
        row.Add(badge, 0, 0);
        row.Add(text, 1, 0);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(9),
            Content = row
        };
    }

    private static View ActivationParameter(string label, string value, Color valueColor)
    {
        var valueRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 6
        };
        valueRow.Add(MonoText(value, 10, valueColor), 0, 0);
        valueRow.Add(CopyIconButton(value), 1, 0);

        var stack = new VerticalStackLayout { Spacing = 2 };
        stack.Add(Text(label, 9, Color.FromArgb(MutedColor)));
        stack.Add(new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(7) },
            Padding = new Thickness(9, 6),
            Content = valueRow
        });
        return stack;
    }

    private static View CopyIconButton(string value)
    {
        var copy = new SfButton
        {
            Text = IconCopy,
            FontFamily = "MaterialAssets",
            FontSize = 11,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(MutedColor),
            WidthRequest = 22,
            HeightRequest = 22,
            Padding = new Thickness(0),
            CornerRadius = 6
        };
        copy.Clicked += async (_, _) => await Clipboard.Default.SetTextAsync(value);
        return copy;
    }

    private static View CopyButton(string text, string value)
    {
        var row = new HorizontalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        row.Add(Icon(IconCopy, 11, Color.FromArgb(PrimaryColor)));
        row.Add(Text(text, 10.5, Color.FromArgb(PrimaryColor)));

        var copy = new SfButton
        {
            Background = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            CornerRadius = 8,
            Padding = new Thickness(10, 6),
            HorizontalOptions = LayoutOptions.Fill,
            Content = new DataTemplate(() => row)
        };
        copy.Clicked += async (_, _) => await Clipboard.Default.SetTextAsync(value);
        return copy;
    }

    private View BuildFooter(EsimProfile? profile, string activationCode)
    {
        var iccid = new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };
        iccid.Add(Icon(IconCircleCheck, 11, Color.FromArgb("#065F46")));
        iccid.Add(MonoText($"ICCID: {FormatIccid(profile?.Iccid ?? "DEMO-8931-0248")}", 9.5, Color.FromArgb(MutedColor)));

        var emailRow = new HorizontalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        emailRow.Add(Icon(IconEmail, 11, Color.FromArgb(InkColor)));
        emailRow.Add(Text("Send QR to Email", 10.5, Color.FromArgb(InkColor)));

        var sendEmail = new SfButton
        {
            Background = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            CornerRadius = 8,
            Padding = new Thickness(12, 7),
            Content = new DataTemplate(() => emailRow)
        };
        sendEmail.Clicked += async (_, _) =>
        {
            try
            {
                await Email.Default.ComposeAsync(new EmailMessage(
                    "Your eSIM activation details",
                    $"Demo QR only - this sample cannot provision an eSIM.\n\nActivation string: {activationCode}"));
            }
            catch (FeatureNotSupportedException)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Email unavailable", "Email composition is not available on this device.", "OK");
            }
        };

        var scannedRow = new HorizontalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        scannedRow.Add(Icon(IconCircleCheck, 11, Colors.White));
        scannedRow.Add(Text("I've Scanned the QR Code", 10.5, Colors.White, FontAttributes.Bold));

        var scanned = new SfButton
        {
            Background = Color.FromArgb(PrimaryColor),
            StrokeThickness = 0,
            CornerRadius = 8,
            Padding = new Thickness(14, 7),
            Content = new DataTemplate(() => scannedRow)
        };
        scanned.Clicked += async (_, _) =>
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Demo only", "This sample does not install or activate an eSIM on your device.", "OK");
            ScannedRequested?.Invoke(this, EventArgs.Empty);
        };

        var footer = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8,
            Padding = new Thickness(14, 9, 14, 11)
        };
        footer.Add(iccid, 0, 0);
        footer.Add(sendEmail, 1, 0);
        footer.Add(scanned, 2, 0);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            Content = footer
        };
    }

    private static string Truncate(string value, int length) =>
        value.Length <= length ? value : value[..length] + "…";

    private static string FormatIccid(string iccid)
    {
        var digits = new string(iccid.Where(char.IsLetterOrDigit).ToArray());
        if (digits.Length < 19) return digits;
        return string.Join(' ', Enumerable.Range(0, 4)
            .Select(i => digits.Substring(i * 4, Math.Min(4, digits.Length - (i * 4)))));
    }
}
