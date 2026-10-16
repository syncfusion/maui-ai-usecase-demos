namespace Todo.Common;

/// <summary>
/// Abstraction over the in-app toast pill shown above the bottom input
/// bar (per the reference design: a dark pill that auto-dismisses).
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Shows a transient message. The <see cref="ToastHost"/> at the
    /// root of the layout subscribes and renders it as a pill.
    /// </summary>
    void Show(string message, TimeSpan? duration = null);
}

/// <summary>
/// Default in-app toast implementation. It simply publishes the message
/// and lets the active <see cref="ToastHost"/> display it; no platform
/// code is required.
/// </summary>
public sealed class InAppToastService : IToastService
{
    public event EventHandler<ToastMessage>? ToastShown;

    public void Show(string message, TimeSpan? duration = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        var span = duration ?? TimeSpan.FromSeconds(4);
        ToastShown?.Invoke(this, new ToastMessage(message, span));
    }
}

/// <summary>
/// Payload for a single toast.
/// </summary>
public sealed record ToastMessage(string Message, TimeSpan Duration);
