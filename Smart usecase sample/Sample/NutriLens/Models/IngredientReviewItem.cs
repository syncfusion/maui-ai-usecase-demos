using CommunityToolkit.Mvvm.ComponentModel;

namespace NutriLens.Models;

/// <summary>
/// One editable ingredient row on the Review page.
/// </summary>
public partial class IngredientReviewItem : ObservableObject
{
    [ObservableProperty]
    private string indexNumber = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;
}