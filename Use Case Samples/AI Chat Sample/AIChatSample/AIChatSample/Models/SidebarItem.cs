using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample;

/// <summary>One row in the legacy sidebar list (New chat, Search, Library, …).</summary>
public class SidebarItem : INotifyPropertyChanged
{
    private bool isSelected;

    /// <summary>Display title.</summary>
    public string? Title { get; set; }

    /// <summary>Font-glyph character for the row's left icon.</summary>
    public string? IconGlyph { get; set; }

    /// <summary>Font family used to render <see cref="IconGlyph"/>.</summary>
    public string? IconFontFamily { get; set; }

    /// <summary>Name of the page to push when the row is tapped.</summary>
    public string? PageName { get; set; }

    private FontImageSource? iconImageSource;

    /// <summary>Pre-built <see cref="FontImageSource"/> for the row's icon (set after construction).</summary>
    public FontImageSource? IconImageSource
    {
        get => this.iconImageSource;
        set
        {
            if (this.iconImageSource == value)
            {
                return;
            }

            this.iconImageSource = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>True while the row is the currently-selected one.</summary>
    public bool IsSelected
    {
        get => this.isSelected;
        set
        {
            if (this.isSelected == value) return;
            this.isSelected = value;
            this.OnPropertyChanged(nameof(this.IsSelected));
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied property name.</summary>
    protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}
