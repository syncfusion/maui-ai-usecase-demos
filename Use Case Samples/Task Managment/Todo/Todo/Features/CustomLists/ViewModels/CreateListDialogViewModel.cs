using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;
using Todo.Features.CustomLists.Services;

namespace Todo.Features.CustomLists.ViewModels;

/// <summary>
/// ViewModel for the centered create-list dialog. Matches the chrome
/// shown in <c>createnewlist.png</c>: title, single text input, and
/// <c>Cancel</c> / <c>Create</c> buttons.
/// </summary>
public partial class CreateListDialogViewModel : ViewModelBase
{
    private readonly IListService _listService;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private string listName = string.Empty;

    /// <summary>
    /// Validation error surfaced inline next to the input. Cleared
    /// when the user starts typing again.
    /// </summary>
    [ObservableProperty]
    private string? errorMessage;

    public string DialogTitle => "Create new list";
    public string PrimaryButtonText => "Create";

    public CreateListDialogViewModel(IListService listService, INavigationService navigation)
    {
        _listService = listService;
        _navigation = navigation;
    }

    partial void OnListNameChanged(string value)
    {
        // Clear the error as soon as the user edits the input.
        if (!string.IsNullOrEmpty(ErrorMessage))
            ErrorMessage = null;
        CreateCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateAsync()
    {
        var trimmed = (ListName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            ErrorMessage = "List name is required.";
            return;
        }

        if (_listService.NameExists(trimmed))
        {
            ErrorMessage = $"A list named '{trimmed}' already exists.";
            return;
        }

        try
        {
            await _listService.CreateAsync(trimmed);
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

    private bool CanCreate() => !string.IsNullOrWhiteSpace(ListName);
}