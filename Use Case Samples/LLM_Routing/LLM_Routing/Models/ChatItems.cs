using Syncfusion.Maui.AIAssistView;

namespace LLM_Routing.Models;

/// <summary>
/// UI-only item: a single AI response showing the selected model, generated content, and request/response telemetry.
/// </summary>
public sealed class AIAssistResponseItem : AssistItem
{
    private string modelName = string.Empty;
    private int inputTokens;
    private int outputTokens;
    private string costText = string.Empty;
    private bool isGenerating;

    public string ModelName
    {
        get => modelName;
        set
        {
            modelName = value;
            OnPropertyChanged(nameof(ModelName));
            OnPropertyChanged(nameof(ModelHeader));
        }
    }

    public string ModelHeader => string.IsNullOrWhiteSpace(ModelName) ? string.Empty : ModelName;

    public int InputTokens
    {
        get => inputTokens;
        set
        {
            inputTokens = value;
            OnPropertyChanged(nameof(InputTokens));
            OnPropertyChanged(nameof(InputTokenText));
        }
    }

    public int OutputTokens
    {
        get => outputTokens;
        set
        {
            outputTokens = value;
            OnPropertyChanged(nameof(OutputTokens));
            OnPropertyChanged(nameof(OutputTokenText));
        }
    }

    public string InputTokenText => inputTokens.ToString("N0");

    public string OutputTokenText => outputTokens.ToString("N0");

    public string CostText
    {
        get => costText;
        set
        {
            costText = value;
            OnPropertyChanged(nameof(CostText));
        }
    }

    /// <summary>
    /// True while the model is still producing a response. Drives the typing-dots
    /// animation overlay in the response card; the view-model flips it to false
    /// once the real text arrives (or on an error).
    /// </summary>
    public bool IsGenerating
    {
        get => isGenerating;
        set
        {
            isGenerating = value;
            OnPropertyChanged(nameof(IsGenerating));
        }
    }
}
