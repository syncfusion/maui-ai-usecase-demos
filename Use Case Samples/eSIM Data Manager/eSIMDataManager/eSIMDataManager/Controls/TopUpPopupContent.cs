using System.ComponentModel;
using eSIMDataManager.Models;
using eSIMDataManager.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Buttons;

namespace eSIMDataManager.Controls;

/// <summary>
/// Builds the Top-Up Data dialog content that mirrors the desktop reference design:
/// a centered dialog on wide layouts and a bottom sheet on compact widths.
/// </summary>
public sealed class TopUpPopupContent : IDisposable
{
    private const string InkColor = "#0B1C30";
    private const string MutedColor = "#434656";
    private const string PrimaryColor = "#1E5BFF";
    private const string SurfaceLow = "#EFF4FF";
    private const string SurfaceHighest = "#D3E4FE";
    private const string MonoFont = "OpenSansRegular";
    private static readonly Style PopupLabelStyle = new(typeof(Label));

    private readonly MainViewModel viewModel;
    private readonly VerticalStackLayout root;
    private readonly Grid tierContainer;
    private readonly List<Border> tierCards = [];
    private readonly List<Border> tierRadios = [];
    private readonly List<Label> tierSizes = [];
    private readonly Label allowanceLabel;
    private readonly Label purchaseLabel;
    private bool isCompact;

    public TopUpPopupContent(MainViewModel viewModel)
    {
        this.viewModel = viewModel;
        root = new VerticalStackLayout { Spacing = 0, BackgroundColor = Colors.White };
        root.Resources.Add(PopupLabelStyle);
        tierContainer = new Grid();
        allowanceLabel = BuildAllowanceLabel();
        purchaseLabel = BuildPurchaseLabel();

        root.Add(BuildAccentBar());
        root.Add(BuildHeader());
        root.Add(BuildProfileBanner());
        root.Add(BuildTierSection());
        root.Add(BuildSummarySection());
        root.Add(BuildFooter());

        BuildTiers();

        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    public View Content => root;

    public event EventHandler? CloseRequested;
    public event EventHandler? CancelRequested;
    public event EventHandler? PurchaseRequested;
    public event EventHandler? ChangePaymentRequested;

    public void Dispose() => viewModel.PropertyChanged -= ViewModelOnPropertyChanged;

    /// <summary>Switches between the wide three-column tier layout and the compact stacked sheet layout.</summary>
    public void SetCompact(bool compact)
    {
        if (isCompact == compact) return;
        isCompact = compact;
        BuildTiers();
    }

    private void BuildTiers()
    {
        tierContainer.Children.Clear();
        tierCards.Clear();
        tierRadios.Clear();
        tierSizes.Clear();

        if (isCompact)
        {
            var stack = new VerticalStackLayout { Spacing = 10 };
            foreach (var pack in viewModel.TopUpPacks)
            {
                stack.Add(BuildTierCard(pack));
            }
            tierContainer.Add(stack);
            return;
        }

        var grid = new Grid
        {
            ColumnSpacing = 12,
            Padding = new Thickness(3),
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };

        var index = 0;
        foreach (var pack in viewModel.TopUpPacks)
        {
            grid.Add(BuildTierCard(pack), index++);
        }
        tierContainer.Add(grid);
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.SelectedTopUpGb)) return;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        for (var index = 0; index < viewModel.TopUpPacks.Count; index++)
        {
            var pack = viewModel.TopUpPacks[index];
            var selected = viewModel.IsTopUpSelected(pack);
            var card = tierCards[index];
            var radio = tierRadios[index];
            var size = tierSizes[index];

            card.BackgroundColor = Color.FromArgb(selected ? "#FFFFFF" : SurfaceLow);
            card.Stroke = Color.FromArgb(selected ? PrimaryColor : SurfaceLow);
            card.StrokeThickness = selected ? 2 : 1;
            size.TextColor = Color.FromArgb(selected ? PrimaryColor : InkColor);

            radio.BackgroundColor = Color.FromArgb(selected ? PrimaryColor : SurfaceHighest);
            radio.Content = new Label
            {
                Text = selected ? "\u2713" : string.Empty,
                FontFamily = "OpenSansRegular",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
        }

        allowanceLabel.Text = viewModel.TopUpAllowanceSummary;
        purchaseLabel.Text = viewModel.TopUpSubmitLabel;
    }

