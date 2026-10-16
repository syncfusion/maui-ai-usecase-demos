using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;
using Todo.Features.CustomLists.Models;
using Todo.Features.CustomLists.Services;
using Todo.Features.CustomLists.Views;
using Todo.Features.Dialogs.ViewModels;
using Todo.Features.Dialogs.Views;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.CustomLists.ViewModels;

/// <summary>
/// Hosts the commands used by the sidebar to manage user-defined
/// lists: create, rename, delete. The list contents themselves are
/// exposed directly from the <see cref="IListService"/> and consumed
/// by the sidebar view; this ViewModel only adds the command surface
/// (open the create dialog, open the rename dialog, delete with
/// confirmation) and forwards them to the shared services.
/// </summary>
public partial class CustomListsViewModel : ViewModelBase
{
    private readonly IListService _listService;
    private readonly INavigationService _navigation;
    private readonly NotesListViewModel _notesListViewModel;

    public CustomListsViewModel(
        IListService listService,
        INavigationService navigation,
        NotesListViewModel notesListViewModel)
    {
        _listService = listService;
        _navigation = navigation;
        _notesListViewModel = notesListViewModel;
    }

    // ---------------------------------------------------------------
    //  Create-list flow (matches createnewlist.png)
    // ---------------------------------------------------------------

    /// <summary>
    /// Opens the centered create-list dialog (matches
    /// <c>createnewlist.png</c>) and adds the new list on confirm.
    /// </summary>
    [RelayCommand]
    private async Task OpenCreateListDialogAsync()
    {
        var vm = new CreateListDialogViewModel(_listService, _navigation);
        var page = new CreateListDialogPage(vm);
        await _navigation.PushModalAsync(page);
    }

    // ---------------------------------------------------------------
    //  Rename-list flow (matches renamelist.png)
    // ---------------------------------------------------------------

    /// <summary>
    /// Opens the centered rename-list dialog (matches
    /// <c>renamelist.png</c>) pre-populated with the current name and
    /// applies the new name on confirm.
    /// </summary>
    [RelayCommand]
    private async Task OpenRenameListDialogAsync(NoteList? list)
    {
        if (list is null) return;

        var vm = new RenameListDialogViewModel(_listService, _navigation, list);
        var page = new RenameListDialogPage(vm);
        await _navigation.PushModalAsync(page);
    }

    // ---------------------------------------------------------------
    //  Delete-list flow (per spec §7.5)
    // ---------------------------------------------------------------

    /// <summary>
    /// Shows a confirmation dialog (matching the destructive
    /// <c>deletepermently.png</c> chrome) and removes the list on
    /// confirm. Notes in the deleted list are returned to My Notes.
    /// </summary>
    [RelayCommand]
    private async Task ConfirmDeleteListAsync(NoteList? list)
    {
        if (list is null) return;

        var vm = new ConfirmDialogViewModel(_navigation)
        {
            Title = "Delete list?",
            Message = $"The '{list.Name}' list will be removed. Notes in this list will move back to My Notes.",
            ConfirmText = "Delete",
            IsDestructive = true
        };
        var page = new ConfirmDialogPage(vm);
        var result = await _navigation.PushModalAsync(page);
        if (result is true)
        {
            await _listService.DeleteAsync(list.Id);

            // If the user was currently viewing this list, switch them
            // back to My Notes so the empty sidebar state is consistent.
            _notesListViewModel.ClearSelectedListIfDeleted(list.Id);
        }
    }
}