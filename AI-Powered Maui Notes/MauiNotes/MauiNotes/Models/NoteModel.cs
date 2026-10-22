using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiNotes.Models;

/// <summary>
/// Represents a single note. Title and Content are observable so that
/// two-way binding from the editor and the list view keep the UI in sync.
/// </summary>
public class NoteModel : INotifyPropertyChanged
{
    public Guid Id { get; init; } = Guid.NewGuid();

    private string title = string.Empty;
    private string content = string.Empty;
    private string previewText = string.Empty;
    private DateTime createdDate;
    private DateTime modifiedDate;
    private string groupName = string.Empty;
    private bool isHovered;
    private bool isSelected;
    private bool isLocked;

    /// <summary>Display name shown in the list and used for grouping/sorting.</summary>
    public string Title
    {
        get => title;
        set
        {
            if (title != value)
            {
                title = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Rich-text (HTML) body of the note, bound to the custom editor.</summary>
    public string Content
    {
        get => content;
        set
        {
            if (content != value)
            {
                content = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Timestamp assigned once when the note is created. Never changes when
    /// the note is edited, renamed, or selected.
    /// </summary>
    public DateTime CreatedDate
    {
        get => createdDate;
        set
        {
            if (createdDate != value)
            {
                createdDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EffectiveDate));
                OnPropertyChanged(nameof(SubtitleText));
                OnPropertyChanged(nameof(PreviewLine));
                OnPropertyChanged(nameof(EditorTimestampText));
            }
        }
    }

    /// <summary>Last-modified timestamp; used for grouping into Today / Yesterday.</summary>
    public DateTime ModifiedDate
    {
        get => modifiedDate;
        set
        {
            if (modifiedDate != value)
            {
                modifiedDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EffectiveDate));
                OnPropertyChanged(nameof(SubtitleText));
                OnPropertyChanged(nameof(PreviewLine));
                OnPropertyChanged(nameof(EditorTimestampText));
            }
        }
    }

    /// <summary>
    /// The timestamp that should be shown to the user: the modification
    /// time once the note has actually been modified, otherwise the
    /// creation time.
    /// </summary>
    public DateTime EffectiveDate => ModifiedDate > CreatedDate ? ModifiedDate : CreatedDate;

    /// <summary>
    /// History-style tree-view subtitle derived from <see cref="EffectiveDate"/>:
    /// a time for today, "Yesterday", a weekday name for the previous
    /// calendar week, or a short date for anything older.
    /// </summary>
    public string SubtitleText
    {
        get
        {
            DateTime effective = EffectiveDate;
            DateTime effectiveDay = effective.Date;
            DateTime today = DateTime.Today;

            if (effectiveDay == today)
                return effective.ToString("h:mm tt");

            if (effectiveDay == today.AddDays(-1))
                return "Yesterday";

            // "Previous calendar week" = within the last 7 days (but not
            // today/yesterday, already handled above) using a consistent
            // calendar-day boundary rather than elapsed 24-hour periods.
            if (effectiveDay > today.AddDays(-7))
                return effective.ToString("dddd");

            return effective.ToString("dd/MM/yyyy");
        }
    }

    /// <summary>
    /// Full read-only timestamp shown above the RTE, e.g.
    /// "21 September 2026 at 3:08 PM".
    /// </summary>
    public string EditorTimestampText => EffectiveDate.ToString("d MMMM yyyy 'at' h:mm tt");

    /// <summary>
    /// Plain-text snippet of the note body (everything after the first
    /// line/title). Shown next to the date/time in the list row. Defaults
    /// to a placeholder when the note has no body content yet.
    /// </summary>
    public string PreviewText
    {
        get => previewText;
        set
        {
            if (previewText != value)
            {
                previewText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewLine));
            }
        }
    }

    /// <summary>Combined "{time}  {preview}" string used in list rows.</summary>
    public string PreviewLine =>
        $"{SubtitleText}   {(string.IsNullOrWhiteSpace(PreviewText) ? "No additional text" : PreviewText)}";

    /// <summary>
    /// Name of the group the note belongs to (e.g. "Today"). Used as the
    /// SfListView GroupDescriptor PropertyName so the control can produce
    /// expand/collapsible GroupResult headers automatically.
    /// </summary>
    public string GroupName
    {
        get => groupName;
        set
        {
            if (groupName != value)
            {
                groupName = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsHovered
    {
        get => isHovered;
        set
        {
            if (isHovered != value)
            {
                isHovered = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected != value)
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsLocked
    {
        get => isLocked;
        set
        {
            if (isLocked != value)
            {
                isLocked = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
