using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIChatSample;

/// <summary>One image card in the Image Library / Media Gallery grid.</summary>
public class ImageItemModel : INotifyPropertyChanged
{
    /// <summary>Stable identifier for the image.</summary>
    public string? Id { get; set; }

    /// <summary>Path to the full-size image file.</summary>
    public string? ImagePath { get; set; }

    /// <summary>Display name for the image.</summary>
    public string? ImageName { get; set; }

    /// <summary>Comma-separated tags used for filter / search.</summary>
    public string? Tags { get; set; }

    /// <summary>Category (e.g. "Marketing", "Standup") used by the filter strip.</summary>
    public string? Category { get; set; }

    /// <summary>Long-form description shown in the image details panel.</summary>
    public string? Description { get; set; }

    /// <summary>Pre-formatted creation date string.</summary>
    public string? CreatedDate { get; set; }

    /// <summary>Optional separate thumbnail path; falls back to <see cref="ImagePath"/>.</summary>
    public string? ThumbnailPath { get; set; }

    private bool isSelected;

    /// <summary>True while the user has the image selected (multi-select grid).</summary>
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
