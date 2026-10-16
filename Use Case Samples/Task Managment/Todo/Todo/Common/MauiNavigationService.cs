namespace Todo.Common;

/// <summary>
/// Default <see cref="INavigationService"/> implementation that
/// delegates to <see cref="Application.Current"/> for modal navigation
/// and action sheets. Modal pages use a <see cref="TaskCompletionSource{TResult}"/>
/// so callers can <c>await</c> the dismissal result.
/// </summary>
public sealed class MauiNavigationService : INavigationService
{
    public async Task<object?> PushModalAsync(Page page)
    {
        var nav = Application.Current?.MainPage?.Navigation;
        if (nav is null) return null;

        var tcs = new TaskCompletionSource<object?>();
        // Hook the page lifecycle so we can resolve the TCS when the
        // page is popped (either by the page itself via PopModalAsync,
        // or by the system, e.g. a tap outside the dialog).
        page.Disappearing += OnPageDisappearing;
        ModalRequestRegistry.Set(page, tcs);

        try
        {
            await nav.PushModalAsync(page);
        }
        catch
        {
            page.Disappearing -= OnPageDisappearing;
            ModalRequestRegistry.Clear(page);
            throw;
        }

        // Await the result the page provides when it pops itself.
        var result = await tcs.Task;
        page.Disappearing -= OnPageDisappearing;
        ModalRequestRegistry.Clear(page);
        return result;
    }

    public async Task PopModalAsync(object? result = null)
    {
        var nav = Application.Current?.MainPage?.Navigation;
        if (nav is null) return;

        if (nav.ModalStack.Count > 0)
        {
            var top = nav.ModalStack[^1];
            ModalRequestRegistry.Resolve(top, result);
        }

        await nav.PopModalAsync();
    }

    public async Task<string?> ShowActionSheetAsync(string title, string? message, string? destruction, params string[] buttons)
    {
        var page = Application.Current?.MainPage;
        if (page is null) return null;
        return await page.DisplayActionSheet(title, "Cancel", destruction, buttons);
    }

    private static void OnPageDisappearing(object? sender, EventArgs e)
    {
        // System-initiated dismiss (e.g. tapping outside a modal).
        // If the caller hasn't already resolved the result, resolve
        // it with null so the awaiter doesn't hang.
        if (sender is Page page)
            ModalRequestRegistry.ResolveIfPending(page, null);
    }
}

/// <summary>
/// Per-modal-page registry that tracks the <see cref="TaskCompletionSource{TResult}"/>
/// awaiting the dismissal result. Keys are held as strong references
/// to the page, but the page lives in the modal stack anyway and the
/// entry is removed in <c>PushModalAsync</c> as soon as the result
/// arrives, so the registry never holds on to a popped page.
/// </summary>
internal static class ModalRequestRegistry
{
    private static readonly Dictionary<Page, TaskCompletionSource<object?>> _pending = new();

    public static void Set(Page page, TaskCompletionSource<object?> tcs)
    {
        _pending[page] = tcs;
    }

    public static void Clear(Page page)
    {
        _pending.Remove(page);
    }

    public static void Resolve(Page page, object? result)
    {
        ResolveIfPending(page, result);
    }

    public static void ResolveIfPending(Page page, object? result)
    {
        if (_pending.TryGetValue(page, out var tcs))
        {
            _pending.Remove(page);
            tcs.TrySetResult(result);
        }
    }
}
