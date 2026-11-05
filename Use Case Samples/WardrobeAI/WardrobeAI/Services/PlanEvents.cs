namespace WardrobeAI.Services;

/// <summary>
/// Lightweight, in-process pub/sub for plan lifecycle events. Lets the
/// <see cref="ViewModels.DashboardViewModel"/> refresh its Upcoming Plans list
/// as soon as a new <see cref="Models.PlannedOutfit"/> is saved, without having
/// to wait for the next <c>OnAppearing</c> cycle.
///
/// Hand-rolled (not CommunityToolkit.Mvvm.Messaging) to keep the dependency
/// surface minimal — the existing codebase does not use that messenger.
/// </summary>
public static class PlanEvents
{
    /// <summary>Raised after a <see cref="Models.PlannedOutfit"/> is successfully saved.</summary>
    public static event EventHandler<PlanSavedEventArgs>? PlanSaved;

    /// <summary>Notify subscribers that a new plan was persisted.</summary>
    public static void RaisePlanSaved(PlanSavedEventArgs args) => PlanSaved?.Invoke(null, args);

    public static void Reset() => PlanSaved = null;
}

/// <summary>Payload describing the plan that was just saved.</summary>
public sealed class PlanSavedEventArgs : EventArgs
{
    public PlanSavedEventArgs(int planId, int userId, DateTime planDate)
    {
        PlanId = planId;
        UserId = userId;
        PlanDate = planDate;
    }

    public int PlanId { get; }
    public int UserId { get; }
    public DateTime PlanDate { get; }
}
