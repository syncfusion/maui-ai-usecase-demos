
using System.Text;

namespace AIChatSample;

/// <summary>
/// Deterministic offline implementation of <see cref="IAIService"/> used
/// when no Azure OpenAI configuration is supplied. Lets the sample run
/// with zero configuration (NFR-2) and supports the harness's per-task
/// manual verification (T02).
/// </summary>
public sealed class MockAIService : IAIService
{
    private static readonly string[] FallbackSuggestions =
    {
        "Tell me more",
        "Give an example",
        "Why?",
        "Summarize this",
    };

    private readonly TimeSpan _simulatedLatency;

    /// <param name="simulatedLatencyMs">
    /// Latency in milliseconds to simulate before responding. Defaults to
    /// 400ms so the UI can demonstrate the busy state without feeling
    /// sluggish.
    /// </param>
    public MockAIService(int simulatedLatencyMs = 400)
    {
        _simulatedLatency = TimeSpan.FromMilliseconds(Math.Max(0, simulatedLatencyMs));
    }

    public async Task<ContextSuggestionResponse?> GetResponseAsync(
        string prompt,
        string? systemContext = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return new ContextSuggestionResponse { Answer = string.Empty };
        }

        // Stay responsive on the UI thread by awaiting the simulated
        // network round-trip on a background scheduler (NFR-8).
        if (_simulatedLatency > TimeSpan.Zero)
        {
            await Task.Delay(_simulatedLatency, ct).ConfigureAwait(false);
        }

        if (ct.IsCancellationRequested)
        {
            return null;
        }

        return new ContextSuggestionResponse
        {
            Answer = BuildAnswer(prompt, systemContext),
            Suggestions = new List<string>(FallbackSuggestions),
        };
    }

    public async Task<AgentDraft?> DraftAgentAsync(string prompt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return null;
        if (_simulatedLatency > TimeSpan.Zero)
        {
            await Task.Delay(_simulatedLatency, ct).ConfigureAwait(false);
        }
        if (ct.IsCancellationRequested) return null;
        return BuildDraft(prompt);
    }

    private static AgentDraft BuildDraft(string prompt)
    {
        // Derive a short, capitalized name from the first significant
        // word of the prompt so the mock produces something meaningful
        // when Azure OpenAI isn't configured.
        var words = prompt
            .Replace('\n', ' ').Replace('\r', ' ')
            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        string titleWord = "Custom";
        foreach (var w in words)
        {
            if (w.Length < 3) continue;
            titleWord = w;
            break;
        }
        var name = titleWord.Length > 1
            ? char.ToUpperInvariant(titleWord[0]) + titleWord.Substring(1) + " Specialist"
            : "Custom Specialist";

        var description = prompt.Trim();
        if (description.Length > 140) description = description.Substring(0, 140) + "…";

        var instructions =
            $"You are a {name.ToLowerInvariant()} that helps with the following: {prompt.Trim()}.\n" +
            "Respond concisely, ask clarifying questions when needed, and structure longer answers with headings or bullets.";

        return new AgentDraft
        {
            Name = name,
            Description = description,
            Instructions = instructions,
        };
    }

    private static string BuildAnswer(string prompt, string? systemContext)
    {
        var sb = new StringBuilder();
        sb.Append("(Mock) ");

        if (!string.IsNullOrWhiteSpace(systemContext))
        {
            sb.Append("Responding using agent behaviour. ");
        }

        sb.Append("You said: ");
        sb.Append(prompt.Trim());
        sb.Append('.');
        return sb.ToString();
    }
}
