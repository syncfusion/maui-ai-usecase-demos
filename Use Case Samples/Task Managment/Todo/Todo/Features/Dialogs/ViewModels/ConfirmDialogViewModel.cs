using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;

namespace Todo.Features.Dialogs.ViewModels;

/// <summary>
/// Lightweight yes/no confirmation dialog ViewModel. The page chrome
/// matches <c>deletepermently.png</c>: title, body, <c>Cancel</c> +
/// primary action. Used by the Bin view to confirm a permanent delete.
/// </summary>
public partial class ConfirmDialogViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private string title = "Are you sure?";

    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private string cancelText = "Cancel";

    [ObservableProperty]
    private string confirmText = "Confirm";

    [ObservableProperty]
    private bool isDestructive;

    public ConfirmDialogViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        await _navigation.PopModalAsync(true);
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.PopModalAsync(false);
    }
}
