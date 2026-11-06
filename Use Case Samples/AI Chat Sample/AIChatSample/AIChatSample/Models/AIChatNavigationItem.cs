using System.ComponentModel;

namespace AIChatSample;

/// <summary>
/// One row in the <c>AIChatNavigationPanel</c> primary-items list (e.g.
/// "New chat", "Search", "Library"). Holds the routing key the drawer
/// uses to push the matching page and the icon / hover state the panel
/// binds to.
/// </summary>
public sealed class AIChatNavigationItem : INotifyPropertyChanged
{
    /// <summary>Routing key passed to <c>ChatNavigationService.Handle</c>.</summary>
    public required string Key { get; init; }

    /// <summary>Display title rendered next to the row's icon.</summary>
    public required string Title { get; init; }

    /// <summary>Icon glyph (private-use Unicode codepoint for the FontIcons font).</summary>
    public required string Icon { get; init; }

    /// <summary>Font family used to render <see cref="Icon"/>.</summary>
    public required string IconFontFamily { get; init; }

    private Color backgroundColor = Colors.Transparent;

    /// <summary>Selected / hover background for the row.</summary>
    public Color BackgroundColor
    {
        get => backgroundColor;
        set
        {
            if (backgroundColor == value)
            {
                return;
            }

            backgroundColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundColor)));
        }
    }

    /// <summary>True while the pointer is over the row (set by the panel's hover code-behind).</summary>
    public bool IsHovered { get; set; }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
}
