using CommunityToolkit.Mvvm.ComponentModel;

namespace Todo.Common;

/// <summary>
/// Lightweight base class for all ViewModels in the sample.
/// Inherits from <see cref="ObservableObject"/> so source generators
/// can provide <c>[ObservableProperty]</c> and <c>[RelayCommand]</c>
/// without any further infrastructure.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? title;
}