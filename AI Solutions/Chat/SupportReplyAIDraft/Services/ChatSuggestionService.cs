using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Services;

public sealed class ChatSuggestionService : IChatSuggestionService
{
    private static readonly ImageSource RegenerateSuggestionImage =
        ImageSource.FromFile("support_reply_regenerate.svg");

    private static readonly ImageSource RetrySuggestionImage =
        ImageSource.FromFile("support_reply_retry.svg");

    private static readonly ImageSource DiscardSuggestionImage =
        ImageSource.FromFile("support_reply_discard.svg");

    public ChatSuggestions Create(params string[] items)
    {
        // Keep icon mapping in one place so the ViewModel stays workflow-focused.
        return new ChatSuggestions
        {
            Items = new ObservableCollection<ISuggestion>(
                items.Select(
                    item => new Suggestion(
                        item,
                        ResolveImage(item))))
        };
    }

    private static ImageSource ResolveImage(string item)
    {
        return item.Trim() switch
        {
            "Discard" => DiscardSuggestionImage,
            "Retry" => RetrySuggestionImage,
            _ => RegenerateSuggestionImage
        };
    }
}
