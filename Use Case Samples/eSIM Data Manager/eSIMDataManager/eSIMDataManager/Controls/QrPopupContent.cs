using eSIMDataManager.Models;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Barcode;
using Syncfusion.Maui.Buttons;

namespace eSIMDataManager.Controls;

/// <summary>
/// Builds the Install &amp; Activate eSIM dialog content that mirrors the reference design:
/// a scan panel on the left, activation steps and manual parameters on the right, and a footer bar.
/// The root is a grid with fixed chrome rows around a star body row, so the footer always stays
/// on screen and the body scrolls instead of overflowing.
/// </summary>
public sealed class QrPopupContent
{
    private const string InkColor = "#0B1C30";
    private const string MutedColor = "#434656";
    private const string PrimaryColor = "#0046CC";
    private const string PrimaryActionColor = "#0D5CFF";
    private const string SurfaceLow = "#EFF4FF";
    private const string SurfaceCanvas = "#F8F9FF";
    private const string OutlineVariant = "#C3C5D9";
    private const string UiFont = "Inter";
    private const string MonoFont = "monospace";

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
    private double chromeHeight;

    public QrPopupContent(EsimProfile? profile)
    {
        var activationCode = profile?.ActivationCode ?? "LPA:1$demo.carrier.example$SAMPLE";

        // A plain stack keeps every section at its natural height: the dialog then hugs its
        // content instead of stretching to the screen, so no empty space appears under the
        // footer. The scrolling body is capped separately so long content never overflows.
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
            bodyGrid.Children.Add(compactStack);
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
        this.availableHeight = availableHeight;
        ApplyScrollLimit();
    }

    private void RootOnSizeChanged(object? sender, EventArgs e)
    {
        // The chrome (accent, header, banner, footer) sits outside the scrolling body, so the
        // space left for the body is the screen height minus that chrome.
        var chrome = root.Height - bodyScroll.Height;
        if (chrome <= 0) return;
        chromeHeight = chrome;
        ApplyScrollLimit();
    }

    private void ApplyScrollLimit()
    {
        if (availableHeight <= 0 || chromeHeight <= 0) return;
        var limit = Math.Max(200, availableHeight - 24 - chromeHeight);
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

    private static View BuildAccentBar() => new BoxView
    {
        HeightRequest = 4,
        Color = Color.FromArgb(PrimaryActionColor),
        HorizontalOptions = LayoutOptions.Fill
    };

    private View BuildHeader(EsimProfile? profile)
    {
        var icon = new Border
        {
            WidthRequest = 34,
            HeightRequest = 34,
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
            Content = new Label
            {
                Text = "\uE7AF",
                FontFamily = "MaterialAssets",
                FontSize = 17,
                TextColor = Color.FromArgb(PrimaryColor),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var titles = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        titles.Add(Text("Install & Activate eSIM", 17, Color.FromArgb(InkColor), FontAttributes.Bold));
        titles.Add(Text("Quick setup configuration for instant cross-border high-speed routing", 10, Color.FromArgb(MutedColor)));

        var badge = new Border
        {
            BackgroundColor = Color.FromArgb("#ECFDF5"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(8, 3),
            VerticalOptions = LayoutOptions.Center,
            Content = Text("\uE70C  Ready to Install", 9, Color.FromArgb("#065F46"), FontAttributes.Bold)
        };

        var close = new SfButton
        {
            Text = "\uE70B",
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

        // The badge sits next to the title and the close button stays pinned to the far right.
        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8,
            Padding = new Thickness(14, 10, 12, 8)
        };
        header.Add(icon, 0, 0);
        header.Add(titles, 1, 0);
        header.Add(badge, 2, 0);
        header.Add(close, 4, 0);
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
        titleRow.Add(new Label
        {
            Text = "\uE73F",
            FontFamily = "MaterialAssets",
            FontSize = 6,
            TextColor = Color.FromArgb(OutlineVariant),
            VerticalOptions = LayoutOptions.Center
        });
        titleRow.Add(Text($"{profile?.Carrier ?? "SoftBank Corp."}  ({network} Multi-IMSI)", 11, Color.FromArgb(MutedColor)));

        var tier = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(8, 3),
            HorizontalOptions = LayoutOptions.Start,
            Content = MonoText($"\uE70C  {profile?.AllocatedGb ?? 10:0.#} GB  ·  {Math.Max(1, profile?.DaysRemaining ?? 30)} Days Global Tier", 10, Color.FromArgb("#006172"))
        };

        var stack = new VerticalStackLayout { Spacing = 6 };
        stack.Add(titleRow);
        stack.Add(tier);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(12, 9),
            Margin = new Thickness(14, 0, 14, 10),
            Content = stack
        };
    }

    private static View BuildScanCard(EsimProfile? profile, string activationCode)
    {
        var heading = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 6
        };
        heading.Add(Text("\uE715  Instant Scan", 11.5, Color.FromArgb(InkColor), FontAttributes.Bold), 0, 0);
        heading.Add(MonoText($"EID: {profile?.Iccid ?? "890490832..."}", 9, Color.FromArgb(MutedColor)), 1, 0);

        var barcode = new SfBarcodeGenerator
        {
            ShowText = false,
            HeightRequest = 168,
            WidthRequest = 168,
            HorizontalOptions = LayoutOptions.Center,
            Symbology = new QRCode(),
            Value = activationCode
        };

        var stack = new VerticalStackLayout { Spacing = 8 };
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

        var hint = Text("Scan this QR code with your device camera or\nin Settings > Cellular > Add eSIM.", 10, Color.FromArgb(InkColor));
        hint.HorizontalTextAlignment = TextAlignment.Center;
        stack.Add(hint);

        var token = Text("\uE70C  Single-use cryptographic profile token", 9, Color.FromArgb(MutedColor));
        token.HorizontalTextAlignment = TextAlignment.Center;
        stack.Add(token);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(12),
            Content = stack
        };
    }

