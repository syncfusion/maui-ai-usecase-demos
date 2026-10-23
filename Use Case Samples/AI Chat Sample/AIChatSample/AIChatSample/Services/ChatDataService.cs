using System.Collections.ObjectModel;
using System.Collections.Specialized;


namespace AIChatSample;

/// <summary>
/// In-memory store for chat sessions and the shared <see cref="UserProfile"/>.
/// This is the single source of truth so view-models never mutate each
/// other's private copies.
/// </summary>
public interface IChatDataService
{
    /// <summary>All chat sessions, including closed ones.</summary>
    ReadOnlyObservableCollection<ChatSession> ChatSessions { get; }

    /// <summary>The single shared user profile.</summary>
    UserProfile Profile { get; }

    /// <summary>Raised (on the UI thread) whenever a session is added or removed.</summary>
    event EventHandler<ChatSession>? ChatSessionAdded;

    /// <summary>
    /// Raised (on the UI thread) whenever the chat-sessions collection
    /// is mutated (added, removed, or re-sorted by <c>TouchSession</c>).
    /// Surfaces the underlying <see cref="ReadOnlyObservableCollection{T}.CollectionChanged"/>
    /// event so consumers can react without needing the protected
    /// accessor.
    /// </summary>
    event NotifyCollectionChangedEventHandler? ChatSessionsChanged;

    /// <summary>Creates a new session optionally bound to an agent, prepends to the list, and returns it.</summary>
    ChatSession CreateChatSession(string? title = null, AgentConfiguration? agent = null);

    /// <summary>Touches the session's <c>LastActiveDate</c> so the Recent list re-sorts.</summary>
    void TouchSession(ChatSession session);

    /// <summary>Removes the session from the in-memory store. No-op if the session is null or already gone.</summary>
    void DeleteSession(ChatSession session);
}

public sealed class ChatDataService : IChatDataService
{
    private readonly ObservableCollection<ChatSession> _sessions = new();

    public ChatDataService()
    {
        ChatSessions = new ReadOnlyObservableCollection<ChatSession>(_sessions);
        Profile = new UserProfile();
        // Expose the underlying collection's CollectionChanged so view-
        // models can react to sort and add/remove operations. The
        // ReadOnlyObservableCollection wrapper does not surface this
        // event publicly, so we forward it here.
        _sessions.CollectionChanged += OnSessionsCollectionChanged;
    }

    private void OnSessionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => ChatSessionsChanged?.Invoke(this, e);

    public ReadOnlyObservableCollection<ChatSession> ChatSessions { get; }

    public UserProfile Profile { get; }

    public event EventHandler<ChatSession>? ChatSessionAdded;

    public event NotifyCollectionChangedEventHandler? ChatSessionsChanged;

    public ChatSession CreateChatSession(string? title = null, AgentConfiguration? agent = null)
    {
        var session = new ChatSession(
            id: Guid.NewGuid().ToString("N"),
            title: string.IsNullOrWhiteSpace(title) ? "New chat" : title!,
            agent: agent);
        _sessions.Insert(0, session);
        ChatSessionAdded?.Invoke(this, session);
        return session;
    }

    public void TouchSession(ChatSession session)
    {
        if (session is null) return;
        session.LastActiveDate = DateTime.UtcNow;

        // Re-sort so "Recent" is always most-recent-first.
        var ordered = _sessions.OrderByDescending(s => s.LastActiveDate).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            var currentIndex = _sessions.IndexOf(ordered[i]);
            if (currentIndex != i)
            {
                _sessions.Move(currentIndex, i);
            }
        }
    }

    public void DeleteSession(ChatSession session)
    {
        if (session is null) return;
        _sessions.Remove(session);
        // Intentionally do not raise ChatSessionAdded here — that event
        // signals "a new session was added" and would mislead consumers
        // (e.g. the nav drawer) into thinking a removed session is
        // fresh. Subscribers receive the underlying
        // ChatSessionsChanged -> CollectionChanged notification when
        // the ObservableCollection is mutated.
    }
}
