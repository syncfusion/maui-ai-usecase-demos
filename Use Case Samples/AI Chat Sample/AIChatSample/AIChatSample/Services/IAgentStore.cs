namespace AIChatSample;

/// <summary>
/// Persist/track created agents across the app (single source of truth).
/// Swap the in-memory implementation for a backend/Azure-backed one later.
/// </summary>
public interface IAgentStore
{
    /// <summary>All agents currently known to the store.</summary>
    IReadOnlyList<AgentConfiguration> Agents { get; }

    /// <summary>Raised on the UI thread when a new agent is added.</summary>
    event EventHandler<AgentConfiguration>? AgentAdded;

    /// <summary>Adds <paramref name="config"/> to the store and raises <see cref="AgentAdded"/>.</summary>
    void Add(AgentConfiguration config);

    /// <summary>Returns <c>true</c> when an agent named <paramref name="name"/> already exists (case-insensitive).</summary>
    bool IsNameTaken(string name);
}

/// <summary>
/// In-memory <see cref="IAgentStore"/> used by the sample. Replace with
/// a backend/Azure-backed implementation when wiring up a real
/// backend.
/// </summary>
public sealed class InMemoryAgentStore : IAgentStore
{
    private readonly List<AgentConfiguration> _agents = new();

    /// <inheritdoc />
    public IReadOnlyList<AgentConfiguration> Agents => _agents;

    /// <inheritdoc />
    public event EventHandler<AgentConfiguration>? AgentAdded;

    /// <inheritdoc />
    public void Add(AgentConfiguration config)
    {
        if (config is null || string.IsNullOrWhiteSpace(config.AgentName))
        {
            throw new ArgumentException("Agent name is required.", nameof(config));
        }

        if (IsNameTaken(config.AgentName))
        {
            throw new InvalidOperationException($"An agent named '{config.AgentName}' already exists.");
        }

        _agents.Add(config);
        AgentAdded?.Invoke(this, config);
    }

    /// <inheritdoc />
    public bool IsNameTaken(string name) => _agents.Any(a => string.Equals(a.AgentName, name?.Trim(), StringComparison.OrdinalIgnoreCase));
}
