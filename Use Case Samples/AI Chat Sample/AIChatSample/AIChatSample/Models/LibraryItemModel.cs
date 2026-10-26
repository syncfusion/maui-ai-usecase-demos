using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample;

/// <summary>Row kind surfaced in the Library list.</summary>
public enum LibraryItemType
{
    /// <summary>A saved chat session.</summary>
    Chat,
    /// <summary>A created agent.</summary>
    Agent,
}

/// <summary>One row in the Library / Knowledge Hub list.</summary>
public class LibraryItemModel : INotifyPropertyChanged
{
    /// <summary>Stable identifier for the row.</summary>
    public string? Id { get; set; }

    /// <summary>Row title.</summary>
    public string? Title { get; set; }

    /// <summary>Longer description preview.</summary>
    public string? Description { get; set; }

    /// <summary>Pre-formatted creation date.</summary>
    public string? CreatedDate { get; set; }

    /// <summary>Pre-formatted last-modified date.</summary>
    public string? ModifiedDate { get; set; }

    /// <summary>Whether the row is a chat or an agent — drives the type badge.</summary>
    public LibraryItemType Type { get; set; }

    /// <summary>Font-glyph used for the row's left icon.</summary>
    public string? IconGlyph { get; set; }

    private string? image;

    /// <summary>Optional image path rendered as the row's preview thumbnail.</summary>
    public string? Image
    {
        get => this.image;
        set
        {
            if (this.image == value)
            {
                return;
            }

            this.image = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>Font family used by <see cref="IconGlyph"/>.</summary>
    public string? IconFontFamily { get; set; }

    /// <summary>True when this row represents an agent.</summary>
    public bool IsAgent => Type == LibraryItemType.Agent;

    /// <summary>True when this row represents a chat session.</summary>
    public bool IsChat => Type == LibraryItemType.Chat;

    /// <summary>True when the "Share" action is available for this row.</summary>
    public bool CanShare { get; set; }

    /// <summary>True when the "Edit" action is available for this row.</summary>
    public bool CanEdit { get; set; }

    private bool isSelected;

    /// <summary>True while the row is selected (multi-select list).</summary>
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

    /// <summary>Pre-formatted "Created • Modified" subtitle line.</summary>
    public string Subtitle =>
        string.IsNullOrEmpty(CreatedDate) || string.IsNullOrEmpty(ModifiedDate)
            ? string.Empty
            : $"{CreatedDate} • {ModifiedDate}";

    /// <summary>Text shown on the type badge ("Chat" / "Agent").</summary>
    public string BadgeText => Type == LibraryItemType.Agent ? "Agent" : "Chat";

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied property name.</summary>
    protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}

/// <summary>Display model for the Library filter chip group.</summary>
public class LibraryFilterTab
{
    /// <summary>Label rendered on the chip.</summary>
    public string? Text { get; set; }

    /// <summary>Filter value associated with the chip.</summary>
    public LibraryFilter Filter { get; set; }
}

/// <summary>Filter values supported by the Library chip group.</summary>
public enum LibraryFilter
{
    /// <summary>Show every row.</summary>
    All,
    /// <summary>Only chat sessions.</summary>
    Chats,
    /// <summary>Only agents.</summary>
    Agents,
}
