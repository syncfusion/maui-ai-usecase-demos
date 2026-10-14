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
/// <see cref="DueDate"/> / <see cref="ReminderDate"/> are exposed here so
/// the date picker added in Module-04 (task 4-4) can bind to them
/// without any further changes to this ViewModel.
/// </para>
/// </summary>
public partial class EditNoteViewModel : ViewModelBase, IModalResultProvider
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
    private DateTime? reminderDate;

    [ObservableProperty]
    private bool isExisting;

    /// <summary>Modal dismissal result consumed by the navigation service.</summary>
    public object? Result { get; set; }

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
            DueDate = existing.DueDate;
            ReminderDate = existing.ReminderDate;
        }
    }

    partial void OnTitleChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var trimmed = (Title ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        if (_existing is null)
        {
            await _noteService.CreateAsync(trimmed, DueDate, ReminderDate);
        }
        else
        {
            _existing.Title = trimmed;
            _existing.DueDate = DueDate;
            _existing.ReminderDate = ReminderDate;
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
