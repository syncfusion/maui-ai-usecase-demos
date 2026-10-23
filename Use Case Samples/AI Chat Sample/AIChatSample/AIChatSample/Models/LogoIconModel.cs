using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample;

/// <summary>One selectable icon in the Agent Logo popup (Create mode).</summary>
public class LogoIconModel : INotifyPropertyChanged
{
    /// <summary>Stable identifier for the icon.</summary>
    public string? Id { get; set; }

    /// <summary>Human-readable name (e.g. "Bolt").</summary>
    public string? IconName { get; set; }

    /// <summary>Font-glyph character (private-use Unicode codepoint).</summary>
    public string? IconGlyph { get; set; }

    /// <summary>Font family that renders <see cref="IconGlyph"/>.</summary>
    public string? IconFontFamily { get; set; }

    private bool isSelected;

    /// <summary>True while this icon is the currently-selected one in the popup.</summary>
    public bool IsSelected
    {
        get => this.isSelected;
        set { if (this.isSelected == value) return; this.isSelected = value; this.OnPropertyChanged(); }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied property name.</summary>
    protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}

/// <summary>One selectable color swatch in the Agent Logo popup (Create mode).</summary>
public class LogoColorModel : INotifyPropertyChanged
{
    /// <summary>Stable identifier for the color.</summary>
    public string? Id { get; set; }

    /// <summary>Hex representation of the color (e.g. <c>#7633DA</c>).</summary>
    public string? ColorHex { get; set; }

    /// <summary>Human-readable name (e.g. "Iris").</summary>
    public string? ColorName { get; set; }

    private bool isSelected;

    /// <summary>True while this swatch is the currently-selected one in the popup.</summary>
    public bool IsSelected
    {
        get => this.isSelected;
        set { if (this.isSelected == value) return; this.isSelected = value; this.OnPropertyChanged(); }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied property name.</summary>
    protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}
