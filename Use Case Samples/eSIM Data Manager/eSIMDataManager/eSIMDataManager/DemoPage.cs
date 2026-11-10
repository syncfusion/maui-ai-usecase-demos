using eSIMDataManager.Models;
using eSIMDataManager.ViewModels;
using Syncfusion.Maui.Barcode;
using Syncfusion.Maui.Popup;

namespace eSIMDataManager;

public class DemoPage
{
    private MainViewModel ViewModel { get; }

    private readonly SfPopup popup = new() { ShowHeader = false, ShowFooter = false, WidthRequest = 580, HeightRequest = 650, StaysOpen = true };
    private readonly VerticalStackLayout popupContent = new() { Spacing = 14, Padding = 22 };
    private EsimProfile? popupProfile;

    public DemoPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        popup.ContentTemplate = new DataTemplate(() => popupContent);

    }

    public SfPopup PopupView => popup;

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
        popupContent.Children.Add(new Label { Text = popupProfile?.ActivationCode ?? "Fictional activation details", FontSize = 12, TextColor = Color.FromArgb("#63748B"), LineBreakMode = LineBreakMode.WordWrap });
        popupContent.Children.Add(new SfBarcodeGenerator { ShowText = false, HeightRequest = 220, WidthRequest = 220, HorizontalOptions = LayoutOptions.Center, Symbology = new QRCode() });
        popupContent.Children.Add(new Label { Text = "This QR is a fictional visual sample and cannot activate or provision an eSIM.", FontSize = 12, TextColor = Color.FromArgb("#63748B"), HorizontalTextAlignment = TextAlignment.Center });
        var close = new Syncfusion.Maui.Buttons.SfButton { Text = "Close" };
        close.Clicked += (_, _) => popup.Dismiss();
        popupContent.Children.Add(close);
        ((SfBarcodeGenerator)popupContent.Children[3]).Value = popupProfile?.ActivationCode ?? "LPA:1$demo.carrier.example$SAMPLE";
        popup.Show();
    }

    public async void DashboardClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//dashboard");
    public async void ProfilesClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//profiles");
    public async void StoreClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//store");
    public async void OrdersClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//orders");
    public void QrActiveClicked(object? sender, EventArgs e) => ShowDemoPopup("Install & Activate eSIM", ViewModel.ActiveProfile);
    public async void TopUpActiveClicked(object? sender, EventArgs e) => await OpenTopUp(ViewModel.ActiveProfile);
    public void QrItemClicked(object? sender, EventArgs e) => ShowDemoPopup("Install & Activate eSIM", (sender as Element)?.BindingContext as EsimProfile);
    public async void TopUpItemClicked(object? sender, EventArgs e) => await OpenTopUp((sender as Element)?.BindingContext as EsimProfile);

    private async Task OpenTopUp(EsimProfile? profile)
    {
        if (profile is null || profile.Status != ProfileStatus.Active) { await ViewModel.TopUpCommand.ExecuteAsync(profile); return; }
        popupProfile = profile;
        popupContent.Children.Clear();
        popupContent.Children.Add(new Label { Text = "SIMULATED · No payment or carrier provisioning", FontSize = 12, TextColor = Color.FromArgb("#8B5B00") });
        popupContent.Children.Add(new Label { Text = "Top-Up Data · Simulated", FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#102B52") });
        popupContent.Children.Add(new Label { Text = $"{profile.ProfileName} · {profile.RemainingGb:0.#} GB remaining · demo", TextColor = Color.FromArgb("#63748B") });
        popupContent.Children.Add(new Label { Text = "Choose a fictional data extension", TextColor = Color.FromArgb("#63748B") });
        foreach (var pack in ViewModel.TopUpPacks)
        {
            var option = new Syncfusion.Maui.Buttons.SfButton { Text = $"{pack.DataGb} GB · ${pack.Price:0.00} · simulated" };
            option.Clicked += (_, _) => { ViewModel.Store.TopUp(profile, pack); popup.Dismiss(); };
            popupContent.Children.Add(option);
        }
        var close = new Syncfusion.Maui.Buttons.SfButton { Text = "Cancel" };
        close.Clicked += (_, _) => popup.Dismiss();
        popupContent.Children.Add(close);
        popup.Show();
    }
    public void ActivateItemClicked(object? sender, EventArgs e) => ShowDemoPopup("Activate Profile · Demo", (sender as Element)?.BindingContext as EsimProfile);
    public async void RenewItemClicked(object? sender, EventArgs e) => await ViewModel.RenewCommand.ExecuteAsync((sender as Element)?.BindingContext as EsimProfile);
    public void QrSelectedClicked(object? sender, EventArgs e) => ShowDemoPopup("Install & Activate eSIM", ViewModel.SelectedProfile);
    public void BuyPlanClicked(object? sender, EventArgs e)
    {
        ViewModel.SelectedPlan = (sender as Element)?.BindingContext as Plan;
        ShowDemoPopup("Purchase Plan · Demo checkout");
    }
    public void DestinationClicked(object? sender, EventArgs e) { if (sender is Button button && button.Text is not null) ViewModel.SelectCountry(button.Text); }
    public void FilterClicked(object? sender, EventArgs e) { if (sender is Button button && button.Text is not null) ViewModel.ToggleFilter(button.Text); }
}
