using eSIMDataManager.Controls;
using eSIMDataManager.Models;
using eSIMDataManager.ViewModels;
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
        WidthRequest = 700,
        StaysOpen = true,
        AutoSizeMode = PopupAutoSizeMode.Height,
        PopupStyle = new PopupStyle
        {
            PopupBackground = Colors.White,
            MessageBackground = Colors.White,
            CornerRadius = 10
        }
    };
    private readonly VerticalStackLayout popupContent = new() { Spacing = 14, Padding = 22 };
    private readonly SfPopup qrPopup = new()
    {
        ShowHeader = false,
        ShowFooter = false,
        WidthRequest = 700,
        StaysOpen = true,
        AutoSizeMode = PopupAutoSizeMode.Height,
        PopupStyle = new PopupStyle
        {
            PopupBackground = Colors.White,
            MessageBackground = Colors.White,
            CornerRadius = 10
        }
    };
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
    private QrPopupContent? qrContent;
    private double availableTopUpWidth = 720;
    private double availableHeight;
    private EsimProfile? popupProfile;

    public DemoPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        popup.ContentTemplate = new DataTemplate(() => popupContent);

    }

    public SfPopup PopupView => popup;

    public SfPopup TopUpPopupView => topUpPopup;

    public SfPopup QrPopupView => qrPopup;

    public void UpdateTopUpPopupWidth(double availableWidth, double availableHeight = 0)
    {
        var compact = availableWidth < 620;
        var width = compact ? Math.Max(320, availableWidth - 24) : Math.Clamp(availableWidth - 32, 480, 720);
        topUpPopup.WidthRequest = width;
        topUpPopup.AutoSizeMode = PopupAutoSizeMode.Height;
        topUpContent?.SetCompact(compact);
        availableTopUpWidth = availableWidth;

        if (availableHeight > 0) this.availableHeight = availableHeight;
        popup.WidthRequest = Math.Clamp(availableWidth - 32, 320, 700);
        if (this.availableHeight > 0)
        {
            // Size to content but never taller than the screen, so the footer stays visible
            // and the body scrolls instead of overflowing.
            popup.AutoSizeMode = PopupAutoSizeMode.Height;
            popup.MaximumHeightRequest = Math.Max(360, this.availableHeight - 24);
        }
        qrPopup.WidthRequest = popup.WidthRequest;
        if (this.availableHeight > 0)
        {
            // Let the dialog hug its content but never grow past the screen; the scrolling
            // body inside absorbs the difference, so the footer is always visible.
            qrPopup.AutoSizeMode = PopupAutoSizeMode.Height;
            qrPopup.MaximumHeightRequest = Math.Max(360, this.availableHeight - 24);
        }
        qrContent?.SetCompact(qrPopup.WidthRequest < 620);
        if (this.availableHeight > 0)
            qrContent?.SetViewport(this.availableHeight);
    }

    protected void ShowDemoPopup(string title, EsimProfile? profile = null)
    {
        popupContent.Children.Clear();
        popup.ContentTemplate = new DataTemplate(() => popupContent);
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
        qrContent = new QrPopupContent(popupProfile);
        qrContent.ScannedRequested += (_, _) => qrPopup.Dismiss();
        qrContent.CloseRequested += (_, _) => qrPopup.Dismiss();
        qrPopup.ContentTemplate = new DataTemplate(() => qrContent!.Content);
        qrContent.SetCompact(qrPopup.WidthRequest < 620);
        qrContent.SetViewport(availableHeight);
        qrPopup.Show();
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
