using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ListView_IntentGrouping.Models;

/// <summary>
/// A category (intent) produced by the AI analyzer. Acts as a group header
/// in the Syncfusion ListView and exposes the underlying items, an AI
/// generated summary, an icon, and a colour for the group header.
/// </summary>
public class IntentCategory : INotifyPropertyChanged
{
    private bool _isExpanded = true;
    private int _visibleCount;

    public IntentCategory()
    {
        Items = new ObservableCollection<FeedbackItem>();
    }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string SummaryTemplate { get; set; } = string.Empty;
    public string Icon { get; set; } = "\uE73E"; // default category glyph
    public string AccentColor { get; set; } = "#6750A4";
    public string IconBackground { get; set; } = "#EADDFF";

    public ObservableCollection<FeedbackItem> Items { get; }

    public int TotalCount => Items.Count;

    private int _averageRating;
    public int AverageRating
    {
        get => _averageRating;
        set { if (_averageRating != value) { _averageRating = value; OnPropertyChanged(); } }
    }

    public int HighPriorityCount
    {
        get
        {
            int count = 0;
            foreach (var item in Items)
            {
                if (item.Priority >= 4) count++;
            }
            return count;
        }
    }

    public int VisibleCount
    {
        get => _visibleCount;
        set { if (_visibleCount != value) { _visibleCount = value; OnPropertyChanged(); } }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExpandGlyph));
        }
    }

    public string ExpandGlyph => _isExpanded ? "\uE74C" : "\uE76C";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
