using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Models.DTOs;

namespace Todo.ViewModels;

/// <summary>
/// View-model wrapper around a single <see cref="TaskDto"/> that exposes
/// bindable properties tailored to the task-card template used by the main
/// <c>TaskListPage</c> (Module 3 — Tasks 3-05 / 3-06).
/// </summary>
/// <remarks>
/// <para>
/// The wrapper exists so the underlying <see cref="TaskDto"/> (a record
/// kept deliberately immutable for transport between layers) does not
/// have to learn about UI concerns such as command bindings, visual
/// selection or "is the card expanded".  All those concerns live here and
/// are refreshed by the owning <see cref="TaskListPageViewModel"/> when
/// the underlying record is mutated.
/// </para>
/// <para>
/// Instances are short-lived: they are re-created by the
/// <see cref="TaskListPageViewModel"/> whenever the source-of-truth
/// <see cref="TaskDto"/> changes (for example, when the user toggles the
/// "important" flag or completes a task).  Bindings therefore never have
/// to deal with in-place mutation of the underlying record.
/// </para>
/// </remarks>
public sealed partial class TaskItemViewModel : ObservableObject
{
    /// <summary>
    /// Underlying transport record.  Kept as a non-nullable field so the
    /// wrapper can never accidentally wrap a null DTO.
    /// </summary>
    public TaskDto Source { get; }

    /// <summary>
    /// Backing store for <see cref="IsCompleted"/>.  Mirrors
    /// <see cref="TaskDto.IsCompleted"/> so two-way bindings can be wired
    /// to the checkbox on the task card.
    /// </summary>
    [ObservableProperty]
    private bool isCompleted;

    /// <summary>
    /// Backing store for <see cref="IsImportant"/>.  Mirrors
    /// <see cref="TaskDto.IsImportant"/>.
    /// </summary>
    [ObservableProperty]
    private bool isImportant;

    /// <summary>
    /// Convenience accessor used by the View layer to apply the strikethrough
    /// style to the title.  Re-raises from
    /// <see cref="OnIsCompletedChanged(bool)"/> so the bindings stay in sync.
    /// </summary>
    public bool IsTitleStrikethrough => IsCompleted;

    /// <summary>
    /// Local identifier used by the card template for
    /// <c>x:Name</c>-style lookups (for example, when a context menu needs
    /// to anchor to a specific card).  Defaults to the underlying
    /// <see cref="TaskDto.Id"/>.
    /// </summary>
    public string Id => Source.Id;

    /// <summary>
    /// Convenience accessor for the underlying title so the XAML does not
    /// have to traverse the <see cref="Source"/> property for every label.
    /// </summary>
    public string Title => Source.Title;

    /// <summary>
    /// Convenience accessor for the optional description.  Used by the
    /// card template to render a second line of metadata.
    /// </summary>
    public string? Description => Source.Description;

    /// <summary>
    /// Convenience accessor for the due date.  Rendered by the card template
    /// as a calendar glyph + localised date when non-null.
    /// </summary>
    public DateTimeOffset? DueAt => Source.DueAt;

    /// <summary>
    /// Convenience accessor for the reminder time.  Rendered by the card
    /// template as a bell glyph + localised time when non-null.
    /// </summary>
    public DateTimeOffset? ReminderAt => Source.ReminderAt;

    /// <summary>
    /// Indicates whether the task has any temporal metadata (a due date
    /// or a reminder).  Bound by the card template to decide whether to
    /// show the metadata row at all.
    /// </summary>
    public bool HasMetadata => DueAt is not null || ReminderAt is not null;

    /// <summary>
    /// Recurrence glyph used by the card template.  Pulled from the
    /// underlying <see cref="TaskDto.Recurrence"/> so the card can show
    /// a recurring badge without leaking the enum into the View.
    /// </summary>
    public string RecurrenceGlyph => Source.Recurrence switch
    {
        Todo.Utilities.Enums.RecurrenceType.Daily   => "↻",
        Todo.Utilities.Enums.RecurrenceType.Weekly  => "↻",
        Todo.Utilities.Enums.RecurrenceType.Monthly => "↻",
        Todo.Utilities.Enums.RecurrenceType.Custom  => "↻",
        _ => string.Empty,
    };

    /// <summary>
    /// Indicates whether the card should render the recurring glyph.
    /// </summary>
    public bool IsRecurring => Source.Recurrence != Todo.Utilities.Enums.RecurrenceType.None;

    /// <summary>
    /// Invoked by the parent <see cref="TaskListPageViewModel"/> when the
    /// user taps the "star" button on the card.  Wired up by the
    /// View layer via <c>Command="{Binding ToggleImportantCommand}"</c>.
    /// </summary>
    public IRelayCommand? ToggleImportantCommand { get; set; }

    /// <summary>
    /// Invoked by the parent <see cref="TaskListPageViewModel"/> when the
    /// user taps the three-dot overflow menu.  Wired up by the View layer
    /// via <c>Command="{Binding OpenContextMenuCommand}"</c>.
    /// </summary>
    public IRelayCommand? OpenContextMenuCommand { get; set; }

    /// <summary>
    /// Invoked when the user taps the task card itself (outside of the
    /// checkbox / star / menu buttons).  The default implementation simply
    /// forwards to the parent ViewModel via
    /// <see cref="OpenContextMenuCommand"/>.
    /// </summary>
    public IRelayCommand? OpenDetailCommand { get; set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="TaskItemViewModel"/>
    /// class.
    /// </summary>
    /// <param name="source">
    /// The underlying task record.  Must not be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    public TaskItemViewModel(TaskDto source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;
        isCompleted = source.IsCompleted;
        isImportant = source.IsImportant;
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit source generator
    /// whenever <see cref="IsCompleted"/> changes.  Re-raises a
    /// <c>PropertyChanged</c> for <see cref="IsTitleStrikethrough"/> so
    /// the card template refreshes its label style in lock-step with the
    /// checkbox state.
    /// </summary>
    partial void OnIsCompletedChanged(bool value) =>
        OnPropertyChanged(nameof(IsTitleStrikethrough));

    /// <summary>
    /// Re-evaluates the locally-cached fields from the underlying
    /// <see cref="TaskDto"/>.  Called by the parent
    /// <see cref="TaskListPageViewModel"/> after a refresh so the wrapper
    /// re-emits <c>PropertyChanged</c> for every relevant property.
    /// </summary>
    public void RefreshFromSource()
    {
        IsCompleted = Source.IsCompleted;
        IsImportant = Source.IsImportant;
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(DueAt));
        OnPropertyChanged(nameof(ReminderAt));
        OnPropertyChanged(nameof(HasMetadata));
        OnPropertyChanged(nameof(RecurrenceGlyph));
        OnPropertyChanged(nameof(IsRecurring));
    }

    /// <summary>
    /// Returns a string representation of the wrapper for diagnostic logs.
    /// </summary>
    public override string ToString() => $"TaskItem({Id}, '{Title}', done={IsCompleted})";
}