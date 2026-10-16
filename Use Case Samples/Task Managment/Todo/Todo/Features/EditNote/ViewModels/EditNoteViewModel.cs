using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.Services;

namespace Todo.Features.EditNote.ViewModels;

/// <summary>
/// ViewModel for the small centered dialog used to create or edit a
/// note. Matches the dialog chrome shown in
/// <c>createnewlist.png</c> / <c>renamelist.png</c>: title, body text
/// input, <c>Cancel</c> / <c>Save</c> buttons.
///
/// <para>
/// The dialog exposes DueDate / ReminderDate / RepeatType /
/// NotificationsEnabled so the bottom input bar can attach a date,
/// time, recurrence, and notification flag to the note without
/// leaving the form.
/// </para>
/// </summary>
public partial class EditNoteViewModel : ViewModelBase
{
    private readonly INoteService _noteService;
    private readonly INavigationService _navigation;
    private readonly Note? _existing;

    /// <summary>The note currently being edited, or <c>null</c> when
    /// creating a new note.</summary>
    public Note? Target => _existing;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private DateTime? dueDate;

    [ObservableProperty]
    private TimeSpan? dueTime;

    [ObservableProperty]
    private bool hasDueDate;

    [ObservableProperty]
    private DateTime? reminderDate;

    [ObservableProperty]
    private TimeSpan? reminderTime;

    [ObservableProperty]
    private bool hasReminder;

    [ObservableProperty]
    private RepeatType repeatType = RepeatType.None;

    /// <summary>
    /// String projection of <see cref="RepeatType"/> for the
    /// <see cref="Picker"/> in the dialog. Setting the string
    /// round-trips through the underlying enum.
    /// </summary>
    public string RepeatTypeText
    {
        get => RepeatType.ToString();
        set
        {
            if (Enum.TryParse<RepeatType>(value, out var parsed))
                RepeatType = parsed;
        }
    }

    [ObservableProperty]
    private bool notificationsEnabled = true;

    [ObservableProperty]
    private bool isExisting;

    /// <summary>Available repeat options for the picker (as strings
    /// so the MAUI <see cref="Picker"/> can render them).</summary>
    public IReadOnlyList<string> RepeatOptions { get; } =
        new[] { "None", "Daily", "Weekly", "Monthly", "Yearly" };

    public string DialogTitle => IsExisting ? "Edit note" : "Create note";
    public string PrimaryButtonText => IsExisting ? "Save" : "Create";

    public EditNoteViewModel(INoteService noteService, INavigationService navigation)
        : this(noteService, navigation, null)
    {
    }

    public EditNoteViewModel(INoteService noteService, INavigationService navigation, Note? existing)
    {
        _noteService = noteService;
        _navigation = navigation;
        _existing = existing;

        if (existing is not null)
        {
            IsExisting = true;
            Title = existing.Title ?? string.Empty;
            HasDueDate = existing.DueDate.HasValue;
            if (existing.DueDate.HasValue)
            {
                DueDate = existing.DueDate.Value.Date;
                DueTime = existing.DueDate.Value.TimeOfDay;
            }
            HasReminder = existing.ReminderDate.HasValue;
            if (existing.ReminderDate.HasValue)
            {
                ReminderDate = existing.ReminderDate.Value.Date;
                ReminderTime = existing.ReminderDate.Value.TimeOfDay;
            }
            RepeatType = existing.RepeatType;
            NotificationsEnabled = existing.NotificationsEnabled;
        }
    }

    partial void OnTitleChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnHasDueDateChanged(bool value)
    {
        if (value && !DueDate.HasValue) DueDate = DateTime.Today;
        if (value && !DueTime.HasValue) DueTime = TimeSpan.FromHours(9);
        SaveCommand.NotifyCanExecuteChanged();
    }
    partial void OnHasReminderChanged(bool value)
    {
        if (value && !ReminderDate.HasValue) ReminderDate = DateTime.Today;
        if (value && !ReminderTime.HasValue) ReminderTime = TimeSpan.FromHours(9);
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var trimmed = (Title ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        DateTime? due = null;
        if (HasDueDate && DueDate.HasValue)
        {
            var d = DueDate.Value.Date;
            var t = DueTime ?? TimeSpan.Zero;
            due = d + t;
        }
        DateTime? reminder = null;
        if (HasReminder && ReminderDate.HasValue)
        {
            var d = ReminderDate.Value.Date;
            var t = ReminderTime ?? TimeSpan.Zero;
            reminder = d + t;
        }

        if (_existing is null)
        {
            await _noteService.CreateAsync(trimmed, due, reminder);
        }
        else
        {
            _existing.Title = trimmed;
            _existing.DueDate = due;
            _existing.ReminderDate = reminder;
            _existing.RepeatType = RepeatType;
            _existing.NotificationsEnabled = NotificationsEnabled;
            await _noteService.UpdateAsync(_existing);
        }

        await _navigation.PopModalAsync(true);
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.PopModalAsync(false);
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);
}
