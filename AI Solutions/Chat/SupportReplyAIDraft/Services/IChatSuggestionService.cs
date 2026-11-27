using Syncfusion.Maui.Chat;

namespace SupportReplyAIDraft.Services;

public interface IChatSuggestionService
{
    ChatSuggestions Create(params string[] items);
}
