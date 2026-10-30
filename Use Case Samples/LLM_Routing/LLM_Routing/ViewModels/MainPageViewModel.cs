using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LLM_Routing.Models;
using LLM_Routing.Services;
using Syncfusion.Maui.AIAssistView;

namespace LLM_Routing.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private static readonly Dictionary<string, string> ImageMediaTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".bmp"] = "image/bmp"
    };

    private const int MaxResponseChars = 8500;

    private readonly AzureOpenAIService _aiService;

    public MainPageViewModel(AzureOpenAIService aiService)
    {
        _aiService = aiService;
    }

    [ObservableProperty]
    private string composerText = string.Empty;

    public ObservableCollection<IAssistItem> AssistItems { get; } = new();

    /// <summary>
    /// Files staged in the composer. The SfAIAssistView populates this collection
    /// when the user picks a file via the attachment button, and renders one chip
    /// per entry above the editor. The view-model does not own this list; it is
    /// only the binding target so the chips stay visible until the user sends.
    /// </summary>
    public ObservableCollection<IAttachment> Attachments { get; } = new();

    /// <summary>
    /// Handles a send event forwarded by the view. The view has already marked the
    /// request as Handled and is responsible for clearing the editor afterwards;
    /// this method only adds the request/response to the chat and calls the model.
    /// </summary>
    public Task HandleSendAsync(IAssistItem requestItem)
    {
        if (requestItem is null) return Task.CompletedTask;
        ComposerText = string.Empty;
        return RunChatTurnAsync(requestItem.Text, requestItem);
    }

    private async Task RunChatTurnAsync(string query, IAssistItem? requestItem = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return;

        requestItem ??= new AssistItem { Text = query, IsRequested = true, DateTime = DateTime.Now };

        if (!AssistItems.Contains(requestItem))
            AssistItems.Add(requestItem);

        if (AssistItems.OfType<AIAssistResponseItem>()
                .Any(item => ReferenceEquals(item.RequestItem, requestItem)))
            return;

        var response = new AIAssistResponseItem
        {
            IsRequested = false,
            RequestItem = requestItem,
            Text = "Analyzing query...",
            ModelName = "Selecting model",
            ShowAssistItemFooter = true,
            DateTime = DateTime.Now
        };
        AssistItems.Add(response);

        try
        {
            response.Text = "Validating query with low-end model...";
            var selectedModel = await _aiService.SelectModelAsync(query);

            var hasImage = TryLoadImageAttachment(requestItem, out var imageBytes, out var mediaType);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                response.ModelName = selectedModel;
                response.Text = hasImage
                    ? $"Routed to {selectedModel}. Analyzing attached image..."
                    : $"Routed to {selectedModel}. Generating response...";

                // Show the typing-dots animation while we wait for the model.
                response.IsGenerating = true;
            });

            var result = hasImage
                ? await _aiService.GenerateResponseAsync(selectedModel, query, imageBytes, mediaType)
                : await _aiService.GenerateResponseAsync(selectedModel, query);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                response.InputTokens = result.InputTokens;
                response.OutputTokens = result.OutputTokens;
                response.CostText = CostCalculator.FormatUsd(
                    CostCalculator.EstimateUsd(selectedModel, result.InputTokens, result.OutputTokens));
                response.IsGenerating = false;
                response.Text = result.Text.Length > MaxResponseChars
                    ? result.Text[..MaxResponseChars] + "\n\n(more)…"
                    : result.Text;
            });

        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                response.IsGenerating = false;
                response.Text = $"Unable to generate a response. Check Azure OpenAI configuration and availability. Details: {ex.Message}";
                response.ModelName = string.IsNullOrWhiteSpace(response.ModelName) ? "Router" : response.ModelName;
            });
        }
    }

    /// <summary>
    /// Reads the first image attachment on the request, if any. Returns false when there is no image.
    /// </summary>
    private static bool TryLoadImageAttachment(IAssistItem requestItem, out byte[] imageBytes, out string mediaType)
    {
        imageBytes = Array.Empty<byte>();
        mediaType = string.Empty;

        if (requestItem is not AssistAttachmentItem attachmentItem)
            return false;

        foreach (var attachment in attachmentItem.Attachments)
        {
            if (attachment.FileContent is null)
                continue;

            var mime = ResolveMediaType(attachment);
            if (mime is null)
                continue;

            using var ms = new MemoryStream();
            attachment.FileContent.CopyTo(ms);
            imageBytes = ms.ToArray();
            mediaType = mime;
            return true;
        }

        return false;
    }

    private static string? ResolveMediaType(IAttachment attachment)
    {
        if (ImageMediaTypes.TryGetValue(attachment.FileExtension, out var mime))
            return mime;

        return null;
    }
}
