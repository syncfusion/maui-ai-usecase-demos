using System.ComponentModel.DataAnnotations;
using Syncfusion.Maui.DataForm;

namespace DataForm_FormGenerator;

public partial class MainPage : ContentPage
{
    internal static readonly FormItemSourceProvider SharedItemSourceProvider = new();

    public MainPage(EducationFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        StudentDataForm.ItemsSourceProvider = SharedItemSourceProvider;
        StudentDataForm.ValidationMode = DataFormValidationMode.PropertyChanged;
        StudentDataForm.RegisterEditor(nameof(StudentInformation.Gender), DataFormEditorType.ComboBox);
        StudentDataForm.RegisterEditor(nameof(StudentInformation.Nationality), DataFormEditorType.ComboBox);
    }
}

/// <summary>
/// OnAttach hook that assigns the page-wide <see cref="FormItemSourceProvider"/>
/// to any SfDataForm that has the behavior attached. This works for
/// forms living inside CollectionView DataTemplates because the
/// behavior is materialized alongside its owning SfDataForm.
/// </summary>
public sealed class DataFormProviderBehavior : Behavior<SfDataForm>
{
    protected override void OnAttachedTo(SfDataForm bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.ItemsSourceProvider ??= MainPage.SharedItemSourceProvider;
    }
}

/// <summary>
/// Provides static option lists to every SfDataForm on the page for
/// fields that should render as a ComboBox (State, AccreditationStatus,
/// Country, Nationality, Gender, Country-Code). The provider is queried
/// per-property-name by SfDataForm, so the same instance works for both
/// the Student form and the per-Education-card forms.
/// </summary>
internal sealed class FormItemSourceProvider : IDataFormSourceProvider
{
    private static readonly string[] UsStates =
    {
        "California (CA)", "Texas (TX)", "New York (NY)", "Washington (WA)"
    };

    private static readonly string[] AccreditationOptions =
    {
        "Accredited", "Pending", "Not Accredited"
    };

    private static readonly string[] CountryOptions =
    {
        "United States", "Canada", "United Kingdom", "Australia", "India"
    };

    public object? GetSource(string sourceName)
    {
        if (sourceName == nameof(HighSchoolEntry.State))
        {
            return UsStates;
        }
        if (sourceName == nameof(DiplomaEntry.AccreditationStatus))
        {
            return AccreditationOptions;
        }
        if (sourceName == nameof(BachelorsEntry.Country))
        {
            return CountryOptions;
        }
        if (sourceName == nameof(StudentInformation.Nationality))
        {
            return StudentInformation.DefaultNationalities;
        }
        if (sourceName == nameof(StudentInformation.Gender))
        {
            return StudentInformation.DefaultGenders;
        }

        return null;
    }
}