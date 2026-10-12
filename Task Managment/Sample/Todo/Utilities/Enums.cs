using System.ComponentModel;

namespace Todo.Utilities;

/// <summary>
/// App-wide enumerations.  All enums are <see langword="int"/>-backed so
/// they can be safely persisted to SQLite (sqlite-net-pcl) and shipped over
/// JSON without further configuration.  Every value carries a
/// <see cref="DescriptionAttribute"/> so the value can be rendered in the UI
/// via <c>EnumToStringConverter</c> (Task 1-08) without duplicating display
/// strings in the View layer.
/// </summary>
public static class Enums
{
    /// <summary>
    /// Top-level grouping shown in the navigation sidebar.  Mirrors the
    /// items listed in §2.1 of the specification.
    /// </summary>
    public enum ListCategory
    {
        /// <summary>Default "My notes" list — every new user starts here.</summary>
        [Description("My Notes")]
        MyNotes = 0,

        /// <summary>Tasks flagged with the star / important icon.</summary>
        [Description("Important")]
        Important = 1,

        /// <summary>Tasks that have a reminder attached.</summary>
        [Description("Reminder")]
        Reminder = 2,

        /// <summary>Soft-deleted tasks pending permanent removal.</summary>
        [Description("Bin")]
        Bin = 3,

        /// <summary>User-defined custom list.</summary>
        [Description("Custom")]
        Custom = 4,
    }

    /// <summary>
    /// Supported recurrence patterns for a task (FR-TASK-010).  A task with
    /// <see cref="RecurrenceType.None"/> does not recur.
    /// </summary>
    public enum RecurrenceType
    {
        /// <summary>The task is single-shot — no recurrence.</summary>
        [Description("None")]
        None = 0,

        /// <summary>The task is recreated every day after completion.</summary>
        [Description("Daily")]
        Daily = 1,

        /// <summary>The task is recreated every week after completion.</summary>
        [Description("Weekly")]
        Weekly = 2,

        /// <summary>The task is recreated every month after completion.</summary>
        [Description("Monthly")]
        Monthly = 3,

        /// <summary>User-defined recurrence (custom interval).</summary>
        [Description("Custom")]
        Custom = 4,
    }

    /// <summary>
    /// Lifecycle state of a task.  Used by repositories to filter the
    /// visible/active vs. deleted tasks and to drive UI affordances such as
    /// strikethrough.
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>Task is visible in its list and not yet completed.</summary>
        [Description("Active")]
        Active = 0,

        /// <summary>User has checked the task off — it is finished.</summary>
        [Description("Completed")]
        Completed = 1,

        /// <summary>Task has been moved to the Bin (soft-deleted).</summary>
        [Description("Deleted")]
        Deleted = 2,
    }

    /// <summary>
    /// Standard severity levels for toast / banner notifications shown
    /// across the app.
    /// </summary>
    public enum NotificationSeverity
    {
        /// <summary>Informational message — neutral colour.</summary>
        [Description("Information")]
        Information = 0,

        /// <summary>Successful outcome — green colour.</summary>
        [Description("Success")]
        Success = 1,

        /// <summary>Cautionary message — amber colour.</summary>
        [Description("Warning")]
        Warning = 2,

        /// <summary>Error message — red colour.</summary>
        [Description("Error")]
        Error = 3,
    }

    /// <summary>
    /// Identifier of a navigation / dialog flow so that
    /// <c>AppNavigator</c> (Task 1-06) can resolve the correct
    /// destination without hard-coding route names.
    /// </summary>
    public enum NavigationTarget
    {
        /// <summary>Navigate to the sign-in screen.</summary>
        [Description("Sign In")]
        SignIn = 0,

        /// <summary>Navigate to the sign-up screen.</summary>
        [Description("Sign Up")]
        SignUp = 1,

        /// <summary>Navigate to the main task list.</summary>
        [Description("Task List")]
        TaskList = 2,

        /// <summary>Navigate to the task detail / editor for a given id.</summary>
        [Description("Task Detail")]
        TaskDetail = 3,

        /// <summary>Navigate to the user profile screen.</summary>
        [Description("Profile")]
        Profile = 4,

        /// <summary>Navigate to the search results screen.</summary>
        [Description("Search")]
        Search = 5,
    }
}
