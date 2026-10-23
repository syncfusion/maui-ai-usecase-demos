

namespace AIChatSample;

/// <summary>
/// Creation/validation gateway for agents. Currently simulates a backend
/// round-trip; replace the body with an Azure AI / REST call when ready.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="name"/> is not already used
    /// by an agent in the store (case-insensitive).
    /// </summary>
    Task<bool> ValidateNameNotTakenAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Validates and "persists" the supplied <paramref name="config"/>.
    /// Throws <see cref="InvalidOperationException"/> when the name is
    /// already taken. The in-memory implementation applies a simulated
    /// network delay so the create flow exercises its busy state.
    /// </summary>
    Task<AgentConfiguration> CreateAgentAsync(AgentConfiguration config, CancellationToken ct = default);
}

/// <summary>
/// In-memory <see cref="IAgentService"/> used by the sample. Replace
/// with an Azure / REST-backed implementation when wiring up a real
/// backend.
/// </summary>
public sealed class DefaultAgentService : IAgentService
{
    private readonly IAgentStore _store;

    /// <summary>Builds the service over the supplied <see cref="IAgentStore"/>.</summary>
    public DefaultAgentService(IAgentStore store) => _store = store;

    /// <inheritdoc />
    public Task<bool> ValidateNameNotTakenAsync(string name, CancellationToken ct = default)
        => Task.FromResult(!_store.IsNameTaken(name));

    /// <inheritdoc />
    public async Task<AgentConfiguration> CreateAgentAsync(AgentConfiguration config, CancellationToken ct = default)
    {
        await Task.Delay(600, ct).ConfigureAwait(false); // simulated network latency

        if (_store.IsNameTaken(config.AgentName ?? string.Empty))
        {
            throw new InvalidOperationException($"An agent named '{config.AgentName}' already exists.");
        }

        return config;
    }
}