    private static View BuildAccentBar() => new Border
    {
        HeightRequest = 6,
        StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(3) },
        Background = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            GradientStops =
            {
                new GradientStop(Color.FromArgb("#0046CC"), 0f),
                new GradientStop(Color.FromArgb("#0D5CFF"), 0.55f),
                new GradientStop(Color.FromArgb("#00687A"), 1f)
            }
        }
    };

    private View BuildHeader()
    {
        var plusIcon = new Grid { WidthRequest = 30, HeightRequest = 30, VerticalOptions = LayoutOptions.Center };
        plusIcon.Add(new Label
        {
            Text = "\uE73F",
            FontFamily = "MaterialAssets",
            FontSize = 30,
            TextColor = Color.FromArgb(PrimaryColor),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        });
        plusIcon.Add(new Label
        {
            Text = "\uE70D",
            FontFamily = "MaterialAssets",
            FontSize = 13,
            TextColor = Color.FromArgb(PrimaryColor),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        });

        var title = new Label
        {
            Text = "Top-Up Data",
            FontFamily = "OpenSansRegular",
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };

        var heading = new HorizontalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center };
        heading.Add(plusIcon);
        heading.Add(title);

        var subtitle = new Label
        {
            Text = "Add extra high-speed data to your active Japan 5G Explorer profile.",
            FontFamily = "OpenSansRegular",
            FontSize = 14,
            TextColor = Color.FromArgb(MutedColor)
        };

        var left = new VerticalStackLayout { Spacing = 4 };
        left.Add(heading);
        left.Add(subtitle);

        var close = new SfButton
        {
            Text = "\uE712",
            FontFamily = "MaterialAssets",
            FontSize = 16,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(MutedColor),
            WidthRequest = 40,
            HeightRequest = 40,
            CornerRadius = 10,
            Padding = new Thickness(0)
        };
        close.Clicked += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        var header = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            Padding = new Thickness(28, 20, 20, 8)
        };
        header.Add(left);
        header.Add(close, 1);
        return header;
    }

    private View BuildProfileBanner()
    {
        var dot = new Ellipse { WidthRequest = 9, HeightRequest = 9, Fill = Color.FromArgb("#007A56"), VerticalOptions = LayoutOptions.Center };

        var caption = new Label
        {
            Text = "Current Profile:",
            FontFamily = "OpenSansRegular",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };

        var name = new Label
        {
            Text = $"{viewModel.TopUpProfileName} ({viewModel.TopUpCarrierName})",
            FontFamily = "OpenSansRegular",
            FontSize = 13,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Center
        };

        var left = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        left.Add(dot);
        left.Add(caption);
        left.Add(name);

        var remaining = new Label
        {
            Text = viewModel.TopUpRemainingText,
            FontFamily = MonoFont,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(PrimaryColor),
            VerticalOptions = LayoutOptions.Center
        };

        var separator = new Label
        {
            Text = "•",
            FontFamily = "OpenSansRegular",
            FontSize = 13,
            TextColor = Color.FromArgb("#C3C5D9"),
            VerticalOptions = LayoutOptions.Center
        };

        var days = new Label
        {
            Text = viewModel.TopUpDaysLeftText,
            FontFamily = MonoFont,
            FontSize = 13,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Center
        };

        var right = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        right.Add(remaining);
        right.Add(separator);
        right.Add(days);

        var banner = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        banner.Add(left);
        banner.Add(right, 1);

        var host = new Border
        {
            BackgroundColor = Color.FromArgb(SurfaceLow),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(16, 11)
        };
        host.Content = banner;

        var wrapper = new Border { Padding = new Thickness(28, 4, 28, 0), StrokeThickness = 0 };
        wrapper.Content = host;
        return wrapper;
    }

    private View BuildTierSection()
    {
        var label = new Label
        {
            Text = "SELECT DATA EXTENSION TIER",
            FontFamily = "OpenSansRegular",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(MutedColor)
        };

        var body = new VerticalStackLayout { Spacing = 10 };
        body.Add(label);
        body.Add(tierContainer);

        var wrapper = new Border { Padding = new Thickness(28, 18, 28, 0), StrokeThickness = 0 };
        wrapper.Content = body;
        return wrapper;
    }

    private View BuildTierCard(TopUpPack pack)
    {
        var selected = viewModel.IsTopUpSelected(pack);

        var size = new Label
        {
            Text = pack.DataLabel,
            FontFamily = "OpenSansRegular",
            FontSize = 19,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(selected ? PrimaryColor : InkColor),
            VerticalOptions = LayoutOptions.Center
        };

        if (isCompact) return BuildCompactTierCard(pack, selected, size);

        var radio = new Border
        {
            WidthRequest = 22,
            HeightRequest = 22,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(11) },
            BackgroundColor = Color.FromArgb(selected ? PrimaryColor : SurfaceHighest),
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = selected ? "\u2713" : string.Empty,
                FontFamily = "OpenSansRegular",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var heading = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        heading.Add(size);
        heading.Add(radio, 1);

        var extension = new Label
        {
            Text = pack.ExtensionLabel,
            FontFamily = MonoFont,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#006B4F"),
            Margin = new Thickness(0, 8, 0, 0)
        };

        var description = new Label
        {
            Text = pack.Description,
            FontFamily = "OpenSansRegular",
            FontSize = 12,
            TextColor = Color.FromArgb(MutedColor),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var price = new Label
        {
            Text = pack.PriceText,
            FontFamily = "OpenSansRegular",
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor)
        };

        var priceRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, VerticalOptions = LayoutOptions.End };
        priceRow.Add(price);

        if (pack.IsPopular)
        {
            var perGb = new Label
            {
                Text = pack.PricePerGbText,
                FontFamily = MonoFont,
                FontSize = 12,
                TextColor = Color.FromArgb(MutedColor),
                VerticalOptions = LayoutOptions.End
            };
            priceRow.Add(perGb, 1);
        }

        var body = new VerticalStackLayout { Spacing = 0 };
        body.Add(heading);
        body.Add(extension);
        body.Add(description);
        body.Add(new BoxView { HeightRequest = 1, Color = Colors.Transparent, Margin = new Thickness(0, 14, 0, 0) });
        body.Add(priceRow);

        var card = new Border
        {
            BackgroundColor = Color.FromArgb(selected ? "#FFFFFF" : SurfaceLow),
            Stroke = Color.FromArgb(selected ? PrimaryColor : SurfaceLow),
            StrokeThickness = selected ? 2 : 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            Padding = new Thickness(14, 16),
            Content = body
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => viewModel.SelectTopUpPackCommand.Execute(pack);
        card.GestureRecognizers.Add(tap);

        tierCards.Add(card);
        tierRadios.Add(radio);
        tierSizes.Add(size);

        if (pack.Tag is { } tag)
        {
            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(pack.IsPopular ? PrimaryColor : SurfaceHighest),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                Padding = new Thickness(10, 3),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Content = new Label
                {
                    Text = tag.ToUpperInvariant(),
                    FontFamily = "OpenSansRegular",
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb(pack.IsPopular ? "#FFFFFF" : PrimaryColor)
                }
            };

            var stack = new Grid();
            stack.Add(badge);
            stack.Add(card);
            return stack;
        }

        return card;
    }

    private View BuildCompactTierCard(TopUpPack pack, bool selected, Label size)
    {
        var radio = new Border
        {
            WidthRequest = 22,
            HeightRequest = 22,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(11) },
            BackgroundColor = Color.FromArgb(selected ? PrimaryColor : "#FFFFFF"),
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = selected ? "\u2713" : string.Empty,
                FontFamily = "OpenSansRegular",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var extension = new Label
        {
            Text = pack.ExtensionLabel.Replace(" extension", string.Empty),
            FontFamily = MonoFont,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#006B4F"),
            VerticalOptions = LayoutOptions.Center
        };

        var description = new Label
        {
            Text = pack.Description,
            FontFamily = "OpenSansRegular",
            FontSize = 12,
            TextColor = Color.FromArgb(MutedColor),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var titleRow = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        titleRow.Add(size);
        titleRow.Add(extension);

        var text = new VerticalStackLayout { Spacing = 2 };
        text.Add(titleRow);
        text.Add(description);

        var left = new HorizontalStackLayout { Spacing = 12 };
        left.Add(radio);
        left.Add(text);

        var price = new Label
        {
            Text = pack.PriceText,
            FontFamily = "OpenSansRegular",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(selected ? PrimaryColor : InkColor),
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Start
        };

        var perGb = new Label
        {
            Text = pack.PricePerGbText,
            FontFamily = MonoFont,
            FontSize = 11,
            TextColor = Color.FromArgb(MutedColor),
            HorizontalTextAlignment = TextAlignment.End
        };

        var right = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.End };
        right.Add(price);
        right.Add(perGb);

        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 12 };
        row.Add(left);
        row.Add(right, 1);

        var card = new Border
        {
            BackgroundColor = Color.FromArgb(selected ? "#E8EFFF" : SurfaceLow),
            Stroke = Color.FromArgb(selected ? PrimaryColor : SurfaceLow),
            StrokeThickness = selected ? 2 : 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            Padding = new Thickness(14, 12),
            Content = row
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => viewModel.SelectTopUpPackCommand.Execute(pack);
        card.GestureRecognizers.Add(tap);

        tierCards.Add(card);
        tierRadios.Add(radio);
        tierSizes.Add(size);

        if (pack.Tag is not { } tag) return card;

        var badge = new Border
        {
            BackgroundColor = Color.FromArgb(pack.IsPopular ? PrimaryColor : SurfaceHighest),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(10, 3),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = tag.ToUpperInvariant(),
                FontFamily = "OpenSansRegular",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(pack.IsPopular ? "#FFFFFF" : PrimaryColor)
            }
        };

        var stack = new Grid();
        stack.Add(badge);
        stack.Add(card);
        return stack;
    }

    private Label BuildAllowanceLabel() => new Label
    {
        Text = viewModel.TopUpAllowanceSummary,
        FontFamily = MonoFont,
        FontSize = 13,
        FontAttributes = FontAttributes.Bold,
        TextColor = Color.FromArgb(InkColor),
        VerticalOptions = LayoutOptions.Center
    };

    private View BuildSummarySection()
    {
        var speedIcon = new Label
        {
            Text = "\u21E7",
            FontFamily = "OpenSansRegular",
            FontSize = 15,
            TextColor = Color.FromArgb("#006B4F"),
            VerticalOptions = LayoutOptions.Center
        };
        var speedLabel = new Label
        {
            Text = "Provisioning Speed",
            FontFamily = "OpenSansRegular",
            FontSize = 13,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Center
        };
        var speedLeft = new HorizontalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        speedLeft.Add(speedIcon);
        speedLeft.Add(speedLabel);
        var speedRight = new Label
        {
            Text = "Instant Activation Upon Payment",
            FontFamily = "OpenSansRegular",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };
        var speedRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        speedRow.Add(speedLeft);
        speedRow.Add(speedRight, 1);

        var allowanceLeft = new Label
        {
            Text = "Selected Top-Up Allowance",
            FontFamily = "OpenSansRegular",
            FontSize = 13,
            TextColor = Color.FromArgb(MutedColor),
            VerticalOptions = LayoutOptions.Center
        };
        var allowanceRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        allowanceRow.Add(allowanceLeft);
        allowanceRow.Add(allowanceLabel, 1);

        var cardBadge = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(5) },
            Padding = new Thickness(8, 2),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = "VISA",
                FontFamily = "OpenSansRegular",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(PrimaryColor)
            }
        };

        var cardLabel = new Label
        {
            Text = "Auto-applied card ending in 4242",
            FontFamily = "OpenSansRegular",
            FontSize = 13,
            TextColor = Color.FromArgb(InkColor),
            VerticalOptions = LayoutOptions.Center
        };
        var cardLeft = new HorizontalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center };
        cardLeft.Add(cardBadge);
        cardLeft.Add(cardLabel);

        var change = new SfButton
        {
            Text = "Change",
            FontFamily = "OpenSansRegular",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            Background = Colors.Transparent,
            TextColor = Color.FromArgb(PrimaryColor),
            Padding = new Thickness(8, 4),
            MinimumHeightRequest = 32
        };
        change.Clicked += (_, _) => ChangePaymentRequested?.Invoke(this, EventArgs.Empty);

        var cardRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        cardRow.Add(cardLeft);
        cardRow.Add(change, 1);

        var body = new VerticalStackLayout { Spacing = 10 };
        body.Add(speedRow);
        body.Add(allowanceRow);
        body.Add(cardRow);

        var host = new Border
        {
            BackgroundColor = Color.FromArgb("#F5F8FF"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Padding = new Thickness(16)
        };
        host.Content = body;

        var wrapper = new Border { Padding = new Thickness(28, 18, 28, 0), StrokeThickness = 0 };
        wrapper.Content = host;
        return wrapper;
    }

    private Label BuildPurchaseLabel() => new Label
    {
        Text = viewModel.TopUpSubmitLabel,
        FontFamily = "OpenSansRegular",
        FontSize = 15,
        FontAttributes = FontAttributes.Bold,
        TextColor = Colors.White,
        VerticalOptions = LayoutOptions.Center
    };

    private View BuildFooter()
    {
        var purchase = new SfButton
        {
            FontFamily = "OpenSansRegular",
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            Background = Color.FromArgb(PrimaryColor),
            TextColor = Colors.White,
            CornerRadius = 10,
            Padding = new Thickness(16, 6),
            HeightRequest = 44,
            MinimumHeightRequest = 44,
            Content = new DataTemplate(() =>
            {
                var row = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(8, 0), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                row.Add(new Label
                {
                    Text = "\uE761",
                    FontFamily = "MaterialAssets",
                    FontSize = 15,
                    TextColor = Colors.White,
                    VerticalOptions = LayoutOptions.Center
                });
                row.Add(purchaseLabel);
                return row;
            })
        };
        purchase.Clicked += (_, _) => PurchaseRequested?.Invoke(this, EventArgs.Empty);

        var cancel = new SfButton
        {
            Text = "Cancel",
            FontFamily = "OpenSansRegular",
            FontSize = 15,
            Background = Color.FromArgb(SurfaceLow),
            TextColor = Color.FromArgb(InkColor),
            CornerRadius = 10,
            Padding = new Thickness(20, 6),
            HeightRequest = 44,
            MinimumHeightRequest = 44
        };
        cancel.Clicked += (_, _) => CancelRequested?.Invoke(this, EventArgs.Empty);

        var actions = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 12,
            HorizontalOptions = LayoutOptions.End
        };
        actions.Add(cancel);
        actions.Add(purchase, 1);

        var footer = new Border { Padding = new Thickness(28, 16, 28, 22), StrokeThickness = 0 };
        footer.Content = actions;
        return footer;
    }
}
