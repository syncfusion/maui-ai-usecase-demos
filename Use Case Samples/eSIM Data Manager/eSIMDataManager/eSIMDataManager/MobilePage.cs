using eSIMDataManager.Models;
using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public sealed class MobilePage : ContentPage
{
    public enum MobileTab { Dashboard, Profiles, Store }
    private readonly MainViewModel vm;
    private readonly MobileTab tab;
    private readonly DemoPage actions;
    private readonly Grid root = new() { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
    private readonly ScrollView scroll = new() { VerticalScrollBarVisibility = ScrollBarVisibility.Never };

    public MobilePage(MainViewModel viewModel, MobileTab selectedTab)
    {
        vm = viewModel;
        tab = selectedTab;
        actions = new DemoPage(vm);
        Title = selectedTab.ToString();
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
        stack.Add(Header("Dashboard"));
        stack.Add(new VerticalStackLayout { Spacing = 3, Children = { Text("⌖  ROAMING IN TOKYO, JAPAN", 12, "#00687A"), Text("Good morning, Alex", 28, "#101D35") } });
        var plan = Card(new VerticalStackLayout { Spacing = 12, Children = {
            new HorizontalStackLayout { Spacing = 8, Children = { Text("🇯🇵",22,"#101D35"), new VerticalStackLayout { Children = { Text("Japan Unlimited   5G+",20,"#101D35",true), Text("SoftBank Corp. • 18ms ping",13,"#00687A") } }, Text("● Connected",12,"#006141") } },
            new Grid { HeightRequest=228, WidthRequest=228, HorizontalOptions=LayoutOptions.Center, Children = {
                new GraphicsView { HeightRequest=220, WidthRequest=220, Drawable=new UsageRingDrawable(vm.AvailablePercent) },
                new VerticalStackLayout { HorizontalOptions=LayoutOptions.Center, VerticalOptions=LayoutOptions.Center, Spacing=2, Children={ Text("HIGH SPEED",12,"#46516A"), Text(vm.RemainingText,36,"#101D35",true), Text($"Remaining of {vm.Allocation:0.#} GB",13,"#00687A"), Text("⚡ ~0.8 GB/day pace",12,"#101D35") } }
            }},
            new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star)}, ColumnSpacing=8, Children={
                Card(new VerticalStackLayout { Children={Text("Quota Status",12,"#26334B"),Text("68% Available",16,"#101D35",true)} },"#EEF3FF"),
                Card(new VerticalStackLayout { Children={Text("Backup Reserve",12,"#26334B"),Text("Unlimited 256kbps",16,"#101D35",true)} },"#EEF3FF")
            }},
            new VerticalStackLayout { Spacing=5, Children={ new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)}, Children={Text($"▢  {vm.DaysRemaining} Days Left",21,"#101D35",true), Pill("Auto-renew ON")} }, new ProgressBar { Progress=.68, ProgressColor=Color.FromArgb("#0649D5"), BackgroundColor=Color.FromArgb("#DBE6FA"), HeightRequest=7 }, Text($"Started {vm.ActivationDateText}            Expires {vm.ExpiryText}",12,"#46516A") } }
        }}, "#FFFFFF");
        stack.Add(plan);
        var actionsGrid = new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star)}, ColumnSpacing=14 };
        actionsGrid.Add(DashboardActionButton("\uE721", "Buy Plan", true, () => Shell.Current.GoToAsync("//store")), 0, 0);
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
        var appHeader = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        var dashboardButton = new Button { Text = "◉   Dashboard", FontSize = 15, FontAttributes = FontAttributes.Bold, Padding = 0, HorizontalOptions = LayoutOptions.Start, BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#26334B") };
        dashboardButton.Clicked += async (_, _) => await Shell.Current.GoToAsync("//dashboard");
        appHeader.Add(dashboardButton);
        appHeader.Add(Text("▮", 17, "#46516A").SetColumn(1));
        appHeader.Add(Text("●", 21, "#0649D5").SetColumn(2));
        stack.Add(appHeader);
        stack.Add(new VerticalStackLayout { Spacing = 1, Children = { Text("My eSIMs", 26, "#101D35", true), Text("Manage profiles & data roaming", 12, "#46516A") } });
        var toolbar = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        var search = new Entry { Placeholder = "⌕", WidthRequest = 42, HeightRequest = 40, BackgroundColor = Colors.White, TextColor = Color.FromArgb("#101D35"), HorizontalTextAlignment = TextAlignment.Center };
        var add = Button("+", () => Shell.Current.GoToAsync("//store")); add.WidthRequest = 42; add.HeightRequest = 40;
        toolbar.Add(new BoxView { Color = Colors.Transparent }); toolbar.Add(search, 1, 0); toolbar.Add(add, 2, 0); stack.Add(toolbar);
        var filters = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 4, BackgroundColor = Color.FromArgb("#E7EEFF"), Padding = 4 };
        filters.Add(FilterLabel($"Active  {vm.ActiveProfiles.Count}"), 0, 0);
        filters.Add(FilterLabel($"Ready  {vm.ReadyProfiles.Count}"), 1, 0);
        filters.Add(FilterLabel($"Expired  {vm.ExpiredProfiles.Count}"), 2, 0);
        stack.Add(filters);
        foreach (var profile in vm.ActiveProfiles.Concat(vm.ReadyProfiles).Concat(vm.ExpiredProfiles))
        {
            var context = new Border { BindingContext = profile };
            if (profile.Status == ProfileStatus.Active)
            {
                var actionsRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
                actionsRow.Add(Button("♜  Top Up Data", () => actions.TopUpItemClicked(context, EventArgs.Empty)), 0, 0);
                actionsRow.Add(Button("▦  Show QR Code", () => actions.QrItemClicked(context, EventArgs.Empty)), 1, 0);
                var secondary = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
                secondary.Add(SecondaryButton("⟳  Renew Plan", () => actions.RenewItemClicked(context, EventArgs.Empty)), 0, 0);
                secondary.Add(SecondaryButton("▣  Install Guide", () => actions.QrItemClicked(context, EventArgs.Empty)), 1, 0);
                var progress = new ProgressBar { Progress = profile.UsageFraction, ProgressColor = Color.FromArgb("#0752DB"), BackgroundColor = Color.FromArgb("#DBE6FA"), HeightRequest = 6 };
                var item = Card(new VerticalStackLayout { Spacing = 10, Children = {
                    new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { new VerticalStackLayout { Children = { Text($"🇯🇵  {profile.ProfileName}", 17, "#101D35", true), Text($"      {profile.Carrier}", 10, "#46516A") } }, Text("● Active   ⌃", 11, "#006141", true).SetColumn(1) } },
                    new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { Text($"◉  {profile.RemainingGb:0.#} GB Remaining", 12, "#0649D5", true), Text($"◷  {profile.DaysRemaining} Days Left", 12, "#101D35", true).SetColumn(1) } },
                    Card(new VerticalStackLayout { Spacing = 6, Children = { new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, Children = { Text($"Country & Network\n{profile.Country}  ·  5G", 11, "#46516A"), Text("Connected\n(Roaming ON)", 11, "#006141", true).SetColumn(1) } }, Text($"ICCID Number   {profile.Iccid}  ▢", 10, "#46516A") } }, "#EEF3FF"),
                    Card(new VerticalStackLayout { Spacing = 5, Children = { new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { Text("Remaining Balance", 11, "#46516A"), Text($"{profile.UsagePercent:0}% Consumed", 10, "#0649D5", true).SetColumn(1) } }, new HorizontalStackLayout { Spacing = 6, Children = { Text($"{profile.RemainingGb:0.#} GB", 19, "#101D35", true), Text($"of {profile.AllocatedGb:0.#} GB Total", 10, "#46516A") } }, progress, Text("◉  128 kbps unlimited reserve active", 10, "#006141") } }, "#EEF3FF"),
                    Card(new VerticalStackLayout { Spacing = 5, Children = { Text($"Activated: {profile.ActivationDate:MMM d, yyyy}      Expires: {profile.ExpiryDate:MMM d, yyyy}", 10, "#46516A"), new ProgressBar { Progress = 1 - profile.DaysRemaining / 14d, ProgressColor = Color.FromArgb("#0752DB"), BackgroundColor = Color.FromArgb("#DBE6FA"), HeightRequest = 5 }, Text($"Validity Period                                  {profile.DaysRemaining} Days Remaining", 10, "#0649D5", true) } }, "#EEF3FF"),
                    Card(ActivationCodeRow(profile), "#EEF3FF"),
                    actionsRow, secondary
                } });
                stack.Add(item);
            }
            else
            {
                var isReady = profile.Status == ProfileStatus.ReadyToInstall;
                Action openAction = isReady ? () => actions.ActivateItemClicked(context, EventArgs.Empty) : () => actions.QrItemClicked(context, EventArgs.Empty);
                var compact = CompactProfileRow(profile, isReady);
                var card = Card(compact); var tap = new TapGestureRecognizer(); tap.Tapped += (_, _) => openAction(); card.GestureRecognizers.Add(tap); stack.Add(card);
            }
        }
        stack.Add(Card(StoreCallout(), "#0649D5"));
        return stack;
    }

    private static Grid ActivationCodeRow(EsimProfile profile)
    {
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        row.Add(Text("▦", 25, "#101D35", true));
        var details = new VerticalStackLayout { Children = { Text("LPA Activation String", 10, "#46516A"), Text(profile.ActivationCode, 9, "#101D35"), Text("Ready for re-installation if needed", 9, "#0649D5") } };
        row.Add(details, 1, 0);
        row.Add(Text("▢", 17, "#0649D5", true), 2, 0);
        return row;
    }

    private static Grid CompactProfileRow(EsimProfile profile, bool isReady)
    {
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        row.Add(Text(profile.Country == "France" ? "EU" : "TH", 14, "#101D35", true));
        var details = new VerticalStackLayout { Children = { Text(profile.ProfileName, 14, "#101D35", true), Text($"{profile.Carrier} • {profile.AllocatedGb:0.#} GB", 10, "#46516A") } };
        row.Add(details, 1, 0);
        row.Add(Pill(isReady ? "Ready to Install" : "Expired"), 2, 0);
        row.Add(Text("⌄", 16, "#46516A"), 3, 0);
        return row;
    }

    private static Grid StoreCallout()
    {
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        row.Add(Text("◉", 20, "#FFFFFF", true));
        var details = new VerticalStackLayout { Children = { Text("Need a new plan?", 12, "#FFFFFF", true), Text("Browse destinations in 190+ countries", 10, "#FFFFFF") } };
        row.Add(details, 1, 0);
        row.Add(Button("Go to Store", () => Shell.Current.GoToAsync("//store")), 2, 0);
        return row;
    }

    private static View FilterLabel(string text) => new Border { Content = Text(text, 10, "#0649D5", true), Padding = new Thickness(4, 7), BackgroundColor = Colors.White, StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 } };
    private static Button SecondaryButton(string text, Action action) { var button = Button(text, action); button.BackgroundColor = Color.FromArgb("#EEF3FF"); button.TextColor = Color.FromArgb("#101D35"); return button; }

    private View StoreContent()
    {
        var stack=PageStack(); stack.Add(Header("Store"));
        stack.Add(new VerticalStackLayout { Spacing=4, Children={Pill("● GLOBAL ROAMING READY"),Text("Instant travel data in 190+ countries",22,"#101D35",true)} });
        var search=new Entry { Placeholder="⌕  Search country or region (e.g. Japan)", BackgroundColor=Colors.White, TextColor=Color.FromArgb("#101D35") }; search.TextChanged+=(_,e)=>vm.Search=e.NewTextValue??""; stack.Add(search);
        var countries=new HorizontalStackLayout { Spacing=6 }; foreach(var c in new[]{"🇯🇵 Japan","🇺🇸 USA","🇫🇷 France","🇰🇷 Korea"}) { var destination=new Button { Text=c, Padding=new Thickness(10,5), BackgroundColor=Color.FromArgb("#EEF3FF"), TextColor=Color.FromArgb("#0649D5"), CornerRadius=20 }; destination.Clicked+=(_,_)=>vm.SelectCountry(c.Split(' ',2)[1]); countries.Add(destination); } stack.Add(countries);
        stack.Add(Card(new VerticalStackLayout { Spacing=4, Children={Pill("FEATURED DESTINATION  •  🇯🇵 Tokyo & Beyond"),Text("Japan Travel Connectivity Pass",18,"#101D35",true),Text("SoftBank & NTT Docomo multi-carrier 5G",12,"#46516A"),Text("$4.50 / starter  •  12 plans ready",15,"#0649D5",true)} },"#DDF7FF"));
        stack.Add(Text($"Available Plans   {vm.FilteredPlans.Count} active",16,"#101D35",true));
        foreach(var plan in vm.FilteredPlans)
        {
            var p=plan;
            var planContext = new Border { BindingContext = p };
            stack.Add(Card(new VerticalStackLayout { Spacing=8, Children={new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)}, Children={Text($"{p.CountryCode}  {p.Region}",17,"#101D35",true),Pill(p.Badge)} },Text($"{p.Provider} • {p.NetworkLabel}",12,"#46516A"), new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star)}, Children={Metric("Data Size",p.DataLabel),Metric("Validity",$"{p.ValidityDays} Days",1),Metric("Network",p.NetworkLabel,2)} }, Text(p.FeatureList,11,"#26334B"),new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)}, Children={Text($"${p.Price:0.00} USD",21,"#101D35",true), Button("Buy Plan  →",()=>actions.BuyPlanClicked(planContext, EventArgs.Empty))} }} }));
        }
        stack.Add(Card(new HorizontalStackLayout { Spacing=8, Children={Text("◉",20,"#0649D5"),Text("Need help choosing a plan?   Chat with Roaming Advisor • 24/7",13,"#101D35")} },"#EEF3FF"));
        return stack;
    }

    private View BottomNavigation()
    {
        var bar=new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star)}, BackgroundColor=Colors.White, Padding=new Thickness(3,8), HeightRequest=62 };
        AddTab(bar,"▦","Dashboard",MobileTab.Dashboard,0); AddTab(bar,"▤","My eSIMs",MobileTab.Profiles,1); AddTab(bar,"◉","Store",MobileTab.Store,2); AddTab(bar,"▣","Orders",MobileTab.Profiles,3); return bar;
    }
    private void AddTab(Grid bar,string icon,string label,MobileTab target,int col){var b=new Button{Text=$"{icon}\n{label}",FontSize=11,Padding=0,BackgroundColor=Colors.Transparent,TextColor=tab==target?Color.FromArgb("#0649D5"):Color.FromArgb("#101D35")}; b.Clicked+=async(_,_)=>await Shell.Current.GoToAsync(target switch{MobileTab.Dashboard=>"//dashboard",MobileTab.Profiles=>"//profiles",_=>"//store"});bar.Add(b,col,0);}
    private View Header(string title)=>new Grid{ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)},Children={Text(title,22,"#101D35",true),Text("▮  ◉",18,"#0649D5").SetColumn(1)}};
    private static VerticalStackLayout PageStack()=>new(){Padding=new Thickness(16,12,16,20),Spacing=14};
    private static Label Text(string value,double size,string color,bool bold=false)=>new(){Text=value,FontFamily="Inter",FontSize=size,TextColor=Color.FromArgb(color),FontAttributes=bold?FontAttributes.Bold:FontAttributes.None,LineBreakMode=LineBreakMode.WordWrap,VerticalOptions=LayoutOptions.Center,HorizontalOptions=LayoutOptions.Center};
    private static Border Card(View content,string bg="#FFFFFF")=>new(){Content=content,Padding=14,BackgroundColor=Color.FromArgb(bg),Stroke=Color.FromArgb("#E7EDF8"),StrokeThickness=1,StrokeShape=new Microsoft.Maui.Controls.Shapes.RoundRectangle{CornerRadius=16}};
    private static Border Pill(string value)=>new(){Content=Text(value,11,"#0649D5",true),Padding=new Thickness(9,5),BackgroundColor=Color.FromArgb("#EAF0FF"),StrokeThickness=0,StrokeShape=new Microsoft.Maui.Controls.Shapes.RoundRectangle{CornerRadius=12},VerticalOptions=LayoutOptions.Center};
    private static Button Button(string text,Action action){var b=new Button{Text=text,BackgroundColor=Color.FromArgb("#0649D5"),TextColor=Colors.White,CornerRadius=10,FontSize=13,Padding=new Thickness(12,7)};b.Clicked+=(_,_)=>action();return b;}
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
    private static View Metric(string label,string value,int col=0)=>new VerticalStackLayout{Spacing=3,Children={Text(label,10,"#46516A"),Text(value,13,"#101D35",true)}}.SetColumn(col);
    private static View RecentActivityCard()
    {
        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        header.Add(Text("Recent Activity", 20, "#101D35", true));
        var viewAll = new Button { Text = "View All  ›", FontSize = 14, Padding = 0, BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#0649D5") };
        viewAll.Clicked += async (_, _) => await Shell.Current.GoToAsync("//profiles");
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

internal sealed class UsageRingDrawable(double progress) : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        const float inset = 12;
        canvas.StrokeSize = 16;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeColor = Color.FromArgb("#E8EEFC");
        canvas.DrawCircle(dirtyRect.Center.X, dirtyRect.Center.Y, dirtyRect.Width / 2 - inset);
        canvas.StrokeColor = Color.FromArgb("#0752DB");
        var rect = new RectF(inset, inset, dirtyRect.Width - inset * 2, dirtyRect.Height - inset * 2);
        canvas.DrawArc(rect, -90, (float)(360 * progress / 100), false, false);
    }
}

internal static class MobileViewExtensions
{
    public static T SetColumn<T>(this T view,int column) where T:View { Grid.SetColumn(view,column); return view; }
}
