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

    /// <summary>Overrides the app-level Label style so text stays visible on the white dialog surface.</summary>
    private static readonly Style PopupLabelStyle = new(typeof(Label))
    {
        Setters =
        {
            new Setter { Property = Label.TextColorProperty, Value = Color.FromArgb(InkColor) },
            new Setter { Property = Label.FontFamilyProperty, Value = UiFont },
            new Setter { Property = Label.FontSizeProperty, Value = 12d },
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
    private bool chromeMeasured;

    public QrPopupContent(EsimProfile? profile)
    {
        var activationCode = profile?.ActivationCode ?? "LPA:1$demo.carrier.example$SAMPLE";

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
            ColumnSpacing = 14,
            Padding = new Thickness(16, 0, 16, 0)
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

    /// <summary>Limits the scrollable body so the dialog never grows past the available screen height.</summary>
    public void SetViewport(double availableHeight)
    {
        this.availableHeight = availableHeight;
        ApplyScrollLimit();
    }

    private void RootOnSizeChanged(object? sender, EventArgs e)
    {
        var chrome = root.Height - bodyScroll.Height;
        if (chrome <= 0) return;
        chromeHeight = chrome;
        chromeMeasured = true;
        ApplyScrollLimit();
    }

    private void ApplyScrollLimit()
    {
        if (!chromeMeasured || availableHeight <= 0) return;
        var limit = Math.Max(160, availableHeight - 32 - chromeHeight);
        if (Math.Abs(bodyScroll.MaximumHeightRequest - limit) > 0.5)
            bodyScroll.MaximumHeightRequest = limit;
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
            WidthRequest = 40,
            HeightRequest = 40,
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Content = new Label
            {
                Text = "\uE7AF",
                FontFamily = "MaterialAssets",
                FontSize = 20,
                TextColor = Color.FromArgb(PrimaryColor),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var titles = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        titles.Add(new Label
        {
            Text = "Install & Activate eSIM",
            FontFamily = UiFont,
            FontSize = 19,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor)
        });
        titles.Add(new Label
        {
            Text = "Quick setup configuration for instant cross-border high-speed routing",
            FontFamily = UiFont,
            FontSize = 11,
            TextColor = Color.FromArgb(MutedColor)
        });

        var badge = new Border
        {
            BackgroundColor = Color.FromArgb("#ECFDF5"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(9, 4),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = "\uE70C  Ready to Install",
                FontFamily = UiFont,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#065F46")
            }
        };

        var close = new SfButton
        {
            Text = "\uE70B",
            FontFamily = "MaterialAssets",
            FontSize = 15,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(MutedColor),
            WidthRequest = 36,
            HeightRequest = 36,
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
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10,
            Padding = new Thickness(16, 12, 14, 10)
        };
        header.Add(icon, 0, 0);
        header.Add(titles, 1, 0);
        header.Add(badge, 2, 0);
        header.Add(close, 3, 0);
        return header;
    }

    private static View BuildProfileBanner(EsimProfile? profile)
    {
        var country = profile?.Country ?? "Japan";
        var countryCode = country.Equals("Japan", StringComparison.OrdinalIgnoreCase) ? "JP" : country[..2].ToUpperInvariant();
        var network = profile?.Network == NetworkType.FiveG ? "5G" : "4G / LTE";

        var titleRow = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        titleRow.Add(new Label
        {
            Text = countryCode,
            FontFamily = UiFont,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        });
        titleRow.Add(new Label
        {
            Text = profile?.ProfileName ?? "Japan 5G Explorer",
            FontFamily = UiFont,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        });
        titleRow.Add(new Label
        {
            Text = "\uE73F",
            FontFamily = "MaterialAssets",
            FontSize = 6,
            TextColor = Color.FromArgb(OutlineVariant),
            VerticalOptions = LayoutOptions.Center
        });

        var meta = new Label
        {
            Text = $"{profile?.Carrier ?? "SoftBank Corp."}  ({network} Multi-IMSI)",
            FontFamily = UiFont,
            FontSize = 12,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };

        var tier = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(9, 4),
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = $"\uE70C  {profile?.AllocatedGb ?? 10:0.#} GB  ·  {Math.Max(1, profile?.DaysRemaining ?? 30)} Days Global Tier",
                FontFamily = MonoFont,
                FontSize = 11,
                TextColor = Color.FromArgb("#006172")
            }
        };

        var stack = new VerticalStackLayout { Spacing = 7 };
        stack.Add(titleRow);
        stack.Add(meta);
        stack.Add(tier);

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(14, 11),
            Margin = new Thickness(16, 0, 16, 12),
            Content = stack
        };
    }

    private static View BuildScanCard(EsimProfile? profile, string activationCode)
    {
        var heading = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        heading.Add(new Label
        {
            Text = "\uE715  Instant Scan",
            FontFamily = UiFont,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);
        heading.Add(new Label
        {
            Text = $"EID: {profile?.Iccid ?? "890490832..."}",
            FontFamily = MonoFont,
            FontSize = 10,
            TextColor = Color.FromArgb(MutedColor),
            LineBreakMode = LineBreakMode.TailTruncation,
            VerticalOptions = LayoutOptions.Center
        }, 1, 0);

        var barcode = new SfBarcodeGenerator
        {
            ShowText = false,
            HeightRequest = 180,
            WidthRequest = 180,
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
            Padding = new Thickness(14),
            HorizontalOptions = LayoutOptions.Center,
            Content = barcode
        });
        stack.Add(new Label
        {
            Text = "Scan this QR code with your device camera or\nin Settings > Cellular > Add eSIM.",
            FontFamily = UiFont,
            FontSize = 11,
            TextColor = Color.FromArgb(InkColor),
            HorizontalTextAlignment = TextAlignment.Center
        });
        stack.Add(new Label
        {
            Text = "\uE70C  Single-use cryptographic profile token",
            FontFamily = MonoFont,
            FontSize = 10,
            TextColor = Color.FromArgb(MutedColor),
            HorizontalTextAlignment = TextAlignment.Center
        });

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(14),
            Content = stack
        };
    }

    private static View BuildSteps(EsimProfile? profile, string activationCode)
    {
        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Add(new Label
        {
            Text = "ACTIVATION STEPS",
            FontFamily = UiFont,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor)
        });
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
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        header.Add(new Label
        {
            Text = "Manual Activation\nParameters",
            FontFamily = UiFont,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);
        header.Add(new Label
        {
            Text = "RSP Protocol\nv2.2",
            FontFamily = MonoFont,
            FontSize = 10,
            TextColor = Color.FromArgb("#006141"),
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center
        }, 1, 0);

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
            Padding = new Thickness(12),
            Content = stack
        };
    }

    private static View BuildCompatibility() => new Border
    {
        BackgroundColor = Color.FromArgb(SurfaceLow),
        StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
        Padding = new Thickness(12),
        Content = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 8,
            Children =
            {
                new Label
                {
                    Text = "\uE71B",
                    FontFamily = "MaterialAssets",
                    FontSize = 14,
                    TextColor = Color.FromArgb(MutedColor),
                    VerticalOptions = LayoutOptions.Start
                },
                new Label
                {
                    Text = "Compatibility: Unlocked iPhone XS or newer, Pixel 3 or later, Galaxy S20+ and compatible dual-SIM devices.",
                    FontFamily = UiFont,
                    FontSize = 11,
                    TextColor = Color.FromArgb(MutedColor)
                }
            }
        }
    };

    private static View ActivationStep(string number, string title, string description)
    {
        var badge = new Border
        {
            WidthRequest = 24,
            HeightRequest = 24,
            BackgroundColor = Color.FromArgb(PrimaryColor),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Content = new Label
            {
                Text = number,
                FontFamily = UiFont,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var text = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        text.Add(new Label
        {
            Text = title,
            FontFamily = UiFont,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor)
        });
        text.Add(new Label
        {
            Text = description,
            FontFamily = UiFont,
            FontSize = 10,
            TextColor = Color.FromArgb(MutedColor),
            LineBreakMode = LineBreakMode.WordWrap
        });

        return new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(10),
            Content = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 9,
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
            ColumnSpacing = 8
        };
        valueRow.Add(new Label
        {
            Text = value,
            FontFamily = MonoFont,
            FontSize = 11,
            TextColor = valueColor,
            LineBreakMode = LineBreakMode.TailTruncation,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);
        valueRow.Add(CopyIconButton(value), 1, 0);

        var stack = new VerticalStackLayout { Spacing = 3 };
        stack.Add(new Label
        {
            Text = label,
            FontFamily = UiFont,
            FontSize = 10,
            TextColor = Color.FromArgb(MutedColor)
        });
        stack.Add(new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(7) },
            Padding = new Thickness(10, 7),
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
            FontSize = 13,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(MutedColor),
            WidthRequest = 26,
            HeightRequest = 26,
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
            FontSize = 11,
            TextColor = Color.FromArgb(PrimaryColor),
            Background = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            CornerRadius = 8,
            Padding = new Thickness(10, 7),
            HorizontalOptions = LayoutOptions.Fill
        };
        copy.Clicked += async (_, _) => await Clipboard.Default.SetTextAsync(value);
        return copy;
    }

    private View BuildFooter(EsimProfile? profile, string activationCode)
    {
        var iccid = new Label
        {
            Text = $"ICCID: {FormatIccid(profile?.Iccid ?? "DEMO-8931-0248")}",
            FontFamily = MonoFont,
            FontSize = 10,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Center
        };

        var sendEmail = new SfButton
        {
            Text = $"\uE717  Send QR to Email",
            FontFamily = UiFont,
            FontSize = 11,
            TextColor = Color.FromArgb(InkColor),
            Background = Colors.White,
            Stroke = Color.FromArgb(OutlineVariant),
            StrokeThickness = 1,
            CornerRadius = 8,
            Padding = new Thickness(12, 7)
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
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            Background = Color.FromArgb(PrimaryActionColor),
            StrokeThickness = 0,
            CornerRadius = 8,
            Padding = new Thickness(14, 7)
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
            Padding = new Thickness(16, 10, 16, 12)
        };
        footer.Add(iccid, 0, 0);
        footer.Add(sendEmail, 1, 0);
        footer.Add(scanned, 2, 0);

        return new Border
        {
            BackgroundColor = Color.FromArgb("#F8F9FF"),
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
