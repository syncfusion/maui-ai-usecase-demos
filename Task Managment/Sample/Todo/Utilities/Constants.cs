using SQLite;

namespace Todo.Utilities;

/// <summary>
/// App-wide, compile-time constants.  Use this class instead of magic
/// strings / numbers so the value can be located in one place when it needs
/// to change.  None of these values are environment-specific — runtime
/// configuration belongs in <c>MauiPreferences</c>, the SQLite database, or
/// the remote service.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Application identification constants.  Mirrors the values defined in
    /// the <c>Todo.csproj</c> file so they can be referenced from code
    /// without parsing MSBuild.
    /// </summary>
    public static class App
    {
        /// <summary>Application display name shown to the user.</summary>
        public const string Name = "Todo";

        /// <summary>Bundle / package identifier.</summary>
        public const string Identifier = "com.companyname.todo";

        /// <summary>Current semantic version displayed in the about screen.</summary>
        public const string Version = "1.0";
    }

    /// <summary>
    /// Local SQLite database configuration (used by Task 1-07 —
    /// <c>AppDbContext</c>).
    /// </summary>
    public static class Database
    {
        /// <summary>Default SQLite database file name (stored in AppDataDirectory).</summary>
        public const string FileName = "todo.db3";

        /// <summary>
        /// SQLite <c>SQLITE_OPEN_*</c> flags used when opening the
        /// connection (create + read-write + shared cache).
        /// </summary>
        public const SQLiteOpenFlags OpenFlags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        /// <summary>Default busy timeout (seconds) when waiting on the database lock.</summary>
        public const int BusyTimeoutSeconds = 5;
    }

    /// <summary>
    /// Shell / navigation route names.  Registered in <c>AppShell.xaml</c> and
    /// referenced when navigating programmatically.
    /// </summary>
    public static class Routes
    {
        /// <summary>Startup / splash screen.</summary>
        public const string Splash = "splash";

        /// <summary>Sign-in page.</summary>
        public const string SignIn = "signin";

        /// <summary>Sign-up page.</summary>
        public const string SignUp = "signup";

        /// <summary>Forgot-password page.</summary>
        public const string ForgotPassword = "forgotpassword";

        /// <summary>Main task list page.</summary>
        public const string TaskList = "tasklist";

        /// <summary>Task detail / editor page.</summary>
        public const string TaskDetail = "taskdetail";

        /// <summary>Task editor page (used when creating a new task).</summary>
        public const string EditTask = "edittask";

        /// <summary>Application settings page.</summary>
        public const string Settings = "settings";

        /// <summary>User profile page.</summary>
        public const string Profile = "profile";

        /// <summary>Search results page.</summary>
        public const string Search = "search";
    }

    /// <summary>
    /// Network and I/O timeouts (seconds) used by services.
    /// </summary>
    public static class Timeouts
    {
        /// <summary>Default HTTP request timeout (seconds).</summary>
        public const int HttpRequestSeconds = 30;

        /// <summary>Default database query timeout (seconds).</summary>
        public const int DatabaseQuerySeconds = 10;

        /// <summary>UI animation / activity indicator debounce.</summary>
        public const int UiDebounceMilliseconds = 250;
    }

    /// <summary>
    /// Validation limits applied to user input throughout the app.
    /// </summary>
    public static class Validation
    {
        /// <summary>Maximum characters allowed in a task title.</summary>
        public const int TaskTitleMaxLength = 500;

        /// <summary>Minimum characters required for a password.</summary>
        public const int PasswordMinLength = 8;

        /// <summary>Maximum characters allowed in a list name.</summary>
        public const int ListNameMaxLength = 80;

        /// <summary>Maximum characters allowed in the "about me" field.</summary>
        public const int AboutMaxLength = 1000;
    }

    /// <summary>
    /// Reminder presets, in minutes, shown in the reminder picker (matches
    /// the options in §3.2 FR-TASK-009 of the specification).
    /// </summary>
    public static class ReminderPresets
    {
        /// <summary>15 minutes before the due date.</summary>
        public const int FifteenMinutes = 15;

        /// <summary>1 hour before the due date.</summary>
        public const int OneHour = 60;

        /// <summary>1 day before the due date.</summary>
        public const int OneDay = 60 * 24;

        /// <summary>1 week before the due date.</summary>
        public const int OneWeek = 60 * 24 * 7;
    }

    /// <summary>
    /// Feature flags — toggled in code while the corresponding feature is in
    /// development.  These are <c>const</c> rather than configuration values
    /// so the compiler can dead-strip unused branches in release builds.
    /// </summary>
    public static class FeatureFlags
    {
        /// <summary>Enable the recurring-tasks UI (FR-TASK-010).</summary>
        public const bool RecurringTasks = true;

        /// <summary>Enable background cloud sync.</summary>
        public const bool BackgroundSync = false;

        /// <summary>Enable the "remember me" checkbox on the sign-in page.</summary>
        public const bool RememberMe = true;
    }
}
