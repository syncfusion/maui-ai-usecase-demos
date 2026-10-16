using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Syncfusion.Maui.AIAssistView;

namespace AIChatSample;

/// <summary>
/// Single conversation between the user and the AI. Holds the message
/// log plus the agent binding so the chat can be re-opened with the same
/// system context (FR-0.4).
/// </summary>
public sealed class ChatSession : INotifyPropertyChanged
{
    private string title = string.Empty;
    private DateTime lastActiveDate = DateTime.UtcNow;

    /// <summary>Default constructor used by serializers and tests.</summary>
    public ChatSession()
    {
    }

    /// <summary>Builds a session with the given id, title, and optional agent binding.</summary>
    public ChatSession(string id, string title, AgentConfiguration? agent = null)
    {
        Id = id;
        Title = title;
        Agent = agent;
    }

    /// <summary>Stable identifier for the session.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>Display title (e.g. "New chat", or the first prompt).</summary>
    public string Title
    {
        get => title;
        set
        {
            if (title == value) return;
            title = value;
            OnPropertyChanged();
        }
    }

    /// <summary>UTC timestamp the session was first created.</summary>
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the most recent activity. Updated by <c>ChatDataService.TouchSession</c> so the Recent list re-sorts (FR-0.4).</summary>
    public DateTime LastActiveDate
    {
        get => lastActiveDate;
        set
        {
            if (lastActiveDate == value) return;
            lastActiveDate = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Agent the session is bound to, or <c>null</c> for the default Copilot.</summary>
    public AgentConfiguration? Agent { get; set; }

    /// <summary>Ordered list of messages exchanged during this session.</summary>
    public ObservableCollection<IAssistItem> Messages { get; } = new();

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
