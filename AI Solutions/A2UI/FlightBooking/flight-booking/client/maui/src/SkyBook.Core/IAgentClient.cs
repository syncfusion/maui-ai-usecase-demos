namespace SkyBook.Core;

/// <summary>Anything that can answer a user turn with an A2UI envelope (remote A2A agent or the in-app <see cref="LocalAgent"/>).</summary>
public interface IAgentClient
{
    Task<AgentReply> SendAsync(string text, string? contextId, CancellationToken ct = default);
}
