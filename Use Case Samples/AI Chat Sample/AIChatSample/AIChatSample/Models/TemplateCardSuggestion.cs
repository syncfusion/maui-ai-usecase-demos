using System.ComponentModel;
using Syncfusion.Maui.AIAssistView;

namespace AIChatSample;

/// <summary>
/// A suggestion card rendered inside the <c>SfAIAssistView</c> on
/// <c>CreateNewAgentPage</c> — either a Template card or a
/// "My Agents" entry, both of which pre-fill the agent-configure form
/// when tapped.
/// </summary>
public class TemplateCardSuggestion : AssistSuggestion, INotifyPropertyChanged
{
    private string? iconGlyph;
    private string? iconFontFamily;
    private string? iconBackground;
    private string? iconColor = "#FFFFFF";
    private string? titleText;
    private string? descriptionText;
    private CardSection section = CardSection.Template;
    private bool isAgentTemplate;

    /// <summary>Font-glyph character (UIKitIcons / MaterialAssets / FontIcons).</summary>
    public string? IconGlyph
    {
        get => this.iconGlyph;
        set
        {
            if (this.iconGlyph == value) return;
            this.iconGlyph = value;
            this.Raise(nameof(IconGlyph));
        }
    }

    /// <summary>Font family used by <see cref="IconGlyph"/>.</summary>
    public string? IconFontFamily
    {
        get => this.iconFontFamily;
        set
        {
            if (this.iconFontFamily == value) return;
            this.iconFontFamily = value;
            this.Raise(nameof(IconFontFamily));
        }
    }

    /// <summary>Background brush color for the 36x36 rounded icon slot.</summary>
    public string? IconBackground
    {
        get => this.iconBackground;
        set
        {
            if (this.iconBackground == value) return;
            this.iconBackground = value;
            this.Raise(nameof(IconBackground));
        }
    }

    /// <summary>Foreground color of the glyph inside the icon slot.</summary>
    public string? IconColor
    {
        get => this.iconColor;
        set
        {
            if (this.iconColor == value) return;
            this.iconColor = value;
            this.Raise(nameof(IconColor));
        }
    }

    /// <summary>Card title text (e.g. "Research agent").</summary>
    public string? TitleText
    {
        get => this.titleText;
        set
        {
            if (this.titleText == value) return;
            this.titleText = value;
            this.Raise(nameof(TitleText));
        }
    }

    /// <summary>Card description text (multi-line supported via LineBreakMode=WordWrap).</summary>
    public string? DescriptionText
    {
        get => this.descriptionText;
        set
        {
            if (this.descriptionText == value) return;
            this.descriptionText = value;
            this.Raise(nameof(DescriptionText));
        }
    }

    /// <summary>Which on-page section this card belongs to (Template / MyAgents).</summary>
    public CardSection Section
    {
        get => this.section;
        set
        {
            if (this.section == value) return;
            this.section = value;
            this.Raise(nameof(Section));
        }
    }

    /// <summary>When True, tapping the card routes the user to AgentConfigurePage.</summary>
    public bool IsAgentTemplate
    {
        get => this.isAgentTemplate;
        set
        {
            if (this.isAgentTemplate == value) return;
            this.isAgentTemplate = value;
            this.Raise(nameof(IsAgentTemplate));
        }
    }

    /// <inheritdoc />
    public new event PropertyChangedEventHandler? PropertyChanged;

    private void Raise(string propertyName) =>
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>Identifies which visible card section a suggestion belongs to.</summary>
public enum CardSection
{
    /// <summary>Static "Template" row of pre-built agent templates.</summary>
    Template,
    /// <summary>"My Agents" row of agents the user has created.</summary>
    MyAgents,
}
