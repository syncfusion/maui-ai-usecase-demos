using System.ComponentModel;

namespace AIChatSample;

/// <summary>
/// Backing model for the Create / Configure Agent form.
/// Implements <see cref="INotifyPropertyChanged"/> so the form's two-way
/// bindings update the ViewModel when fields change directly.
/// </summary>
public class AgentConfiguration : INotifyPropertyChanged
{
    private string? id;
    private string? agentName;
    private string? agentDescription;
    private string? instructions;
    private string? companyName;
    private string? color;
    private string? initial;
    private string? avatarSource;
    private string? avatarGlyph;

    /// <summary>
    /// Stable unique identifier for the agent. Assigned at creation
    /// time and used to bind chat sessions / nav-list rows
    /// back to a specific agent configuration.
    /// </summary>
    public string? Id
    {
        get => this.id;
        set => this.SetProperty(ref this.id, value);
    }

    /// <summary>Gets or sets the agent display name.</summary>
    public string? AgentName
    {
        get => this.agentName;
        set => this.SetProperty(ref this.agentName, value);
    }

    /// <summary>Gets or sets the short description shown on the agent card.</summary>
    public string? AgentDescription
    {
        get => this.agentDescription;
        set => this.SetProperty(ref this.agentDescription, value);
    }

    /// <summary>Gets or sets the system instructions for the agent.</summary>
    public string? Instructions
    {
        get => this.instructions;
        set => this.SetProperty(ref this.instructions, value);
    }

    /// <summary>Gets or sets the company name associated with the agent.</summary>
    public string? CompanyName
    {
        get => this.companyName;
        set => this.SetProperty(ref this.companyName, value);
    }

    /// <summary>Gets or sets the timestamp (UTC) when the agent was created.</summary>
    public DateTime CreatedUtc
    {
        get => this.createdUtc;
        set => this.SetProperty(ref this.createdUtc, value);
    }
    private DateTime createdUtc;

    /// <summary>
    /// Background color used for the agent avatar (hex, e.g. <c>#7633DA</c>).
    /// Picked from a small palette so newly created agents visually
    /// stand apart without forcing the user through a color picker.
    /// Editable later via the logo editor.
    /// </summary>
    public string? Color
    {
        get => this.color;
        set => this.SetProperty(ref this.color, value);
    }

    /// <summary>
    /// Single character used as the agent avatar glyph when no image
    /// is uploaded. Derived from the first non-whitespace character
    /// of <see cref="AgentName"/>, upper-cased.
    /// </summary>
    public string? Initial
    {
        get => this.initial;
        set => this.SetProperty(ref this.initial, value);
    }

    /// <summary>Optional uploaded image path used by the logo editor.</summary>
    public string? AvatarSource
    {
        get => this.avatarSource;
        set => this.SetProperty(ref this.avatarSource, value);
    }

    /// <summary>Optional glyph/fallback avatar definition used by the logo editor.</summary>
    public string? AvatarGlyph
    {
        get => this.avatarGlyph;
        set => this.SetProperty(ref this.avatarGlyph, value);
    }

    /// <summary>
    /// Populates <see cref="Id"/>, <see cref="Initial"/>, and
    /// <see cref="Color"/> from the current <see cref="AgentName"/>.
    /// Idempotent — safe to call more than once. Used by the create
    /// flow and exposed publicly so the AgentLogoEditor
    /// can re-derive <see cref="Color"/> when the name changes.
    /// </summary>
    public void EnsureIdentityFields()
    {
        if (string.IsNullOrEmpty(this.Id))
        {
            this.Id = Guid.NewGuid().ToString("N");
        }

        this.Initial = DeriveInitial(this.AgentName);
        this.Color = DeriveColor(this.AgentName);
    }

    /// <summary>
    /// Returns the first non-whitespace character of <paramref name="name"/>,
    /// upper-cased, or <c>"?"</c> if <paramref name="name"/> is null /
    /// empty. Pure function — exposed <c>internal</c> so unit tests
    /// can assert on the derivation rule.
    /// </summary>
    internal static string DeriveInitial(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        foreach (var ch in name.Trim())
        {
            if (!char.IsWhiteSpace(ch))
            {
                return char.ToUpperInvariant(ch).ToString();
            }
        }

        return "?";
    }

    /// <summary>
    /// Picks a deterministic background color from a small palette
    /// based on the hash of <paramref name="name"/>. The same name
    /// always maps to the same color, so a "Code Helper" agent
    /// keeps its visual identity even after the logo editor is
    /// used. Exposed <c>internal</c> so unit tests can assert on the
    /// derivation rule.
    /// </summary>
    internal static string DeriveColor(string? name)
    {
        // Small palette lifted from the UI Kit avatar swatches so the
        // generated avatars look at home next to the static ones.
        string[] palette =
        {
            "#7633DA", // primary purple
            "#1F8A70", // teal
            "#D55E2C", // warm orange
            "#2F6FED", // blue
            "#B5308E", // magenta
            "#0E7C7B", // dark teal
            "#8D4CF5", // light purple
            "#3A6B35", // green
        };

        if (string.IsNullOrWhiteSpace(name))
        {
            return palette[0];
        }

        var hash = 0;
        foreach (var ch in name.Trim())
        {
            hash = unchecked((hash * 31) + char.ToUpperInvariant(ch));
        }

        var index = ((hash % palette.Length) + palette.Length) % palette.Length;
        return palette[index];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return;
        }

        storage = value;
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}