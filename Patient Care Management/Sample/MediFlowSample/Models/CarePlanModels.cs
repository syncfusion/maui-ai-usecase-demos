using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace MediFlowSample.Models;

public sealed class CareTask
{
    public string TaskId { get; init; } = string.Empty;
    public string PatientId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Priority { get; init; } = "Normal";
    public string DueDisplay { get; init; } = string.Empty;
    public string Status { get; init; } = "Planned";

    public bool IsHighPriority => Priority == "High";

    public bool IsCompleted => Status == "Completed";

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public bool ShowTaskDetails => !IsCompleted;

    public bool ShowCompletedAction => IsCompleted;

    public string IndicatorHex => Status switch
    {
        "In Progress" => "#DC2626",
        "Completed" => "#087F73",
        _ => "#14213D"
    };

    public Color IndicatorColorValue => Color.FromArgb(IndicatorHex);

    public Color CardBackgroundValue => Status switch
    {
        "Planned" => Color.FromArgb("#EEF4FF"),
        _ => Colors.White
    };

    public string ActionIconGlyph => Status switch
    {
        "In Progress" => "\uE5CA",
        "Completed" => "\uE86C",
        _ => string.Empty
    };

    public Color ActionBackgroundValue => Status == "In Progress" ? Color.FromArgb("#087F73") : Colors.Transparent;

    public Color ActionIconColorValue => Status == "In Progress" ? Colors.White : Color.FromArgb("#087F73");

    public bool HasActionIcon => !string.IsNullOrEmpty(ActionIconGlyph);

    public string StatusIconGlyph => Status switch
    {
        "In Progress" => "\uE8F4",
        "Completed" => "\uE86C",
        _ => "\uE8B5"
    };

    public TextDecorations TitleDecoration => IsCompleted ? TextDecorations.Strikethrough : TextDecorations.None;
}
