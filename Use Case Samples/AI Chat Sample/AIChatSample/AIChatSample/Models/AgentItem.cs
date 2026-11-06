using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample;

/// <summary>
/// View-model-friendly wrapper around an <see cref="AgentConfiguration"/>
/// for the navigation-drawer agent list. Carries the icon/initial/colour
/// data the drawer binds to so it can render an agent row without
/// referencing the form-bound <see cref="AgentConfiguration"/> directly.
/// </summary>
public class AgentItem : INotifyPropertyChanged
{
    private bool isSelected;
    private string? initial;
    private Color? color;
    private string? id;
    private string? avatarSource;
    private string? avatarGlyph;

    /// <summary>Display name shown in the agent row.</summary>
    public string? Name { get; set; }

    /// <summary>Icon-glyph used when the row renders as an icon (legacy field).</summary>
    public string? Glyph { get; set; }

    /// <summary>Stable id copied from <see cref="AgentConfiguration.Id"/>; lets chat sessions / nav rows bind back to the same record.</summary>
    public string? Id
    {
        get => this.id;
        set
        {
            if (this.id == value) return;
            this.id = value;
            this.OnPropertyChanged(nameof(this.Id));
        }
    }

    /// <summary>Single-character avatar initial derived from the agent name.</summary>
    public string? Initial
    {
        get => this.initial;
        set
        {
            if (this.initial == value) return;
            this.initial = value;
            this.OnPropertyChanged(nameof(this.Initial));
        }
    }

    /// <summary>Avatar background color (hex) lifted from <see cref="AgentConfiguration.Color"/>.</summary>
    public Color? Color
    {
        get => this.color;
        set
        {
            if (this.color == value) return;
            this.color = value;
            this.OnPropertyChanged(nameof(this.Color));
        }
    }

    /// <summary>Optional uploaded image path used by the logo editor.</summary>
    public string? AvatarSource
    {
        get => this.avatarSource;
        set
        {
            if (this.avatarSource == value) return;
            this.avatarSource = value;
            this.OnPropertyChanged(nameof(this.AvatarSource));
            this.OnPropertyChanged(nameof(this.ShowInitial));
        }
    }

    /// <summary>Optional "FontFamily;Glyph;Color" avatar definition produced by the AgentLogo popup.</summary>
    public string? AvatarGlyph
    {
        get => this.avatarGlyph;
        set
        {
            if (this.avatarGlyph == value) return;
            this.avatarGlyph = value;
            this.OnPropertyChanged(nameof(this.AvatarGlyph));
            this.OnPropertyChanged(nameof(this.ShowIconAvatar));
            this.OnPropertyChanged(nameof(this.ShowInitial));
            this.OnPropertyChanged(nameof(this.IconAvatar));
        }
    }

    /// <summary>True while this row is the currently-selected agent in the drawer.</summary>
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

    /// <summary>
    /// True when the agent has a custom icon avatar (i.e. the AvatarGlyph
    /// string is populated with the encoded "FontFamily;Glyph;Color" pair
    /// produced by the AgentLogo popup). Used by the nav drawer so the
    /// icon Image is only shown when an icon is actually configured.
    /// </summary>
    public bool ShowIconAvatar => !string.IsNullOrEmpty(this.AvatarGlyph) && this.AvatarGlyph!.Contains(';');

    /// <summary>
    /// True when the nav drawer should fall back to the letter-based
    /// initial (i.e. no uploaded image and no custom icon).
    /// </summary>
    public bool ShowInitial =>
        string.IsNullOrEmpty(this.AvatarSource) &&
        !this.ShowIconAvatar;

    /// <summary>
    /// Built from <see cref="AvatarGlyph"/> so the nav-drawer avatar
    /// Image can render a Create-mode icon. Mirrors
    /// <c>AgentLogoPopupViewModel.IconAvatar</c>.
    /// </summary>
    public ImageSource? IconAvatar
    {
        get
        {
            if (string.IsNullOrEmpty(this.avatarGlyph) || !this.avatarGlyph!.Contains(';'))
            {
                return null;
            }

            var parts = this.avatarGlyph.Split(';');
            if (parts.Length < 2) return null;
            // The third part of the encoded avatar is the *background* color
            // (matches the popup's live preview). The icon glyph itself
            // should be a contrasting color so it stays visible on the
            // selected swatch — use white as a safe default.
            return new FontImageSource
            {
                FontFamily = parts[0],
                Glyph = parts[1],
                Color = Colors.White,
                Size = 18
            };
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied property name.</summary>
    protected void OnPropertyChanged([CallerMemberName] string? p = null) =>
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}

public class RecentChatItem
{
    public string? Title { get; set; }
    public string? Time { get; set; }
    public string? Glyph { get; set; }

    /// <summary>
    /// Carries the underlying <see cref="ChatSession"/> (or its id as a
    /// string) so the host view-model can resolve the row back to a
    /// stored session. Set by <c>AIMainLayoutViewModel</c> when
    /// materialising the Recent chats list.
    /// </summary>
    public object? Tag { get; set; }
}
