using eSIMDataManager.Models;
using eSIMDataManager.ViewModels;
using Syncfusion.Maui.Gauges;

namespace eSIMDataManager;

public sealed class MobilePage : ContentView
{
    public enum MobileTab { Dashboard, Profiles, Store }
    private readonly MainViewModel vm;
    private readonly MobileTab tab;
    private readonly DemoPage actions;
    private readonly Grid root = new() { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
    private readonly ScrollView scroll = new() { VerticalScrollBarVisibility = ScrollBarVisibility.Never };

    private readonly Action<MobileTab> navigate;

    public MobilePage(MainViewModel viewModel, MobileTab selectedTab, Action<MobileTab>? navigate = null)
    {
        vm = viewModel;
        tab = selectedTab;
        this.navigate = navigate ?? (target =>
        {
            var shell = Shell.Current;
            if (shell is null)
                return;
            _ = shell.GoToAsync(target switch
            {
                MobileTab.Dashboard => "//dashboard",
                MobileTab.Profiles => "//profiles",
                _ => "//store"
            });
        });
        actions = new DemoPage(vm);
        //Title = string.Empty;
        BackgroundColor = Color.FromArgb("#F6F8FF");
        scroll.Content = PageContent();
        vm.Store.Changed += (_, _) => MainThread.BeginInvokeOnMainThread(() => scroll.Content = PageContent());
        root.Add(scroll, 0, 0);
        root.Add(BottomNavigation(), 0, 1);
        Grid.SetRowSpan(actions.PopupView, 2);
        Grid.SetRowSpan(actions.QrPopupView, 2);
        Grid.SetRowSpan(actions.TopUpPopupView, 2);
        Grid.SetRowSpan(actions.PurchasePopupView, 2);
        root.Children.Add(actions.PopupView);
        root.Children.Add(actions.QrPopupView);
        root.Children.Add(actions.TopUpPopupView);
        root.Children.Add(actions.PurchasePopupView);
        Content = root;
        SizeChanged += (_, _) => actions.UpdateTopUpPopupWidth(Width, Height);
    }

    private View PageContent() => tab switch
    {
        MobileTab.Dashboard => DashboardContent(),
        MobileTab.Profiles => ProfilesContent(),
        _ => StoreContent()
    };

    private View DashboardContent()
    {
        var stack = PageStack();
        stack.Add(new VerticalStackLayout { Spacing = 3, Children = { new Label { Text = "◆  ROAMING IN TOKYO, JAPAN", FontFamily = "Inter", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#00687A") }, new Label { Text = "Good morning, Alex", FontFamily = "Inter", FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35") } } });
        var plan = new Border
        {
            Padding = new Thickness(18, 20),
            Background = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Color.FromArgb("#FFFFFF"), 0), new GradientStop(Color.FromArgb("#F0FBFF"), 1) }, new Point(0, 0), new Point(1, 1)),
            Stroke = Color.FromArgb("#E8EDF7"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 },
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
                        ColumnSpacing = 8,
                        Children =
                        {
                            new Border { WidthRequest = 54, HeightRequest = 54, Content = Text("JP", 23, "#101D35"), BackgroundColor = Color.FromArgb("#E5EEFF"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 } },
                            new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { Text("Japan Unlimited", 19, "#101D35", true), new HorizontalStackLayout { Spacing = 4, Children = { Text("SoftBank Corp.", 13, "#26334B"), Text("•", 12, "#AAB6CB"), new Label { Text = "18ms ping", FontFamily = "monospace", FontSize = 12, TextColor = Color.FromArgb("#007C91"), VerticalOptions = LayoutOptions.Center } } } } }.SetColumn(1),
                            new Border { Content = Text("5G+", 13, "#0752DB", true), HeightRequest = 28, Padding = new Thickness(10, 0), BackgroundColor = Color.FromArgb("#E1EBFF"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 19 }, VerticalOptions = LayoutOptions.Start }.SetColumn(2),
                            new Border { Content = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center, Children = { new BoxView { WidthRequest = 9, HeightRequest = 9, CornerRadius = 5, Color = Color.FromArgb("#007A56"), VerticalOptions = LayoutOptions.Center }, Text("Connected", 13, "#006141", true) } }, HeightRequest = 28, Padding = new Thickness(10, 0), BackgroundColor = Color.FromArgb("#D6F2ED"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 19 }, VerticalOptions = LayoutOptions.Start }.SetColumn(3)
                        }
                    },
                    new Grid
                    {
                        HeightRequest = 260,
                        MinimumHeightRequest = 260,
                        WidthRequest = 260,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            CreateUsageGauge(),
                            new VerticalStackLayout
                            {
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                Spacing = 2,
                                Children =
                                {
                                    Text("HIGH SPEED", 14, "#46516A"),
                                    new HorizontalStackLayout { Spacing = 3, HorizontalOptions = LayoutOptions.Center, Children = { Text(vm.Remaining.ToString("0.#"), 40, "#101D35", true), Text("GB", 21, "#0752DB", true) } },
                                    Text($"Remaining of {vm.Allocation:0.#} GB", 14, "#007C91", true),
                                    new Border { Content = new HorizontalStackLayout { Spacing = 6, Children = { Text("ϟ", 17, "#008B78", true), Text("~0.8 GB/day pace", 14, "#26334B") } }, Padding = new Thickness(10, 4), BackgroundColor = Color.FromArgb("#EEF3FF"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 }, Margin = new Thickness(0, 7, 0, 0) }
                                }
                            }
                        }
                    },
                    new Grid
                    {
                        ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                        ColumnSpacing = 8,
                        Children =
                        {
                            DashboardMetric("\uE74C", "#E0E9FF", "#0752DB", "Quota Status", $"{vm.AvailablePercent:0}% Available"),
                            DashboardMetric("\uE725", "#D9ECF4", "#007C91", "Backup Reserve", "Unlimited\n256kbps").SetColumn(1)
                        }
                    },
                    new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { new HorizontalStackLayout { Spacing = 8, Children = { new Label { Text = "\uE70D", FontFamily = "UiKitIcons", FontSize = 21, TextColor = Color.FromArgb("#0752DB"), VerticalOptions = LayoutOptions.Center }, Text($"{vm.DaysRemaining} Days Left", 25, "#101D35", true) } }, Pill("Auto-renew ON").SetColumn(1) } },
                            new ProgressBar { Progress = Math.Clamp(vm.DaysRemaining / 30.0,0,1), ProgressColor = Color.FromArgb("#0649D5"), BackgroundColor = Color.FromArgb("#DFE9FC"), HeightRequest = 7 },
                            new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { new Label { Text = $"Started {vm.ActiveProfile?.ActivationDate?.ToString("MMM d") ?? "—"}", FontFamily = "Inter", FontSize = 14, TextColor = Color.FromArgb("#46516A"), HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center }.SetColumn(0), new Label { Text = $"Expires {vm.ActiveProfile?.ExpiryDate.ToString("MMM d") ?? "—"}, 23:59 JST", FontFamily = "Inter", FontSize = 14, TextColor = Color.FromArgb("#46516A"), HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center }.SetColumn(1) } }
                        }
                    }
                }
            }
        };
        stack.Add(plan);
        var actionsGrid = new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star)}, ColumnSpacing=14 };
        actionsGrid.Add(DashboardActionButton("\uE721", "Buy Plan", true, () => navigate(MobileTab.Store)), 0, 0);
        actionsGrid.Add(DashboardActionButton("\uE707", "Top Up", false, () => actions.TopUpActiveClicked(this, EventArgs.Empty)), 1, 0);
        actionsGrid.Add(DashboardActionButton("\uE712", "Show QR", false, () => actions.QrActiveClicked(this, EventArgs.Empty)), 2, 0);
        stack.Add(actionsGrid);
        stack.Add(Card(UsageCard()));
        stack.Add(RecentActivityCard());
        return stack;
    }

    private View ProfilesContent()
    {
        var stack = PageStack();
        var titleRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        titleRow.Add(new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { Text("My eSIMs", 28, "#101D35", true), Text("Manage profiles & data roaming", 13, "#46516A") } });
        titleRow.Add(RoundIconButton("\uE715", "MaterialAssets", "#0649D5", () => { }), 1, 0);
        titleRow.Add(RoundIconButton("\uE70D", "MaterialAssets", "#0649D5", () => navigate(MobileTab.Store)), 2, 0);
        stack.Add(titleRow);
        var filters = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 6, BackgroundColor = Color.FromArgb("#E7EEFF"), Padding = 5 };
        filters.Add(FilterLabel($"Active  {vm.ActiveProfiles.Count}", true), 0, 0);
        filters.Add(FilterLabel($"Ready  {vm.ReadyProfiles.Count}"), 1, 0);
        filters.Add(FilterLabel($"Expired  {vm.ExpiredProfiles.Count}"), 2, 0);
        stack.Add(filters);
        foreach (var profile in vm.ActiveProfiles.Concat(vm.ReadyProfiles).Concat(vm.ExpiredProfiles))
        {
            var context = new Border { BindingContext = profile };
            if (profile.Status == ProfileStatus.Active)
            {
                stack.Add(ActiveProfileCard(profile, context));
            }
            else
            {
                var isReady = profile.Status == ProfileStatus.ReadyToInstall;
                Action openAction = isReady ? () => actions.ActivateItemClicked(context, EventArgs.Empty) : () => actions.QrItemClicked(context, EventArgs.Empty);
                var card = Card(CompactProfileRow(profile, isReady));
                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) => openAction();
                card.GestureRecognizers.Add(tap);
                stack.Add(card);
            }
        }
        stack.Add(StoreCallout());
        return stack;
    }

    private View ActiveProfileCard(EsimProfile profile, Border context)
    {
        var countryFlag = profile.Country switch
        {
            "Japan" => "🇯🇵",
            "France" => "🇫🇷",
            "USA" => "🇺🇸",
            "Thailand" => "🇹🇭",
            "Italy" => "🇮🇹",
            _ => "🌐"
        };

        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        header.Add(new Border { WidthRequest = 52, HeightRequest = 52, Content = Text(countryFlag, 24, "#101D35"), BackgroundColor = Color.FromArgb("#EEF2FB"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 } });
        header.Add(new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { Text(profile.ProfileName, 19, "#101D35", true), Text(profile.Carrier, 13, "#46516A") } }, 1, 0);
        header.Add(new Border { Content = new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center, Children = { new BoxView { WidthRequest = 8, HeightRequest = 8, CornerRadius = 4, Color = Color.FromArgb("#00805A"), VerticalOptions = LayoutOptions.Center }, Text("Active", 13, "#006141", true) } }, Padding = new Thickness(11, 6), BackgroundColor = Color.FromArgb("#DDF8EE"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 } }, 2, 0);
        header.Add(Text("⌃", 17, "#46516A", true), 3, 0);

        var stats = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        stats.Add(new Label { Text = "\uE74C", FontFamily = "MaterialAssets", FontSize = 18, TextColor = Color.FromArgb("#0649D5"), VerticalOptions = LayoutOptions.Center }, 0, 0);
        stats.Add(new Label { Text = $"{profile.RemainingGb:0.#} GB Remaining", FontFamily = "Inter", FontSize = 14, TextColor = Color.FromArgb("#0649D5"), FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center }, 1, 0);
        stats.Add(new BoxView { WidthRequest = 4, HeightRequest = 4, CornerRadius = 2, Color = Color.FromArgb("#B4BFD4"), VerticalOptions = LayoutOptions.Center }, 2, 0);
        stats.Add(new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = "\uE70F", FontFamily = "UiKitIcons", FontSize = 16, TextColor = Color.FromArgb("#26334B"), VerticalOptions = LayoutOptions.Center }, new Label { Text = $"{profile.DaysRemaining} Days Left", FontFamily = "Inter", FontSize = 14, TextColor = Color.FromArgb("#26334B"), VerticalOptions = LayoutOptions.Center } } }, 3, 0);

        var network = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        network.Add(new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { Text("Country & Network", 13, "#46516A"), new Label { Text = $"{profile.Country}  ·  5G", FontFamily = "Inter", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35") } } });
        network.Add(new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End, Children = { Text("Connected", 14, "#006141", true), Text("(Roaming ON)", 13, "#006141", true) } }, 2, 0);
        var iccid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        iccid.Add(new Label { Text = "ICCID Number", FontFamily = "Inter", FontSize = 13, TextColor = Color.FromArgb("#46516A"), VerticalOptions = LayoutOptions.Center });
        iccid.Add(new Label { Text = FormatIccid(profile.Iccid), FontFamily = "monospace", FontSize = 13, TextColor = Color.FromArgb("#101D35"), VerticalOptions = LayoutOptions.Center }, 1, 0);
        iccid.Add(new Label { Text = "\uE7A0", FontFamily = "MaterialAssets", FontSize = 17, TextColor = Color.FromArgb("#0649D5"), VerticalOptions = LayoutOptions.Center }, 2, 0);

        var balance = new VerticalStackLayout { Spacing = 8 };
        balance.Add(new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { Text("Remaining Balance", 13, "#46516A"), new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.End, Children = { Text($"{profile.UsagePercent:0}% Consumed", 14, "#0649D5", true), Text($"{profile.ConsumedGb:0.#} GB Used", 12, "#26334B") } }.SetColumn(1) } });
        balance.Add(new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.End, Children = { Text($"{profile.RemainingGb:0.#} GB", 34, "#101D35", true), Text($"of {profile.AllocatedGb:0.#} GB Total", 14, "#46516A") } });
        balance.Add(new ProgressBar { Progress = profile.UsageFraction, ProgressColor = Color.FromArgb("#0752DB"), BackgroundColor = Color.FromArgb("#DBE6FA"), HeightRequest = 9 });
        balance.Add(new HorizontalStackLayout { Spacing = 8, Children = { new Label { Text = "\uE716", FontFamily = "MaterialAssets", FontSize = 16, TextColor = Color.FromArgb("#0A6C7C"), VerticalOptions = LayoutOptions.Center }, Text("128 kbps unlimited reserve active", 13, "#00687A") } });

        var validity = new VerticalStackLayout { Spacing = 8 };
        validity.Add(new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { new Label { Text = $"Activated: {profile.ActivationDate:MMM d, yyyy}", FontFamily = "Inter", FontSize = 13, TextColor = Color.FromArgb("#46516A"), VerticalOptions = LayoutOptions.Center }, new Label { Text = $"Expires: {profile.ExpiryDate:MMM d, yyyy}", FontFamily = "Inter", FontSize = 13, TextColor = Color.FromArgb("#46516A"), VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End } } });
        validity.Add(new ProgressBar { Progress = Math.Clamp(1 - profile.DaysRemaining / 14d, 0, 1), ProgressColor = Color.FromArgb("#00687A"), BackgroundColor = Color.FromArgb("#DBE6FA"), HeightRequest = 7 });
        validity.Add(new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { Text("Validity Period", 13, "#46516A"), new Label { Text = $"{profile.DaysRemaining} Days Remaining", FontFamily = "Inter", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0649D5"), HorizontalOptions = LayoutOptions.End } } });

        var activation = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        activation.Add(new Label { Text = "\uE712", FontFamily = "UiKitIcons", FontSize = 40, TextColor = Color.FromArgb("#101D35"), VerticalOptions = LayoutOptions.Center }, 0, 0);
        activation.Add(new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { Text("LPA Activation String", 13, "#46516A"), new Label { Text = profile.ActivationCode, FontFamily = "monospace", FontSize = 13, TextColor = Color.FromArgb("#101D35"), LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 }, Text("Ready for re-installation if needed", 13, "#0649D5") } }, 1, 0);
        activation.Add(new Border { WidthRequest = 44, HeightRequest = 44, Content = new Label { Text = "\uE7A0", FontFamily = "MaterialAssets", FontSize = 18, TextColor = Color.FromArgb("#26334B"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }, BackgroundColor = Colors.White, StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 }, VerticalOptions = LayoutOptions.Center }, 2, 0);

        var primaryActions = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
        primaryActions.Add(ProfileActionButton("\uE70D", "MaterialAssets", "Top Up Data", true, () => actions.TopUpItemClicked(context, EventArgs.Empty)), 0, 0);
        primaryActions.Add(ProfileActionButton("\uE712", "UiKitIcons", "Show QR Code", false, () => actions.QrItemClicked(context, EventArgs.Empty)), 1, 0);
        var secondaryActions = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
        secondaryActions.Add(ProfileActionButton("\uE7A2", "MaterialAssets", "Renew Plan", false, () => actions.RenewItemClicked(context, EventArgs.Empty)), 0, 0);
        secondaryActions.Add(ProfileActionButton("\uE775", "MaterialAssets", "Install Guide", false, () => actions.QrItemClicked(context, EventArgs.Empty)), 1, 0);

        return Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                header,
                stats,
                SoftPanel(new VerticalStackLayout { Spacing = 10, Children = { network, iccid } }),
                SoftPanel(balance),
                SoftPanel(validity),
                SoftPanel(activation),
                primaryActions,
                secondaryActions
            }
        });
    }

    private static string FormatIccid(string iccid)
    {
        if (string.IsNullOrWhiteSpace(iccid) || iccid.StartsWith("DEMO", StringComparison.OrdinalIgnoreCase))
            return iccid;
        var groups = iccid.Select((character, index) => (character, index))
            .Where(item => item.index % 4 == 3 && item.index > 0)
            .Select(item => item.index)
            .Reverse()
            .ToList();
        var formatted = iccid;
        foreach (var index in groups)
            formatted = formatted.Insert(index, " ");
        return formatted;
    }

    private static Border SoftPanel(View content) => new Border { Content = content, Padding = new Thickness(14, 12), BackgroundColor = Color.FromArgb("#EEF3FF"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 } };

    private static View ProfileActionButton(string icon, string fontFamily, string label, bool primary, Action action)
    {
        var content = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = icon, FontFamily = fontFamily, FontSize = 18, TextColor = primary ? Colors.White : Color.FromArgb("#101D35"), VerticalOptions = LayoutOptions.Center },
                new Label { Text = label, FontFamily = "Inter", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = primary ? Colors.White : Color.FromArgb("#101D35"), VerticalOptions = LayoutOptions.Center }
            }
        };
        var button = new Border
        {
            Content = content,
            BackgroundColor = primary ? Color.FromArgb("#0649D5") : Color.FromArgb("#EEF3FF"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            HeightRequest = 52,
            Padding = new Thickness(10, 0)
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => action();
        button.GestureRecognizers.Add(tap);
        return button;
    }

    private static View RoundIconButton(string icon, string fontFamily, string color, Action action)
    {
        var button = new Border
        {
            WidthRequest = 44,
            HeightRequest = 44,
            Content = new Label { Text = icon, FontFamily = fontFamily, FontSize = 21, TextColor = Color.FromArgb(color), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center },
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 22 }
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => action();
        button.GestureRecognizers.Add(tap);
        return button;
    }

    private static Grid CompactProfileRow(EsimProfile profile, bool isReady)
    {
        var flag = profile.Country switch
        {
            "Japan" => "🇯🇵",
            "France" => "🇫🇷",
            "USA" => "🇺🇸",
            "Thailand" => "🇹🇭",
            "Italy" => "🇮🇹",
            _ => "🌐"
        };
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        row.Add(new Border { WidthRequest = 46, HeightRequest = 46, Content = Text(flag, 22, "#101D35"), BackgroundColor = Color.FromArgb("#EEF2FB"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 } });
        var details = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = profile.ProfileName, FontFamily = "Inter", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35"), MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation }, new Label { Text = $"{profile.Carrier} • {profile.AllocatedGb:0.#} GB", FontFamily = "Inter", FontSize = 12, TextColor = Color.FromArgb("#46516A"), MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation } } };
        row.Add(details, 1, 0);
        row.Add(new Border { Content = new Label { Text = isReady ? "Ready to Install" : "Expired", FontFamily = "Inter", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = isReady ? Color.FromArgb("#00687A") : Color.FromArgb("#63748B") }, Padding = new Thickness(11, 6), BackgroundColor = isReady ? Color.FromArgb("#DDF8FF") : Color.FromArgb("#F1F4FA"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 } }, 2, 0);
        row.Add(Text("⌄", 18, "#46516A"), 3, 0);
        return row;
    }

    private View StoreCallout()
    {
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        row.Add(new Border { WidthRequest = 44, HeightRequest = 44, Content = new Label { Text = "\uE77F", FontFamily = "MaterialAssets", FontSize = 24, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }, BackgroundColor = Color.FromArgb("#0649D5"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 } });
        var details = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { Text("Need a new plan?", 15, "#101D35", true), Text("Browse destinations in 190+ countries", 13, "#46516A") } };
        row.Add(details, 1, 0);
        var goToStore = new Button { Text = "Go to Store", FontFamily = "Inter", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, BackgroundColor = Color.FromArgb("#0649D5"), CornerRadius = 14, Padding = new Thickness(16, 10) };
        goToStore.Clicked += (_, _) => navigate(MobileTab.Store);
        row.Add(goToStore, 2, 0);
        return Card(row, "#EEF3FF");
    }

    private static View FilterLabel(string text, bool selected = false) => new Border { Content = new Label { Text = text, HorizontalOptions = LayoutOptions.Center, FontFamily = "Inter", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = selected ? Color.FromArgb("#0649D5") : Color.FromArgb("#26334B") }, Padding = new Thickness(8, 9), BackgroundColor = selected ? Colors.White : Colors.Transparent, StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } };

    private View StoreContent()
    {
        var stack = PageStack();
        stack.Add(new VerticalStackLayout { Spacing = 3, Children = { Text("●  ZERO ROAMING FEES", 12, "#0752DB", true), Text("Instant high-speed travel data in 190+ countries", 13, "#46516A") } });

        var search = new Entry { Placeholder = "Search country or region (e.g. Japan, France)", BackgroundColor = Colors.White, TextColor = Color.FromArgb("#101D35"), PlaceholderColor = Color.FromArgb("#6C7890"), FontSize = 14 };
        search.TextChanged += (_, e) => vm.Search = e.NewTextValue ?? "";
        var searchRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
        searchRow.Add(new Label { Text = "\uE715", FontFamily = "MaterialAssets", FontSize = 19, TextColor = Color.FromArgb("#26334B"), VerticalOptions = LayoutOptions.Center });
        searchRow.Add(search, 1, 0);
        stack.Add(new Border { Content = searchRow, Padding = new Thickness(12, 0), BackgroundColor = Colors.White, StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 }, HeightRequest = 46 });

        var countries = new HorizontalStackLayout { Spacing = 8 };
        foreach (var country in new[] { "🇯🇵 Japan", "🇺🇸 USA", "🇫🇷 France", "🇰🇷 Korea" })
        {
            var name = country;
            var destination = new Border
            {
                Content = new Label { Text = country, FontFamily = "Inter", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35") },
                Padding = new Thickness(12, 7),
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E5EBF4"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 }
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => vm.SelectCountry(name.Split(' ', 2)[1]);
            destination.GestureRecognizers.Add(tap);
            countries.Add(destination);
        }
        stack.Add(countries);

        var filterRow = new HorizontalStackLayout { Spacing = 6 };
        foreach (var filter in new[] { "Local", "Regional", "Global" })
        {
            var coverage = filter;
            var chip = new Border
            {
                Content = new Label { Text = coverage, FontFamily = "Inter", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#26334B") },
                Padding = new Thickness(12, 6),
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 }
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => vm.SelectCoverage(coverage);
            chip.GestureRecognizers.Add(tap);
            filterRow.Add(chip);
        }
        filterRow.Add(new Border { Content = new Label { Text = "⚡ 5G Ultra", FontFamily = "Inter", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0752DB") }, Padding = new Thickness(12, 6), BackgroundColor = Color.FromArgb("#EAF0FF"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } });
        filterRow.Add(new Border { Content = new Label { Text = "Unlimited Only", FontFamily = "Inter", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#26334B") }, Padding = new Thickness(12, 6), BackgroundColor = Colors.White, StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } });
        stack.Add(filterRow);

        stack.Add(FeaturedDestinationCard());
        stack.Add(new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center, Children = { Text("Available Plans", 17, "#101D35", true), new Border { Content = new Label { Text = $"{vm.FilteredPlans.Count} active", FontFamily = "Inter", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0649D5") }, Padding = new Thickness(9, 4), BackgroundColor = Color.FromArgb("#EAF0FF"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } } } }, new Label { Text = "Tax included  ⓘ", FontFamily = "Inter", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0649D5"), VerticalOptions = LayoutOptions.Center }.SetColumn(1) } });

        foreach (var plan in vm.FilteredPlans)
        {
            var p = plan;
            var planContext = new Border { BindingContext = p };
            stack.Add(StorePlanCard(p, planContext));
        }

        stack.Add(StoreBenefits());
        stack.Add(StoreSupportCard());
        return stack;
    }

    private static View FeaturedDestinationCard()
    {
        var content = new VerticalStackLayout { Spacing = 6 };
        content.Add(new Border { Content = new HorizontalStackLayout { Spacing = 7, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = "Featured Destination", FontFamily = "Inter", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0752DB") }, new Label { Text = "🇯🇵 Tokyo & Beyond", FontFamily = "Inter", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35") } } }, Padding = new Thickness(10, 5), BackgroundColor = Color.FromArgb("#DCE9FF"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 }, HorizontalOptions = LayoutOptions.Start });
        content.Add(new Label { Text = "Japan Travel Connectivity Pass", FontFamily = "Inter", FontSize = 19, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35") });
        content.Add(new Label { Text = "SoftBank & NTT Docomo multi-carrier 5G with...", FontFamily = "Inter", FontSize = 13, TextColor = Color.FromArgb("#26334B") });
        content.Add(new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = "$4.50", FontFamily = "Inter", FontSize = 21, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35") }, new Label { Text = "/ starter", FontFamily = "Inter", FontSize = 13, TextColor = Color.FromArgb("#46516A"), VerticalOptions = LayoutOptions.End }, new Label { Text = "•", FontFamily = "Inter", FontSize = 13, TextColor = Color.FromArgb("#46516A"), VerticalOptions = LayoutOptions.Center }, new Label { Text = "12 plans ready", FontFamily = "Inter", FontSize = 13, TextColor = Color.FromArgb("#46516A"), VerticalOptions = LayoutOptions.Center } } });
        return new Border { Content = content, Padding = new Thickness(16), Background = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Color.FromArgb("#E8F4FF"), 0), new GradientStop(Color.FromArgb("#DDF7FF"), 1) }, new Point(0, 0), new Point(1, 1)), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 } };
    }

    private View StorePlanCard(Plan plan, Border planContext)
    {
        var flag = plan.Country switch
        {
            "Japan" => "🇯🇵",
            "United States" => "🇺🇸",
            "France" => "🇫🇷",
            "South Korea" => "🇰🇷",
            "Europe Regional" => "🇪🇺",
            _ => "🌐"
        };

        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        header.Add(new Border { WidthRequest = 40, HeightRequest = 40, Content = Text(flag, 20, "#101D35"), BackgroundColor = Color.FromArgb("#EEF2FB"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } });
        header.Add(new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = plan.Region, FontFamily = "Inter", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35"), MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation }, new Label { Text = $"{plan.Provider}  •  {plan.NetworkLabel}", FontFamily = "Inter", FontSize = 12, TextColor = Color.FromArgb("#46516A"), MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation } } }, 1, 0);
        header.Add(new Border { Content = new Label { Text = plan.Badge, FontFamily = "Inter", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#006141") }, Padding = new Thickness(9, 5), BackgroundColor = Color.FromArgb("#DDF8EE"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } }, 2, 0);

        var specs = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
        specs.Add(PlanSpec("Data Size", plan.DataLabel), 0, 0);
        specs.Add(PlanSpec("Validity", $"{plan.ValidityDays} Days"), 1, 0);
        specs.Add(PlanSpec("Network", plan.NetworkLabel), 2, 0);

        var buyButton = new Button { Text = "Buy Plan  →", FontFamily = "Inter", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, BackgroundColor = Color.FromArgb("#0649D5"), CornerRadius = 14, Padding = new Thickness(16, 10), HorizontalOptions = LayoutOptions.End };
        buyButton.Clicked += (_, _) => actions.BuyPlanClicked(planContext, EventArgs.Empty);

        var priceRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        priceRow.Add(new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { Text("Price", 12, "#46516A"), new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.End, Children = { Text($"${plan.Price:0.00}", 24, "#101D35", true), Text("USD", 12, "#46516A", false) } } } });
        priceRow.Add(buyButton, 1, 0);

        return Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                header,
                SoftPanel(specs),
                Text(plan.FeatureList, 12, "#26334B"),
                priceRow
            }
        });
    }

    private static View PlanSpec(string label, string value) => new VerticalStackLayout { Spacing = 3, Children = { new Label { Text = label, FontFamily = "Inter", FontSize = 12, TextColor = Color.FromArgb("#46516A") }, new Label { Text = value, FontFamily = "Inter", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0649D5") } } };

    private static View StoreBenefits()
    {
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
        row.Add(BenefitTile("\uE712", "UiKitIcons", "100% Digital eSIM", "No physical SIM swap"), 0, 0);
        row.Add(BenefitTile("\uE71B", "UiKitIcons", "Keep WhatsApp", "Original number active"), 1, 0);
        row.Add(BenefitTile("\uE729", "UiKitIcons", "Zero Bill Shock", "Prepaid fixed cost"), 2, 0);
        return Card(row);
    }

    private static View BenefitTile(string icon, string fontFamily, string title, string subtitle) => new VerticalStackLayout
    {
        Spacing = 4,
        Children =
        {
            new Label { Text = icon, FontFamily = fontFamily, FontSize = 22, TextColor = Color.FromArgb("#0649D5"), HorizontalTextAlignment = TextAlignment.Center, HorizontalOptions = LayoutOptions.Fill },
            new Label { Text = title, FontFamily = "Inter", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35"), HorizontalTextAlignment = TextAlignment.Center, HorizontalOptions = LayoutOptions.Fill },
            new Label { Text = subtitle, FontFamily = "Inter", FontSize = 11, TextColor = Color.FromArgb("#46516A"), HorizontalTextAlignment = TextAlignment.Center, HorizontalOptions = LayoutOptions.Fill }
        }
    };

    private static View StoreSupportCard()
    {
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        row.Add(new Border { WidthRequest = 40, HeightRequest = 40, Content = new Label { Text = "\uE71B", FontFamily = "UiKitIcons", FontSize = 20, TextColor = Color.FromArgb("#0649D5"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }, BackgroundColor = Color.FromArgb("#EAF0FF"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 } });
        row.Add(new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { Text("Need help choosing a plan?", 14, "#101D35", true), Text("Chat with Roaming Advisor • 24/7", 12, "#46516A") } }, 1, 0);
        row.Add(new Border { WidthRequest = 40, HeightRequest = 40, Content = new Label { Text = "›", FontFamily = "Inter", FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#26334B"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }, BackgroundColor = Colors.White, StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 } }, 2, 0);
        return Card(row, "#EEF3FF");
    }

    private View BottomNavigation()
    {
        var bar = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, BackgroundColor = Colors.White, Padding = new Thickness(3, 8), HeightRequest = 68 };
        AddTab(bar, "\uE714", "UiKitIcons", "Dashboard", MobileTab.Dashboard, 0);
        AddTab(bar, "\uE713", "UiKitIcons", "My eSIMs", MobileTab.Profiles, 1);
        AddTab(bar, "\uE729", "UiKitIcons", "Store", MobileTab.Store, 2);
        return bar;
    }

    private void AddTab(Grid bar, string icon, string fontFamily, string label, MobileTab target, int col)
    {
        var color = tab == target ? Color.FromArgb("#0649D5") : Color.FromArgb("#26334B");
        var content = new VerticalStackLayout
        {
            Spacing = 3,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = icon, FontFamily = fontFamily, FontSize = 22, TextColor = color, HorizontalTextAlignment = TextAlignment.Center, HorizontalOptions = LayoutOptions.Fill },
                new Label { Text = label, FontFamily = "Inter", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = color, HorizontalTextAlignment = TextAlignment.Center, HorizontalOptions = LayoutOptions.Fill }
            }
        };
        var button = new Border { Content = content, BackgroundColor = Colors.Transparent, StrokeThickness = 0 };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => navigate(target);
        button.GestureRecognizers.Add(tap);
        bar.Add(button, col, 0);
    }
    private static VerticalStackLayout PageStack()=>new(){Padding=new Thickness(16,12,16,20),Spacing=14};
    private static Label Text(string value,double size,string color,bool bold=false)=>new(){Text=value,FontFamily="Inter",FontSize=size,TextColor=Color.FromArgb(color),FontAttributes=bold?FontAttributes.Bold:FontAttributes.None,LineBreakMode=LineBreakMode.WordWrap,VerticalOptions=LayoutOptions.Center,HorizontalOptions=LayoutOptions.Center};
    private static Border Card(View content,string bg="#FFFFFF")=>new(){Content=content,Padding=14,BackgroundColor=Color.FromArgb(bg),Stroke=Color.FromArgb("#E7EDF8"),StrokeThickness=1,StrokeShape=new Microsoft.Maui.Controls.Shapes.RoundRectangle{CornerRadius=16}};
    private static Border Pill(string value)=>new(){Content=Text(value,11,"#0649D5",true),Padding=new Thickness(9,5),BackgroundColor=Color.FromArgb("#EAF0FF"),StrokeThickness=0,StrokeShape=new Microsoft.Maui.Controls.Shapes.RoundRectangle{CornerRadius=12},VerticalOptions=LayoutOptions.Center};
    private static View DashboardActionButton(string icon, string label, bool primary, Action action)
    {
        var iconBackground = label == "Top Up" ? "#BDEFF6" : primary ? "#3470D6" : "#E4ECFF";
        var iconColor = primary ? Colors.White : Color.FromArgb("#006F82");
        var content = new VerticalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Border
                {
                    WidthRequest = 68,
                    HeightRequest = 68,
                    Content = new Label { Text = icon, FontFamily = "UiKitIcons", FontSize = 27, TextColor = iconColor, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center },
                    BackgroundColor = Color.FromArgb(iconBackground),
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 34 },
                    HorizontalOptions = LayoutOptions.Center
                },
                Text(label, 20, primary ? "#FFFFFF" : "#101D35", primary)
            }
        };
        var button = new Border
        {
            Content = content,
            BackgroundColor = Color.FromArgb(primary ? "#0649D5" : "#FFFFFF"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 },
            HeightRequest = 134,
            Padding = new Thickness(8, 10),
            Shadow = new Shadow { Brush = Color.FromArgb("#24324A"), Opacity = 0.08f, Radius = 4, Offset = new Point(0, 2) }
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => action();
        button.GestureRecognizers.Add(tap);
        return button;
    }
    private View CreateUsageGauge()
    {
        var maximum = Math.Max(vm.Allocation, 0.1);
        var gauge = new SfRadialGauge
        {
            HeightRequest = 260,
            WidthRequest = 260,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.White
        };
        var axis = new RadialAxis
        {
            Minimum = 0,
            Maximum = maximum,
            StartAngle = 270,
            EndAngle = 270,
            ShowLabels = false,
            ShowTicks = false,
            RadiusFactor = 0.9,
            AxisLineStyle = new RadialLineStyle { Thickness = 18, ThicknessUnit = SizeUnit.Pixel, Fill = new SolidColorBrush(Color.FromArgb("#EEF3FF")) },
            MajorTickStyle = new RadialTickStyle { Length = 0, Stroke = new SolidColorBrush(Colors.Transparent) },
            MinorTickStyle = new RadialTickStyle { Length = 0, Stroke = new SolidColorBrush(Colors.Transparent) },
            Pointers =
            {
                new RangePointer { Value = Math.Clamp(vm.Consumed, 0, maximum), PointerWidth = 18, WidthUnit = SizeUnit.Pixel, Fill = new SolidColorBrush(Color.FromArgb("#0752FF")), CornerStyle = CornerStyle.BothCurve }
            }
        };
        gauge.Axes.Add(axis);
        return gauge;
    }

    private static View DashboardMetric(string icon, string background, string color, string label, string value)
    {
        return new Border
        {
            Padding = new Thickness(12, 10),
            BackgroundColor = Color.FromArgb("#EEF3FF"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 },
            Content = new HorizontalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Border { WidthRequest = 44, HeightRequest = 44, Content = new Label { Text = icon, FontFamily = icon == "\uE74C" ? "MaterialAssets" : "UiKitIcons", FontSize = 23, TextColor = Color.FromArgb(color), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }, BackgroundColor = Color.FromArgb(background), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 }, VerticalOptions = LayoutOptions.Center },
                    new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = label, FontFamily = "Inter", FontSize = 13, TextColor = Color.FromArgb("#26334B"), HorizontalOptions = LayoutOptions.Fill, VerticalTextAlignment = TextAlignment.Center }, new Label { Text = value, FontFamily = "Inter", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#101D35"), HorizontalOptions = LayoutOptions.Fill, VerticalTextAlignment = TextAlignment.Center } } }
                }
            }
        };
    }
    private View RecentActivityCard()
    {
        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        header.Add(Text("Recent Activity", 20, "#101D35", true));
        var viewAll = new Button { Text = "View All  ›", FontSize = 14, Padding = 0, BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#0649D5") };
        viewAll.Clicked += (_, _) => navigate(MobileTab.Profiles);
        header.Add(viewAll, 1, 0);

        var content = new VerticalStackLayout { Spacing = 9 };
        content.Add(header);
        content.Add(Activity("⊕", "#DCE8FF", "#0649D5", "Top-Up Purchased", "Nov 21, 14:15 JST", "+3.0 GB", "$6.50 USD", "#006141"));
        content.Add(Activity("◩", "#D8EAF0", "#007C8A", "High-Speed Roaming", "Yesterday in Shinjuku", "1.2 GB", "5G Sub-6", "#00687A"));
        content.Add(Activity("✈︎", "#D9EFE8", "#00856A", "Profile Activated", "Nov 14, 08:31 JST", "Haneda (HND)", null, "#006141"));
        return Card(content);
    }

    private static View Activity(string icon, string iconBackground, string iconColor, string title, string detail, string value, string? secondaryValue, string valueColor)
    {
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        var iconTile = new Border { WidthRequest = 42, HeightRequest = 42, Content = Text(icon, 21, iconColor, true), BackgroundColor = Color.FromArgb(iconBackground), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 13 }, VerticalOptions = LayoutOptions.Center };
        row.Add(iconTile, 0, 0);
        row.Add(new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { Text(title, 16, "#101D35", true), Text(detail, 12, "#26334B") } }, 1, 0);

        View trailing;
        if (secondaryValue is null)
        {
            trailing = new Border { Content = Text(value, 11, "#007C64", true), Padding = new Thickness(9, 4), BackgroundColor = Color.FromArgb("#C6F3E9"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 }, VerticalOptions = LayoutOptions.Center };
        }
        else
        {
            trailing = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End, Children = { Text(value, 13, valueColor, true), Text(secondaryValue, 11, valueColor, true) } };
        }
        row.Add(trailing, 2, 0);
        return new Border { Content = row, Padding = new Thickness(12, 10), BackgroundColor = Color.FromArgb("#EEF3FF"), StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 } };
    }
    private View UsageCard()
    {
        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        header.Add(new HorizontalStackLayout { Spacing = 10, Children = { new Label { Text = "\uE71F", FontFamily = "UiKitIcons", FontSize = 24, TextColor = Color.FromArgb("#006F82"), VerticalOptions = LayoutOptions.Center }, Text("Last 7 Days Usage", 23, "#101D35", true) } });
        header.Add(Pill($"{vm.SevenDayTotal:0.00} GB Total"), 1, 0);

        var points = new[]
        {
            new DailyUsage("Fri", .7), new DailyUsage("Sat", .9), new DailyUsage("Sun", 1.1),
            new DailyUsage("Mon", .8), new DailyUsage("Tue", .9), new DailyUsage("Wed", 1.0),
            new DailyUsage("Thu", 1.4)
        };
        var peak = points.OrderByDescending(point => point.UsedGb).First();
        var peakIndex = Array.IndexOf(points, peak);
        var maxValue = Math.Max(peak.UsedGb, 0.1);
        var charts = new Grid { ColumnSpacing = 5, HeightRequest = 206 };
        foreach (var _ in points)
            charts.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        for (var i = 0; i < points.Length; i++)
        {
            var point = points[i];
            var height = (float)(Math.Max(point.UsedGb / maxValue, 0.08) * 132);
            var label = new VerticalStackLayout { Spacing = 7, VerticalOptions = LayoutOptions.End };
            label.Add(Text(point.UsedGb.ToString("0.0"), 14, i == peakIndex ? "#0649D5" : "#26334B", i == peakIndex));
            var bar = new BoxView { HeightRequest = height, Color = Color.FromArgb(i == peakIndex ? "#0649D5" : "#E2ECFF"), HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.End };
            label.Add(new Border { Content = bar, StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(13, 13, 0, 0) } });
            label.Add(Text(point.Day[..1], 17, i == peakIndex ? "#0649D5" : "#26334B", i == peakIndex));
            charts.Add(label, i, 0);
        }

        var footer = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        footer.Add(new HorizontalStackLayout { Spacing = 7, Children = { new BoxView { WidthRequest = 12, HeightRequest = 12, CornerRadius = 6, Color = Color.FromArgb("#0649D5"), VerticalOptions = LayoutOptions.Center }, Text($"Peak: {peak.Day} ({peak.UsedGb:0.#} GB in Shibuya)", 14, "#46516A") } });
        footer.Add(Text("Daily limit: Unlimited", 14, "#46516A"), 1, 0);

        return new VerticalStackLayout { Spacing = 20, Children = { header, charts, footer } };
    }
}


internal static class MobileViewExtensions
{
    public static T SetColumn<T>(this T view,int column) where T:View { Grid.SetColumn(view,column); return view; }
}
