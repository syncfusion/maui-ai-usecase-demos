using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataForm_FormGenerator;

/// <summary>
/// UI-only state for one Education card. Holds the level that was
/// selected when the card was added, the POCO bound to SfDataForm,
/// and a delete command. The parent EducationFormViewModel owns the
/// AI service and calls <see cref="ApplyGenerated"/> when a generation
/// succeeds.
/// </summary>
public sealed class EducationEntryViewModel : INotifyPropertyChanged
{
    private readonly Func<string, object> entryFactory;
    private readonly string educationType;
    private readonly int index;
    private object? entry;
    private bool isFormGenerated;
    private bool isExpanded = true;
    private string? errorMessage;
    private string header;
    private bool isSaved;
    private bool isDone;
    private bool hasValidationError;
    private string validationTitle = string.Empty;
    private string validationMessage = string.Empty;

    public EducationEntryViewModel(int index, string educationType, Func<string, object> entryFactory)
    {
        this.index = index;
        this.educationType = educationType;
        this.entryFactory = entryFactory;
        this.header = $"Education {index}";

        DeleteCommand = new Command(() => DeleteRequested?.Invoke(this, EventArgs.Empty));
        ToggleExpandedCommand = new Command(() => IsExpanded = !IsExpanded);
        MarkDoneCommand = new Command(() => IsDone = true);
    }

    public event EventHandler? DeleteRequested;
    public int Index => this.index;
    public string EducationType => this.educationType;

    public string Header
    {
        get => this.header;
        private set
        {
            if (this.header != value)
            {
                this.header = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Re-render the header using the supplied 1-based position. Called
    /// by the parent after a deletion so the remaining cards stay
    /// numbered Education 1, Education 2, … with no gaps.
    /// </summary>
    public void Renumber(int newIndex)
    {
        var colon = this.header.IndexOf(':');
        var suffix = colon >= 0 ? this.header[(colon + 1)..].TrimStart() : this.educationType;
        Header = $"Education {newIndex}: {suffix}";
    }

    public Command DeleteCommand { get; }

    public Command ToggleExpandedCommand { get; }

    /// <summary>
    /// POCO the SfDataForm binds to. Swapped to a fresh template on every
    /// successful generation so existing field values are not silently
    /// inherited from the previous POCO type.
    /// </summary>
    public object? Entry
    {
        get => this.entry;
        private set
        {
            if (!ReferenceEquals(this.entry, value))
            {
                this.entry = value;
                OnPropertyChanged();
                ClearValidation();
            }
        }
    }

    /// <summary>
    /// True when this card already shows a generated SfDataForm.
    /// Collapses entirely while false.
    /// </summary>
    public bool IsFormGenerated
    {
        get => this.isFormGenerated;
        private set
        {
            if (this.isFormGenerated != value)
            {
                this.isFormGenerated = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFormNotGenerated));
                OnPropertyChanged(nameof(IsBodyVisible));
            }
        }
    }

    public bool IsFormNotGenerated => !this.isFormGenerated;

    /// <summary>
    /// True while the card's body (the loading message and the
    /// SfDataForm/SfDataForm-error block) is visible. Flipping this
    /// collapses the card without changing its generation state, so
    /// collapsing and re-expanding does not lose data.
    /// </summary>
    public bool IsExpanded
    {
        get => this.isExpanded;
        private set
        {
            if (this.isExpanded != value)
            {
                this.isExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBodyVisible));
                OnPropertyChanged(nameof(IsChevronExpanded));
            }
        }
    }

    public bool IsBodyVisible => this.isExpanded && this.isFormGenerated;
    public bool IsChevronExpanded => this.isExpanded;

    /// <summary>
    /// True once the user has explicitly marked this card as complete by
    /// tapping its "Done" button. The parent VM uses this to decide
    /// whether to re-show the global "Add Education" button.
    /// </summary>
    public bool IsDone
    {
        get => this.isDone;
        private set
        {
            if (this.isDone != value)
            {
                this.isDone = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDoneButtonVisible));
            }
        }
    }

    /// <summary>
    /// Convenience flag for XAML: shows the per-card Done button only
    /// once the form has actually been generated and before the user
    /// has marked it done.
    /// </summary>
    public bool IsDoneButtonVisible => this.isFormGenerated && !this.isDone;

    public Command MarkDoneCommand { get; }

    public string? ErrorMessage
    {
        get => this.errorMessage;
        private set
        {
            if (this.errorMessage != value)
            {
                this.errorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// True when the user clicked Save but the SfDataForm reported at
    /// least one validation error. Drives the inline red banner shown
    /// above the Save button so the user can see what they need to fix.
    /// </summary>
    public bool HasValidationError
    {
        get => this.hasValidationError;
        private set
        {
            if (this.hasValidationError != value)
            {
                this.hasValidationError = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Short heading shown next to the warning icon (e.g. "Please complete
    /// the required fields below.").
    /// </summary>
    public string ValidationTitle
    {
        get => this.validationTitle;
        private set
        {
            if (this.validationTitle != value)
            {
                this.validationTitle = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Comma-joined list of failing field error messages captured the
    /// last time the user pressed Save with an invalid form.
    /// </summary>
    public string ValidationMessage
    {
        get => this.validationMessage;
        private set
        {
            if (this.validationMessage != value)
            {
                this.validationMessage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Called by the parent VM after a successful AI generation.
    /// Replaces the bound POCO with a fresh template for this card's
    /// education type and updates the header with the schema's title..
    /// </summary>
    public void ApplyGenerated(string schemaTitle)
    {
        Entry = this.entryFactory(this.educationType);
        var suffix = string.IsNullOrWhiteSpace(schemaTitle)
            ? this.educationType
            : schemaTitle;
        Header = $"Education {this.index}: {suffix}";
        IsFormGenerated = true;
        ErrorMessage = null;
    }

    public void SetError(string message)
    {
        ErrorMessage = message;
        IsFormGenerated = false;
    }

    /// <summary>
    /// Show the validation banner with the supplied per-field error
    /// messages. Surfaced to any caller (e.g. the global
    /// "Save Application" command) that runs validation against the
    /// card's POCO and needs to display the per-card result.
    /// </summary>
    public void ReportValidationErrors(IEnumerable<string> fieldErrors)
    {
        var list = fieldErrors?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList()
                   ?? new List<string>();
        if (list.Count == 0)
        {
            ClearValidation();
            return;
        }
        HasValidationError = true;
        ValidationTitle = "Please complete the required fields before saving.";
        ValidationMessage = string.Join("\n", list.Select(e => "• " + e));
    }

    /// <summary>
    /// Hide the validation banner.
    /// </summary>
    public void ClearValidation()
    {
        HasValidationError = false;
        ValidationTitle = string.Empty;
        ValidationMessage = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}