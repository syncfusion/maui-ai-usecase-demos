using eSIMDataManager.Controls;
using eSIMDataManager.Models;
using eSIMDataManager.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Barcode;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Popup;

namespace eSIMDataManager;

public class DemoPage
{
    private MainViewModel ViewModel { get; }

    private readonly SfPopup popup = new()
    {
        ShowHeader = false,
        ShowFooter = false,
        WidthRequest = 760,
        StaysOpen = true,
        AutoSizeMode = PopupAutoSizeMode.Height,
        PopupStyle = new PopupStyle { PopupBackground = Colors.White, MessageBackground = Colors.White }
    };
    private readonly VerticalStackLayout popupContent = new() { Spacing = 14, Padding = 22 };
    private readonly SfPopup topUpPopup = new()
    {
        ShowHeader = false,
        ShowFooter = false,
        WidthRequest = 720,
        StaysOpen = true,
        AutoSizeMode = PopupAutoSizeMode.Height,
        PopupStyle = new PopupStyle { PopupBackground = Colors.White, MessageBackground = Colors.White }
    };
    private TopUpPopupContent? topUpContent;
    private double availableTopUpWidth = 720;
    private EsimProfile? popupProfile;

    public DemoPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        popup.ContentTemplate = new DataTemplate(() => popupContent);

    }

    public SfPopup PopupView => popup;

    public SfPopup TopUpPopupView => topUpPopup;

    public void UpdateTopUpPopupWidth(double availableWidth)
    {
        var compact = availableWidth < 620;
        var width = compact ? Math.Max(320, availableWidth - 24) : Math.Clamp(availableWidth - 32, 480, 720);
        topUpPopup.WidthRequest = width;
        topUpPopup.AutoSizeMode = PopupAutoSizeMode.Height;
        topUpContent?.SetCompact(compact);
        availableTopUpWidth = availableWidth;
        popup.WidthRequest = Math.Clamp(availableWidth - 32, 320, 960);
    }

    protected void ShowDemoPopup(string title, EsimProfile? profile = null)
    {
        popupContent.Children.Clear();
        popupContent.Children.Add(new Label { Text = "SIMULATED · No payment or eSIM provisioning", FontSize = 12, TextColor = Color.FromArgb("#8B5B00") });
        popupContent.Children.Add(new Label { Text = title, FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#102B52") });
        if (title.StartsWith("Activate", StringComparison.Ordinal) && profile is { Status: ProfileStatus.ReadyToInstall } readyProfile)
        {
            popupContent.Children.Add(new Label { Text = $"{readyProfile.ProfileName} · {readyProfile.Country}", TextColor = Color.FromArgb("#63748B") });
            popupContent.Children.Add(new Label { Text = "This sample action changes only the in-memory demo profile status. It does not install an eSIM.", TextColor = Color.FromArgb("#8B5B00") });
            var activate = new Syncfusion.Maui.Buttons.SfButton { Text = "Activate demo profile" };
            activate.Clicked += (_, _) => { ViewModel.Store.Activate(readyProfile); popup.Dismiss(); };
            popupContent.Children.Add(activate);
            var cancelActivation = new Syncfusion.Maui.Buttons.SfButton { Text = "Cancel" };
            cancelActivation.Clicked += (_, _) => popup.Dismiss();
            popupContent.Children.Add(cancelActivation);
            popup.Show();
            return;
        }
        if (title.StartsWith("Purchase", StringComparison.Ordinal) && ViewModel.SelectedPlan is { } plan)
        {
            popupContent.Children.Add(new Label { Text = $"{plan.Country} · {plan.Provider} · {plan.DataLabel} · {plan.ValidityDays} days", TextColor = Color.FromArgb("#63748B") });
            popupContent.Children.Add(new Label { Text = $"Plan: ${plan.Price:0.00}  |  Illustrative tax: ${ViewModel.SelectedPlanTax:0.00}  |  Total: ${ViewModel.SelectedPlanTotal:0.00} USD", FontAttributes = FontAttributes.Bold });
            var confirm = new Syncfusion.Maui.Buttons.SfButton { Text = "Confirm demo purchase" };
            confirm.Clicked += (_, _) => { ViewModel.Store.Purchase(plan); popup.Dismiss(); _ = Shell.Current.GoToAsync("//profiles"); };
            popupContent.Children.Add(confirm);
            var cancel = new Syncfusion.Maui.Buttons.SfButton { Text = "Cancel" };
            cancel.Clicked += (_, _) => popup.Dismiss();
            popupContent.Children.Add(cancel);
            popup.Show();
            return;
        }
        popupProfile = profile ?? ViewModel.SelectedProfile;
        BuildQrPopup();
        popup.Show();
    }

    private void BuildQrPopup()
    {
        var profile = popupProfile;
        var activationCode = profile?.ActivationCode ?? "LPA:1$demo.carrier.example$SAMPLE";
        var readyToInstall = profile?.Status == ProfileStatus.ReadyToInstall;
        var ink = Color.FromArgb("#0B1C30");
        var muted = Color.FromArgb("#434656");
        var primary = Color.FromArgb("#0046CC");
        var surfaceLow = Color.FromArgb("#EFF4FF");
        var outline = Color.FromArgb("#C3C5D9");

        popupContent.Children.Clear();
        popupContent.Spacing = 16;
        popupContent.Padding = new Thickness(24, 18);

        popupContent.Children.Add(new BoxView
        {
            HeightRequest = 5,
            Color = Color.FromArgb("#0D5CFF"),
            HorizontalOptions = LayoutOptions.Fill
        });

        var headingIcon = new Border
        {
            WidthRequest = 50,
            HeightRequest = 50,
            BackgroundColor = surfaceLow,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            Content = new Label
            {
                Text = "▦",
                FontFamily = "MaterialAssets",
                FontSize = 25,
                TextColor = primary,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var titleBlock = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        titleBlock.Children.Add(new Label
        {
            Text = "Install & Activate eSIM",
            FontFamily = "Inter",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = ink
        });
        titleBlock.Children.Add(new Label
        {
            Text = "Quick setup configuration for instant cross-border high-speed routing",
            FontFamily = "Inter",
            FontSize = 13,
            TextColor = muted
        });

        var status = new Border
        {
            BackgroundColor = Color.FromArgb(readyToInstall ? "#ACEDFF" : "#EFF4FF"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Padding = new Thickness(10, 5),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = readyToInstall ? "● Ready to Install" : profile?.Status == ProfileStatus.Active ? "● Active Profile" : "● Demo QR",
                FontFamily = "Inter",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(readyToInstall ? "#006172" : "#434656")
            }
        };

        var close = new SfButton
        {
            Text = "×",
            FontFamily = "Inter",
            FontSize = 25,
            TextColor = ink,
            Background = surfaceLow,
            WidthRequest = 44,
            HeightRequest = 44,
            CornerRadius = 12,
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Center
        };
        close.Clicked += (_, _) => popup.Dismiss();

        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 14
        };
        header.Add(headingIcon, 0, 0);
        header.Add(titleBlock, 1, 0);
        header.Add(status, 2, 0);
        header.Add(close, 3, 0);
        popupContent.Children.Add(header);

        var profileName = profile?.ProfileName ?? "Japan 5G Explorer";
        var country = profile?.Country ?? "Japan";
        var carrier = profile?.Carrier ?? "Demo Carrier";
        var profileDetails = new VerticalStackLayout { Spacing = 6 };
        var profileLine = new HorizontalStackLayout { Spacing = 7, VerticalOptions = LayoutOptions.Center };
        profileLine.Children.Add(new Label
        {
            Text = country.Equals("Japan", StringComparison.OrdinalIgnoreCase) ? "🇯🇵" : "◉",
            FontSize = 17,
            VerticalOptions = LayoutOptions.Center
        });
        profileLine.Children.Add(new Label
        {
            Text = profileName,
            FontFamily = "Inter",
            FontSize = 19,
            FontAttributes = FontAttributes.Bold,
            TextColor = ink,
            VerticalOptions = LayoutOptions.Center
        });
        profileLine.Children.Add(new Label
        {
            Text = "●",
            FontSize = 9,
            TextColor = outline,
            VerticalOptions = LayoutOptions.Center
        });
        profileDetails.Children.Add(profileLine);
        profileDetails.Children.Add(new Label
        {
            Text = $"{carrier} · ({(profile?.Network == NetworkType.FiveG ? "5G" : "4G / LTE")} Multi-IMSI)",
            FontFamily = "Inter",
            FontSize = 13,
            TextColor = ink
        });

        var tier = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = outline,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
            Padding = new Thickness(10, 5),
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = $"◉  {profile?.AllocatedGb ?? 10:0.#} GB  ·  {(readyToInstall ? Math.Max(1, profile?.DaysRemaining ?? 30) : 30)} Days Global Tier",
                FontFamily = "OpenSansRegular",
                FontSize = 12,
                TextColor = Color.FromArgb("#006172")
            }
        };
        profileDetails.Children.Add(tier);
        popupContent.Children.Add(new Border
        {
            BackgroundColor = surfaceLow,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(16, 12),
            Content = profileDetails
        });

        var scanHeading = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        scanHeading.Add(new Label
        {
            Text = "◉  Instant Scan",
            FontFamily = "Inter",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = ink,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);
        scanHeading.Add(new Label
        {
            Text = $"EID: {profile?.Iccid ?? "DEMO-0000"}",
            FontFamily = "OpenSansRegular",
            FontSize = 12,
            TextColor = muted,
            LineBreakMode = LineBreakMode.TailTruncation,
            VerticalOptions = LayoutOptions.Center
        }, 1, 0);

        var barcode = new SfBarcodeGenerator
        {
            ShowText = false,
            HeightRequest = 220,
            WidthRequest = 220,
            HorizontalOptions = LayoutOptions.Center,
            Symbology = new QRCode(),
            Value = activationCode
        };
        var scanPanel = new VerticalStackLayout { Spacing = 12 };
        scanPanel.Children.Add(scanHeading);
        scanPanel.Children.Add(new Border
        {
            BackgroundColor = Colors.White,
            Stroke = outline,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(12),
            HorizontalOptions = LayoutOptions.Center,
            Content = barcode
        });
        scanPanel.Children.Add(new Label
        {
            Text = "Scan this QR code with your device camera or in Settings > Cellular > Add eSIM.",
            FontFamily = "Inter",
            FontSize = 11,
            TextColor = ink,
            HorizontalTextAlignment = TextAlignment.Center
        });
        scanPanel.Children.Add(new Label
        {
            Text = "●  Single-use cryptographic profile token · Demo only, cannot provision an eSIM.",
            FontFamily = "OpenSansRegular",
            FontSize = 10,
            TextColor = muted,
            HorizontalTextAlignment = TextAlignment.Center
        });
        var scanCard = new Border
        {
            BackgroundColor = surfaceLow,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Padding = new Thickness(16),
            Content = scanPanel
        };

        var steps = new VerticalStackLayout { Spacing = 10 };
        steps.Children.Add(new Label
        {
            Text = "ACTIVATION STEPS",
            FontFamily = "Inter",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = ink
        });
        steps.Children.Add(ActivationStep("1", "Scan QR Code in Device Settings", "Navigate to Settings > Cellular (iOS) or SIM Manager (Android) and add plan."));
        steps.Children.Add(ActivationStep("2", "Enable 'Data Roaming' upon Landing", "Turn on roaming toggles specifically for this profile once in Japan."));
        steps.Children.Add(ActivationStep("3", "Set Profile as Cellular Data Carrier", "Keep your primary home SIM for SMS 2FA while using this eSIM for all data."));

        var manualActivation = new VerticalStackLayout { Spacing = 8 };
        var manualHeader = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        manualHeader.Add(new Label
        {
            Text = "Manual Activation Parameters",
            FontFamily = "Inter",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = ink,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);
        manualHeader.Add(new Border
        {
            BackgroundColor = surfaceLow,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Padding = new Thickness(8, 4),
            Content = new Label { Text = "RSP Protocol v2.2", FontFamily = "OpenSansRegular", FontSize = 10, TextColor = Color.FromArgb("#006141") }
        }, 1, 0);
        manualActivation.Children.Add(manualHeader);
        manualActivation.Children.Add(ActivationParameter("SM-DP+ Address", "rsp.global-esim.net", ink));
        manualActivation.Children.Add(ActivationParameter("Activation Code (LPA String)", activationCode, primary));
        var copyFullCode = new SfButton
        {
            Text = "▣  Copy Full Activation String",
            FontFamily = "Inter",
            FontSize = 12,
            TextColor = primary,
            Background = surfaceLow,
            CornerRadius = 8,
            Padding = new Thickness(12, 8)
        };
        copyFullCode.Clicked += async (_, _) => await Clipboard.Default.SetTextAsync(activationCode);
        manualActivation.Children.Add(copyFullCode);
        steps.Children.Add(new Border
        {
            BackgroundColor = Colors.White,
            Stroke = outline,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(12),
            Content = manualActivation
        });

        if (popup.WidthRequest < 620)
        {
            popupContent.Children.Add(scanCard);
            popupContent.Children.Add(steps);
        }
        else
        {
            var body = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 20
            };
            body.Add(scanCard, 0, 0);
            body.Add(steps, 1, 0);
            popupContent.Children.Add(body);
        }

        var compatibility = new Border
        {
            BackgroundColor = surfaceLow,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(9) },
            Padding = new Thickness(10, 8),
            Content = new Label
            {
                Text = "▯  Compatibility: Unlocked iPhone XS or newer, Pixel 3 or later, Galaxy S20+ and compatible dual-SIM devices.",
                FontFamily = "Inter",
                FontSize = 11,
                TextColor = muted
            }
        };
        steps.Children.Add(compatibility);

        var footer = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8,
            Padding = new Thickness(0, 8, 0, 0)
        };
        var iccid = new Label
        {
            Text = $"◉  ICCID: {FormatIccid(profile?.Iccid ?? "DEMO-0000")}",
            FontFamily = "OpenSansRegular",
            FontSize = 10,
            TextColor = muted,
            VerticalOptions = LayoutOptions.Center
        };
        var sendEmail = new SfButton
        {
            Text = "✉  Send QR to Email",
            FontFamily = "Inter",
            FontSize = 12,
            TextColor = ink,
            Background = Colors.White,
            Stroke = outline,
            StrokeThickness = 1,
            CornerRadius = 9,
            Padding = new Thickness(12, 8)
        };
        sendEmail.Clicked += async (_, _) =>
        {
            try
            {
                await Email.Default.ComposeAsync(new EmailMessage("Your eSIM activation details", $"Demo QR only — this sample cannot provision an eSIM.\n\nActivation string: {activationCode}"));
            }
            catch (FeatureNotSupportedException)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert("Email unavailable", "Email composition is not available on this device.", "OK");
            }
        };
        var scanned = new SfButton
        {
            Text = "◉  I've Scanned the QR Code",
            FontFamily = "Inter",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            Background = primary,
            CornerRadius = 9,
            Padding = new Thickness(14, 8)
        };
        scanned.Clicked += async (_, _) =>
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Demo only", "This sample does not install or activate an eSIM on your device.", "OK");
            popup.Dismiss();
        };
        footer.Add(iccid, 0, 0);
        footer.Add(sendEmail, 1, 0);
        footer.Add(scanned, 2, 0);
        popupContent.Children.Add(new Border
        {
            BackgroundColor = Color.FromArgb("#F8F9FF"),
            StrokeThickness = 0,
            Padding = new Thickness(12, 8),
            Content = footer
        });
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
            FontFamily = "OpenSansRegular",
            FontSize = 11,
            TextColor = valueColor,
            LineBreakMode = LineBreakMode.TailTruncation,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);
        var copy = new SfButton
        {
            Text = "▢",
            FontFamily = "Inter",
            FontSize = 12,
            TextColor = Color.FromArgb("#434656"),
            Background = Colors.Transparent,
            WidthRequest = 28,
            HeightRequest = 28,
            Padding = new Thickness(0),
            CornerRadius = 6
        };
        copy.Clicked += async (_, _) => await Clipboard.Default.SetTextAsync(value);
        valueRow.Add(copy, 1, 0);

        var stack = new VerticalStackLayout { Spacing = 3 };
        stack.Children.Add(new Label { Text = label, FontFamily = "Inter", FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#434656") });
        stack.Children.Add(new Border
        {
            BackgroundColor = Color.FromArgb("#EFF4FF"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(7) },
            Padding = new Thickness(8, 3),
            Content = valueRow
        });
        return stack;
    }

    private static string FormatIccid(string iccid)
    {
        var digits = new string(iccid.Where(char.IsDigit).ToArray());
        return string.Join(' ', Enumerable.Range(0, (digits.Length + 3) / 4).Select(i => digits.Substring(i * 4, Math.Min(4, digits.Length - i * 4))));
    }

    private static Border ActivationStep(string number, string title, string description)
    {
        var numberBadge = new Border
        {
            WidthRequest = 30,
            HeightRequest = 30,
            BackgroundColor = Color.FromArgb("#0046CC"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(999) },
            Content = new Label
            {
                Text = number,
                FontFamily = "Inter",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };
        var text = new VerticalStackLayout { Spacing = 2 };
        text.Children.Add(new Label
        {
            Text = title,
            FontFamily = "Inter",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#0B1C30")
        });
        text.Children.Add(new Label
        {
            Text = description,
            FontFamily = "Inter",
            FontSize = 11,
            TextColor = Color.FromArgb("#434656"),
            LineBreakMode = LineBreakMode.WordWrap
        });
        var row = new HorizontalStackLayout { Spacing = 10, Children = { numberBadge, text } };
        return new Border
        {
            BackgroundColor = Color.FromArgb("#EFF4FF"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(11) },
            Padding = new Thickness(12),
            Content = row
        };
    }

    public void QrActiveClicked(object? sender, EventArgs e) => ShowDemoPopup("Install & Activate eSIM", ViewModel.ActiveProfile);
    public async void TopUpActiveClicked(object? sender, EventArgs e) => await OpenTopUp(ViewModel.ActiveProfile);
    public void QrItemClicked(object? sender, EventArgs e) => ShowDemoPopup("Install & Activate eSIM", (sender as Element)?.BindingContext as EsimProfile);
    public async void TopUpItemClicked(object? sender, EventArgs e) => await OpenTopUp((sender as Element)?.BindingContext as EsimProfile);

    private async Task OpenTopUp(EsimProfile? profile)
    {
        if (profile is null || profile.Status != ProfileStatus.Active) { await ViewModel.TopUpCommand.ExecuteAsync(profile); return; }
        popupProfile = profile;
        ViewModel.TopUpProfile = profile;
        ViewModel.SelectedTopUpGb = 3;

        topUpContent?.Dispose();
        topUpContent = new TopUpPopupContent(ViewModel);
        topUpContent.CloseRequested += (_, _) => CloseTopUp();
        topUpContent.CancelRequested += (_, _) => CloseTopUp();
        topUpContent.ChangePaymentRequested += (_, _) => ChangePaymentRequested();
        topUpContent.PurchaseRequested += (_, _) => ConfirmTopUp(profile);

        topUpPopup.ContentTemplate = new DataTemplate(() => topUpContent!.Content);
        UpdateTopUpPopupWidth(availableTopUpWidth);
        topUpPopup.Show();
    }

    private void CloseTopUp()
    {
        topUpPopup.Dismiss();
        topUpContent?.Dispose();
        topUpContent = null;
    }

    private async void ConfirmTopUp(EsimProfile profile)
    {
        var pack = ViewModel.SelectedTopUpPack;
        var before = profile.RemainingGb;
        if (!ViewModel.ApplyTopUp()) return;
        CloseTopUp();
        await Application.Current!.Windows[0].Page!.DisplayAlert(
            "Top-up added",
            $"{pack.DataLabel} was added to {profile.ProfileName}. Remaining data is now {profile.RemainingGb:0.#} GB (was {before:0.#} GB).\n\nDemo only — no payment was taken and no carrier was contacted.",
            "OK");
    }

    private async void ChangePaymentRequested()
    {
        await Application.Current!.Windows[0].Page!.DisplayAlert(
            "Demo payment method",
            "The sample top-up always uses the demo card ending in 4242. No real payment details are collected and no card is charged.",
            "OK");
    }
    public void ActivateItemClicked(object? sender, EventArgs e) => ShowDemoPopup("Activate Profile · Demo", (sender as Element)?.BindingContext as EsimProfile);
    public async void RenewItemClicked(object? sender, EventArgs e) => await ViewModel.RenewCommand.ExecuteAsync((sender as Element)?.BindingContext as EsimProfile);
    public void QrSelectedClicked(object? sender, EventArgs e) => ShowDemoPopup("Install & Activate eSIM", ViewModel.SelectedProfile);
    public void BuyPlanClicked(object? sender, EventArgs e)
    {
        ViewModel.SelectedPlan = (sender as Element)?.BindingContext as Plan;
        ShowDemoPopup("Purchase Plan · Demo checkout");
    }
    public void DestinationClicked(object? sender, EventArgs e) { var text = sender switch { Syncfusion.Maui.Core.SfChip chip => chip.Text, Syncfusion.Maui.Buttons.SfButton button => button.Text, _ => null }; if (!string.IsNullOrWhiteSpace(text)) { var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries); ViewModel.SelectCountry(words.Length > 1 ? string.Join(' ', words.Skip(1)) : words[0]); } }
    public void FilterClicked(object? sender, EventArgs e) { if (sender is Syncfusion.Maui.Buttons.SfButton button && button.Text is not null) ViewModel.SelectCoverage(button.Text); }
    public void NetworkFilterSelected(object? sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) { if (e.AddedItems.FirstOrDefault() is string value) ViewModel.SetNetworkFilter(value); }
    public void QuotaFilterSelected(object? sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) { if (e.AddedItems.FirstOrDefault() is string value) ViewModel.SetQuotaFilter(value); }
    public void DurationFilterSelected(object? sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) { if (e.AddedItems.FirstOrDefault() is string value) ViewModel.SetDurationFilter(value); }
    public void SortFilterSelected(object? sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) { if (e.AddedItems.FirstOrDefault() is string value) ViewModel.SetSortFilter(value); }
}
