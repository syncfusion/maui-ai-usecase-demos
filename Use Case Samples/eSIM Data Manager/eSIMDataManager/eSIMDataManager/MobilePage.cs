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
        var actionsGrid = new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star)}, ColumnSpacing=8 };
        actionsGrid.Add(ActionButton("🛒", "Buy Plan", "#0649D5", Colors.White, () => Shell.Current.GoToAsync("//store")),0,0);
        actionsGrid.Add(ActionButton("ϟ", "Top Up", "#FFFFFF", Color.FromArgb("#101D35"), () => actions.TopUpActiveClicked(this, EventArgs.Empty)),1,0);
        actionsGrid.Add(ActionButton("▦", "Show QR", "#FFFFFF", Color.FromArgb("#101D35"), () => actions.QrActiveClicked(this, EventArgs.Empty)),2,0);
        stack.Add(actionsGrid);
        stack.Add(Card(new VerticalStackLayout { Spacing=12, Children={new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)}, Children={Text("Last 7 Days Usage",20,"#101D35",true),Pill($"{vm.SevenDayTotal:0.00} GB Total")} }, UsageBars()} }));
        stack.Add(Card(new VerticalStackLayout { Spacing=9, Children={ Text("Recent Activity",20,"#101D35",true), Activity("Top-Up Purchased", "Nov 21, 14:15 JST", "+3.0 GB"), Activity("High-Speed Roaming", "Yesterday in Shinjuku", "1.2 GB"), Activity("Profile Activated", "Nov 14, 08:31 JST", "Haneda (HND)") } }));
        return stack;
    }

    private View ProfilesContent()
    {
        var stack=PageStack(); stack.Add(Header("My eSIMs"));
        stack.Add(new HorizontalStackLayout { Spacing=8, Children={ Pill($"Active  {vm.ActiveProfiles.Count}"), Pill($"Ready  {vm.ReadyProfiles.Count}"), Pill($"Expired  {vm.ExpiredProfiles.Count}") } });
        foreach(var profile in vm.ActiveProfiles.Concat(vm.ReadyProfiles).Concat(vm.ExpiredProfiles))
        {
            var buttons=new HorizontalStackLayout { Spacing=8 };
            var profileContext = new Border { BindingContext = profile };
            if(profile.Status==ProfileStatus.Active){ buttons.Add(Button("Top Up",()=>actions.TopUpItemClicked(profileContext,EventArgs.Empty))); buttons.Add(Button("Show QR",()=>actions.QrItemClicked(profileContext,EventArgs.Empty))); }
            else if(profile.Status==ProfileStatus.ReadyToInstall) buttons.Add(Button("Activate / Show QR",()=>actions.ActivateItemClicked(profileContext,EventArgs.Empty)));
            var item=Card(new VerticalStackLayout { Spacing=9, Children={new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)}, Children={Text($"🇯🇵  {profile.ProfileName}",18,"#101D35",true),Pill(profile.Status.ToString())} }, Text($"{profile.Carrier}  •  {profile.Country}",13,"#46516A"), Text($"{profile.RemainingGb:0.#} GB remaining     {profile.DaysRemaining} days left",14,"#0649D5",true), new ProgressBar { Progress=profile.UsageFraction, ProgressColor=Color.FromArgb("#0752DB"), BackgroundColor=Color.FromArgb("#DBE6FA"), HeightRequest=8 }, buttons} }); stack.Add(item);
        }
        stack.Add(Button("Add New eSIM",()=>Shell.Current.GoToAsync("//store")));
        return stack;
    }

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
            stack.Add(Card(new VerticalStackLayout { Spacing=8, Children={new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)}, Children={Text($"{p.CountryCode}  {p.Region}",17,"#101D35",true),Pill(p.Badge)} },Text($"{p.Provider} • {p.NetworkLabel}",12,"#46516A"), new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star)}, Children={Metric("Data Size",p.DataLabel),Metric("Validity",$"{p.ValidityDays} Days",1),Metric("Network",p.NetworkLabel,2)} }, Text(p.FeatureList,11,"#26334B"),new Grid { ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)}, Children={Text($"${p.Price:0.00} USD",21,"#101D35",true), Button("Buy Plan  →",()=>actions.BuyPlanClicked(planContext))} }} }));
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
    private static Label Text(string value,double size,string color,bool bold=false)=>new(){Text=value,FontFamily="Inter",FontSize=size,TextColor=Color.FromArgb(color),FontAttributes=bold?FontAttributes.Bold:FontAttributes.None,LineBreakMode=LineBreakMode.WordWrap,VerticalOptions=LayoutOptions.Center};
    private static Border Card(View content,string bg="#FFFFFF")=>new(){Content=content,Padding=14,BackgroundColor=Color.FromArgb(bg),Stroke=Color.FromArgb("#E7EDF8"),StrokeThickness=1,StrokeShape=new Microsoft.Maui.Controls.Shapes.RoundRectangle{CornerRadius=16}};
    private static Border Pill(string value)=>new(){Content=Text(value,11,"#0649D5",true),Padding=new Thickness(9,5),BackgroundColor=Color.FromArgb("#EAF0FF"),StrokeThickness=0,StrokeShape=new Microsoft.Maui.Controls.Shapes.RoundRectangle{CornerRadius=12},VerticalOptions=LayoutOptions.Center};
    private static Button Button(string text,Action action){var b=new Button{Text=text,BackgroundColor=Color.FromArgb("#0649D5"),TextColor=Colors.White,CornerRadius=10,FontSize=13,Padding=new Thickness(12,7)};b.Clicked+=(_,_)=>action();return b;}
    private static View ActionButton(string icon,string label,string bg,Color fg,Action action){var b=new Button{Text=$"{icon}\n{label}",BackgroundColor=Color.FromArgb(bg),TextColor=fg,CornerRadius=15,HeightRequest=84,FontSize=15,FontAttributes=FontAttributes.Bold};b.Clicked+=(_,_)=>action();return b;}
    private static View Metric(string label,string value,int col=0)=>new VerticalStackLayout{Spacing=3,Children={Text(label,10,"#46516A"),Text(value,13,"#101D35",true)}}.SetColumn(col);
    private static View Activity(string title,string detail,string value)=>new Border{Content=new Grid{ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)},Children={new VerticalStackLayout{Children={Text(title,16,"#101D35",true),Text(detail,12,"#46516A")}},Text(value,13,"#006141",true).SetColumn(1)}},Padding=10,BackgroundColor=Color.FromArgb("#EEF3FF"),StrokeThickness=0,StrokeShape=new Microsoft.Maui.Controls.Shapes.RoundRectangle{CornerRadius=12}};
    private static View UsageBars(){var row=new Grid{ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star)},ColumnSpacing=5,HeightRequest=96};var values=new[]{.7,.9,1.1,.8,.9,1.0,1.4};for(int i=0;i<7;i++){var col=new VerticalStackLayout{VerticalOptions=LayoutOptions.End,Spacing=2,Children={Text(values[i].ToString("0.0"),10,i==6?"#0649D5":"#26334B"),new BoxView{HeightRequest=values[i]*38,Color=Color.FromArgb(i==6?"#0649D5":"#DCE8FF"),VerticalOptions=LayoutOptions.End},Text(new[]{"F","S","S","M","T","W","T"}[i],10,"#101D35")}};row.Add(col,i,0);}return row;}
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
