using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataForm_FormGenerator;

public class EducationFormViewModel : INotifyPropertyChanged
{
    private readonly EducationFormService formService;
    private readonly StudentInformation studentInformation;
    private bool isBusy;
    private string? statusMessage;
    private string? errorMessage;
    private string validationSummary = "SfDataForm Validation: Ready";
    private string? pendingEducationType;
    private bool isPickerVisible;

    public EducationFormViewModel(IAiSchemaService schemaService)
    {
        this.formService = new EducationFormService(schemaService);
        this.studentInformation = new StudentInformation
        {
            FullLegalName = "Shalini Suresh",
            EmailAddress = "shalini.suresh@example.com",
            MobileNumber = "5551234567",
            Nationality = "United States",
            Gender = "Female",
            DateOfBirth = new DateTime(1998, 6, 15),
        };
        this.formService.BusyChanged += OnFormServiceBusyChanged;

        ShowEducationPickerCommand = new Command(
            () => IsPickerVisible = !IsPickerVisible,
            () => CanShowPicker);
        SaveApplicationCommand = new Command(async () => await SaveApplicationAsync(),() => !IsBusy && EducationEntries.Count > 0);

        _ = ProbeCredentialsOnStartupAsync();
    }

    /// <summary>
    /// Runs the credential probe asynchronously on startup. On failure
    /// the message is published to <see cref="ErrorMessage"/> so the
    /// existing error banner in the page picks it up. On success it
    /// silently clears any leftover banner so a user who fixed a bad
    /// key sees a clean page.
    /// </summary>
    private async Task ProbeCredentialsOnStartupAsync()
    {
        try
        {
            await formService.ValidateCredentialsAsync();
            if (ErrorMessage?.StartsWith("Invalid API key", StringComparison.OrdinalIgnoreCase) == true)
            {
                ErrorMessage = null;
            }
        }
        catch (InvalidOperationException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
            {
                ErrorMessage = ex.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Invalid API key details. {ex.Message}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// POCO bound to the top "Student Information" SfDataForm. Always bound
    /// so the form stays in sync with the user's edits.
    /// </summary>
    public StudentInformation StudentInformation => this.studentInformation;

    /// <summary>
    /// One card per education level added during the session. Each card
    /// owns its own picker + SfDataForm and renders independently.
    /// </summary>
    public ObservableCollection<EducationEntryViewModel> EducationEntries { get; } = new();

    public Command ShowEducationPickerCommand { get; }

    public Command SaveApplicationCommand { get; }

    public IReadOnlyList<string> EducationOptions => EducationFormCatalog.Options;

    /// <summary>
    /// Determines whether the user can open the per-row education picker.
    /// Disabled while a generation is in flight or while the picker is
    /// already open so a stray double-tap cannot spawn two cards.
    /// </summary>
    public bool CanShowPicker => !IsBusy && !IsPickerVisible;

    /// <summary>
    /// True while the per-row "pick an education level" picker is shown.
    /// Driven by the "Add Education" button at the bottom of the section —
    /// each tap flips the slot from "button" to "picker", each completed
    /// selection flips it back to "button" so the next entry can be added.
    /// </summary>
    public bool IsPickerVisible
    {
        get => this.isPickerVisible;
        private set
        {
            if (this.isPickerVisible != value)
            {
                this.isPickerVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAddButtonVisible));
                ShowEducationPickerCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>
    /// Mirrors <see cref="IsPickerVisible"/> so the XAML can bind the
    /// visibility of the "Add Education" button (true when the picker is
    /// hidden) without having to use a converter.
    /// </summary>
    public bool IsAddButtonVisible => !this.isPickerVisible;

    public bool IsBusy
    {
        get => this.isBusy;
        private set
        {
            if (this.isBusy != value)
            {
                this.isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
                ShowEducationPickerCommand.ChangeCanExecute();
                SaveApplicationCommand.ChangeCanExecute();
            }
        }
    }

    public bool IsNotBusy => !this.isBusy;

    public string? StatusMessage
    {
        get => this.statusMessage;
        private set
        {
            if (this.statusMessage != value)
            {
                this.statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

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

    public string ValidationSummary
    {
        get => this.validationSummary;
        private set
        {
            if (this.validationSummary != value)
            {
                this.validationSummary = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Currently-selected level inside the per-row picker. When this
    /// changes to a non-empty value the VM immediately generates a card
    /// for that level — no separate "Generate" button is needed, the
    /// pick itself is the trigger. After the generation completes the
    /// picker hides and this property resets to null so the next slot
    /// opens on a clean picker.
    /// </summary>
    public string? PendingEducationType
    {
        get => this.pendingEducationType;
        set
        {
            var normalized = value;
            if (string.Equals(this.pendingEducationType, normalized, StringComparison.Ordinal))
            {
                return;
            }
            this.pendingEducationType = normalized;
            OnPropertyChanged();

            if (!string.IsNullOrWhiteSpace(normalized)
                && EducationFormCatalog.IsKnown(normalized))
            {
                _ = HandlePendingEducationSelectedAsync(normalized);
            }
        }
    }

    private async Task HandlePendingEducationSelectedAsync(string level)
    {
        await AddEducationEntryAsync(level);
        PendingEducationType = null;
    }

    /// <summary>
    /// Returns a fresh POCO for the given education level. Used as the
    /// EducationEntryViewModel's entry factory so each card renders the
    /// correct strongly-typed form template.
    /// </summary>
    public static object CreateEntryFor(string educationType)
    {
        var normalized = educationType.Trim().ToLowerInvariant();
        if (normalized == EducationFormCatalog.HighSchool.ToLowerInvariant())
        {
            return new HighSchoolEntry();
        }
        if (normalized == EducationFormCatalog.Diploma.ToLowerInvariant())
        {
            return new DiplomaEntry();
        }
        if (normalized == EducationFormCatalog.AssociateDegree.ToLowerInvariant())
        {
            return new AssociateEntry();
        }
        if (normalized == EducationFormCatalog.BachelorsDegree.ToLowerInvariant())
        {
            return new BachelorsEntry();
        }
        if (normalized == EducationFormCatalog.MastersDegree.ToLowerInvariant())
        {
            return new MasterEntry();
        }
        return new HighSchoolEntry();
    }

    private EducationEntryViewModel CreateCard(string educationType)
    {
        var idx = EducationEntries.Count + 1;
        var card = new EducationEntryViewModel(idx, educationType, CreateEntryFor);
        card.DeleteRequested += (_, _) => RemoveEducationEntry(card);
        card.PropertyChanged += OnEducationCardPropertyChanged;

        return card;
    }

    /// <summary>
    /// Re-evaluates the global "Add Education" button visibility whenever
    /// a card's readiness or Done state changes. The button stays hidden
    /// while any generated card is still pending its Done confirmation,
    /// and re-appears once every card has been marked done.
    /// </summary>
    private void OnEducationCardPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EducationEntryViewModel.IsDone)
            or nameof(EducationEntryViewModel.IsFormGenerated))
        {
            OnPropertyChanged(nameof(HasOutstandingCard));
            OnPropertyChanged(nameof(IsAddButtonVisible));
        }
    }

    /// <summary>
    /// True when at least one card is generated but not yet marked
    /// done by the user. Controls whether the "Add Education" button is
    /// available — while any card is outstanding we wait for it to be
    /// completed before letting the user start another one.
    /// </summary>
    public bool HasOutstandingCard => EducationEntries.Any(c => c.IsFormGenerated && !c.IsDone);

    /// <summary>
    /// Adds a new card pinned to <paramref name="level"/> and kicks off
    /// AI generation. Called by the per-row picker's SelectedIndexChanged
    /// handler once the user has chosen a level. On any failure we still
    /// keep the empty card around so the user can retry from the next
    /// picker slot.
    /// </summary>
    private async Task AddEducationEntryAsync(string level)
    {
        if (IsBusy)
        {
            return;
        }

        var normalized = (level ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(normalized) || !EducationFormCatalog.IsKnown(normalized))
        {
            ErrorMessage = "Pick an education level before adding fields.";
            return;
        }

        ErrorMessage = null;

        var card = CreateCard(normalized);
        EducationEntries.Add(card);
        SaveApplicationCommand.ChangeCanExecute();

        await GenerateForCardAsync(card);
    }

    private void RemoveEducationEntry(EducationEntryViewModel card)
    {
        if (!EducationEntries.Remove(card))
        {
            return;
        }

        for (var i = 0; i < EducationEntries.Count; i++)
        {
            EducationEntries[i].Renumber(i + 1);
        }

        SaveApplicationCommand.ChangeCanExecute();
        StatusMessage = "Education entry removed.";
    }

    private async Task GenerateForCardAsync(EducationEntryViewModel card)
    {
        if (card is null || IsBusy)
        {
            return;
        }

        var level = card.EducationType;
        if (string.IsNullOrEmpty(level) || !EducationFormCatalog.IsKnown(level))
        {
            card.SetError("Invalid education level.");
            ErrorMessage = "Pick an education level before generating.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            ValidationSummary = "SfDataForm Validation: Generating...";

            var (title, entry) = await formService.GenerateAsync(level);
            if (title is null || entry is null)
            {
                card.SetError("Generation did not return a valid schema.");
                return;
            }

            card.ApplyGenerated(title);
            ValidationSummary = "SfDataForm Validation: Ready";
            StatusMessage = $"{card.Header} ready.";
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Generation was cancelled.";
            ValidationSummary = "SfDataForm Validation: Ready";
        }
        catch (Exception ex)
        {
            var message = $"Unable to generate form: {ex.Message}";
            card.SetError(message);
            ErrorMessage = message;
            ValidationSummary = "SfDataForm Validation: Failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnFormServiceBusyChanged(object? sender, EventArgs e)
        => IsBusy = formService.IsBusy;

    /// <summary>
    /// Pretend to commit the entire application: each completed card
    /// counts as one row in the success message. Real persistence is out
    /// of scope for the UX prototype.
    /// </summary>
    private async Task SaveApplicationAsync()
    {
        var page = Application.Current?.MainPage;
        var completedCards = EducationEntries.Count(c => c.IsFormGenerated && c.Entry is not null);
        if (page is not null)
        {
            await page.DisplayAlert(
                "Application Saved",
                $"Your application with {completedCards} education entr{(completedCards == 1 ? "y" : "ies")} has been saved.",
                "OK");
        }

        ValidationSummary = "SfDataForm Validation: Passed";
        StatusMessage = $"Application saved ({completedCards} entr{(completedCards == 1 ? "y" : "ies")}).";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}