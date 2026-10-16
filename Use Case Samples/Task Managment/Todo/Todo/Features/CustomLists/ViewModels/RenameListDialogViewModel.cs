using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;
using Todo.Features.CustomLists.Models;
using Todo.Features.CustomLists.Services;

namespace Todo.Features.CustomLists.ViewModels;

/// <summary>
/// ViewModel for the centered rename-list dialog. Matches the chrome
/// shown in <c>renamelist.png</c>: title, single text input
/// pre-populated with the existing name, and <c>Cancel</c> /
/// <c>Save</c> buttons.
/// </summary>
public partial class RenameListDialogViewModel : ViewModelBase, IModalResultProvider
{
    private readonly IListService _listService;
    private readonly INavigationService _navigation;
    private readonly NoteList _target;

    [ObservableProperty]
    private string listName;

    [ObservableProperty]
    private string? errorMessage;

    /// <summary>Modal dismissal result consumed by the navigation service.</summary>
    public object? Result { get; set; }

    public string DialogTitle => "Rename list";
    public string PrimaryButtonText => "Save";

    /// <summary>The list being renamed. Surfaced so the View can show
    /// the current name in the title (per the design image).</summary>
    public NoteList Target => _target;

    public RenameListDialogViewModel(IListService listService, INavigationService navigation, NoteList target)
    {
        _listService = listService;
        _navigation = navigation;
        _target = target;
        // Set the observable property so source-gen OnListNameChanged
        // fires once during construction (clears any default value).
        ListName = target?.Name ?? string.Empty;
    }

    partial void OnListNameChanged(string value)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
            ErrorMessage = null;
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var trimmed = (ListName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            ErrorMessage = "List name is required.";
            return;
        }

        // Allow saving the same name (no-op rename).
        if (string.Equals(trimmed, _target.Name, StringComparison.Ordinal))
        {
            await _navigation.PopModalAsync(true);
            return;
        }

        if (_listService.NameExists(trimmed, excludeId: _target.Id))
        {
            ErrorMessage = $"A list named '{trimmed}' already exists.";
            return;
        }

        try
        {
            await _listService.RenameAsync(_target.Id, trimmed);
            await _navigation.PopModalAsync(true);
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.PopModalAsync(false);
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(ListName);
}