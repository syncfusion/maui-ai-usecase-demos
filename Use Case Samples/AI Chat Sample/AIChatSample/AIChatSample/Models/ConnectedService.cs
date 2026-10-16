namespace AIChatSample;

/// <summary>One row in the Connected Services card on the Profile page.</summary>
public sealed class ConnectedService
{
    /// <summary>Display name (e.g. "Google Drive").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Image file name rendered as the row's left icon.</summary>
    public string IconSource { get; init; } = string.Empty;

    /// <summary>True when the service is currently connected.</summary>
    public bool IsConnected { get; init; }

    /// <summary>Text shown in the row's action button (e.g. "Connect").</summary>
    public string ActionText { get; init; } = "Connect";
}