    private static View BuildSteps(EsimProfile? profile, string activationCode)
    {
        var stack = new VerticalStackLayout { Spacing = 7 };
        stack.Add(Text("ACTIVATION STEPS", 11, Color.FromArgb(InkColor), FontAttributes.Bold));
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
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 8
        };
        header.Add(Text("Manual Activation\nParameters", 11, Color.FromArgb(InkColor), FontAttributes.Bold), 0, 0);

        var protocol = MonoText("RSP Protocol\nv2.2", 9, Color.FromArgb("#006141"));
        protocol.HorizontalTextAlignment = TextAlignment.End;
        header.Add(protocol, 1, 0);

        var stack = new VerticalStackLayout { Spacing = 6 };
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
            Padding = new Thickness(10),
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
        grid.Add(new Label
        {
            Text = "\uE71B",
            FontFamily = "MaterialAssets",
            FontSize = 12,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Start
        }, 0, 0);
        grid.Add(Text("Compatibility: Unlocked iPhone XS or newer, Pixel 3 or later, Galaxy S20+ and compatible dual-SIM devices.", 10, Color.FromArgb(MutedColor)), 1, 0);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(10),
            Content = grid
        };
    }

    private static View ActivationStep(string number, string title, string description)
    {
        var badge = new Border
        {
            WidthRequest = 20,
            HeightRequest = 20,
            BackgroundColor = Color.FromArgb(PrimaryColor),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Content = new Label
            {
                Text = number,
                FontFamily = UiFont,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var text = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        text.Add(Text(title, 11, Color.FromArgb(InkColor), FontAttributes.Bold));
        text.Add(Text(description, 9.5, Color.FromArgb(MutedColor)));

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(9),
            Content = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 8,
                Children =
                {
                    badge,
                    text
                }
            }
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
            Text = "\uE7A0",
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
        var copy = new SfButton
        {
            Text = $"\uE7A0  {text}",
            FontFamily = UiFont,
            FontSize = 10,
            TextColor = Color.FromArgb(PrimaryColor),
            Background = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            CornerRadius = 8,
            Padding = new Thickness(9, 6),
            HorizontalOptions = LayoutOptions.Fill
        };
        copy.Clicked += async (_, _) => await Clipboard.Default.SetTextAsync(value);
        return copy;
    }

    private View BuildFooter(EsimProfile? profile, string activationCode)
    {
        var iccid = MonoText($"ICCID: {FormatIccid(profile?.Iccid ?? "DEMO-8931-0248")}", 9, Color.FromArgb(MutedColor));
        iccid.LineBreakMode = LineBreakMode.WordWrap;

        var sendEmail = new SfButton
        {
            Text = $"\uE717  Send QR to Email",
            FontFamily = UiFont,
            FontSize = 10,
            TextColor = Color.FromArgb(InkColor),
            Background = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            CornerRadius = 8,
            Padding = new Thickness(10, 6)
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

        var scanned = new SfButton
        {
            Text = $"\uE78C  I've Scanned the QR Code",
            FontFamily = UiFont,
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            Background = Color.FromArgb(PrimaryActionColor),
            StrokeThickness = 0,
            CornerRadius = 8,
            Padding = new Thickness(12, 6)
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
            Padding = new Thickness(14, 9, 14, 10)
        };
        footer.Add(iccid, 0, 0);
        footer.Add(sendEmail, 1, 0);
        footer.Add(scanned, 2, 0);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceCanvas),
            StrokeThickness = 0,
            Content = footer
        };
    }

    private static string FormatIccid(string iccid)
    {
        var digits = new string(iccid.Where(char.IsLetterOrDigit).ToArray());
        if (digits.Length < 19) return digits;
        return string.Join(' ', Enumerable.Range(0, 4)
            .Select(i => digits.Substring(i * 4, Math.Min(4, digits.Length - (i * 4)))));
    }
}
