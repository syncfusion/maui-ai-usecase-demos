namespace WardrobeAI.ViewModels;

/// <summary>
/// Implemented by every main-tab view model so the
/// <see cref="MainTabHostViewModel"/> can dispatch a "reload now" call when
/// the <c>SfTabView</c> selection changes (replacing the per-tab
/// <c>OnAppearing</c> that the original Shell <c>TabBar</c> triggered for
/// free).
/// </summary>
public interface ITabRefreshable
{
    /// <summary>
    /// Reload any data needed to render the tab. Must be idempotent — the
    /// host calls it both on initial appearance and on every selection
    /// change, so it should coalesce concurrent invocations and avoid
    /// throwing on transient failures (log instead).
    /// </summary>
    Task RefreshAsync();
}
