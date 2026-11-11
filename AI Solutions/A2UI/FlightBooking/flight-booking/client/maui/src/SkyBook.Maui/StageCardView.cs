using Microsoft.Maui.Controls.Shapes;
using SkyBook.Core;

namespace SkyBook.Maui;

/// <summary>Welcome-screen stage card (React: <c>.stage-card</c>).</summary>
public sealed class StageCardView : Border
{
    public StageCardView(Stage stage, Action<Stage> onOpen, bool compactSamples = false)
    {
        var accent = stage.Id switch
        {
            StageId.Search => Color.FromArgb("#2563eb"),
            StageId.Results => Color.FromArgb("#7c3aed"),
            _ => Color.FromArgb("#ca8a04"),
        };
        StrokeShape = new RoundRectangle { CornerRadius = 14 };
        Stroke = Color.FromArgb("#cbd5e1");
        StrokeThickness = 1;
        BackgroundColor = Colors.White;
        Padding = new Thickness(20, 18);
        AutomationId = $"stage-{stage.Id.ToString().ToLowerInvariant()}";
        SemanticProperties.SetDescription(this, $"Step {stage.Step}: {stage.Title}");

        var icon = new Border
        {
            WidthRequest = 46, HeightRequest = 46, StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Background = new LinearGradientBrush([new GradientStop(accent, 0), new GradientStop(accent.WithAlpha(.75f), 1)], new Point(0, 0), new Point(1, 1)),
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label { Text = stage.Icon, FontSize = 22, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
        };
        var stepBadge = new Label { Text = $"Step {stage.Step}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#64748b"), HorizontalOptions = LayoutOptions.End };

        var top = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)] };
        top.Add(icon); top.Add(stepBadge, 1);

        var samples = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, Margin = new Thickness(0, 4, 0, 0) };
        foreach (var s in stage.Samples)
            samples.Add(new Border
            {
                Margin = new Thickness(0, 0, 6, 6), Padding = new Thickness(9, 3), StrokeThickness = 1,
                Stroke = accent.WithAlpha(.35f), BackgroundColor = accent.WithAlpha(.08f),
                StrokeShape = new RoundRectangle { CornerRadius = 999 },
                Content = new Label { Text = s, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = accent },
            });

        var highlights = new VerticalStackLayout { Spacing = 4 };
        foreach (var h in stage.Highlights)
            highlights.Add(new HorizontalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label { Text = "✓", TextColor = Color.FromArgb("#14a37f"), FontAttributes = FontAttributes.Bold, FontSize = 13 },
                    new Label { Text = h, TextColor = Color.FromArgb("#1e293b"), FontSize = 13 },
                },
            });

        var inner = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                top,
                new Label { Text = stage.Title, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0f172a") },
                new Label { Text = stage.Copy, FontSize = 14, TextColor = Color.FromArgb("#334155") },
                samples,
                highlights,
                new Label { Text = $"{stage.Cta} →", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#2563eb"), Margin = new Thickness(0, 6, 0, 0) },
            },
        };
        
        Content = new Grid
        {
            RowDefinitions = [new(GridLength.Auto), new(GridLength.Star)],
            RowSpacing = 0,
            Children =
            {
                new BoxView { HeightRequest = 3, Margin = new Thickness(-20, -18, -20, 14), VerticalOptions = LayoutOptions.Start,
                    Background = new LinearGradientBrush([new GradientStop(Color.FromArgb("#2563eb"), 0), new GradientStop(Color.FromArgb("#22ad78"), 1)], new Point(0, 0), new Point(1, 0)) },
            },
        };
        ((Grid)Content).Add(inner, 0, 1);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => onOpen(stage);
        GestureRecognizers.Add(tap);
    }
}
