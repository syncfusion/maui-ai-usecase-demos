using CommunityToolkit.Mvvm.ComponentModel;
using Todo.Utilities;

namespace Todo.Models;

/// <summary>
/// View-model wrapper around a single entry in the main task list sidebar.
/// Used by <c>TaskListPage</c> (Module 3, Task 3-01 / 3-03) to render the
/// list of categories ("My notes", "Important", "Reminder", "Bin") plus any
/// user-defined custom lists.
/// </summary>
/// <remarks>
/// <para>
/// Inherits from <see cref="ObservableObject"/> so the visual-selection
/// highlight can react to changes in <see cref="IsSelected"/> without a
/// dedicated <c>INotifyPropertyChanged</c> implementation in the host page.
/// </para>
/// <para>
/// The class is intentionally <c>sealed</c> and <c>partial</c>-friendly so
/// the CommunityToolkit source generators can emit <c>OnIsSelectedChanged</c>
/// hooks if a derived ViewModel needs to react to selection transitions.
/// </para>
/// </remarks>
public sealed partial class SidebarNavigationItem : ObservableObject
{
    /// <summary>
    /// Stable identifier for the entry.  For built-in categories this is the
    /// <see cref="Enums.ListCategory"/> name ("MyNotes", "Important", …).
    /// For user-defined lists this is the underlying
    /// <see cref="TodoList"/> id.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The category the entry represents.  When the entry is a
    /// user-defined list the value is <see cref="Enums.ListCategory.Custom"/>
    /// and <see cref="CustomListId"/> holds the underlying list id.
    /// </summary>
    public Enums.ListCategory Category { get; }

    /// <summary>
    /// Identifier of the underlying custom list when
    /// <see cref="Category"/> equals <see cref="Enums.ListCategory.Custom"/>.
    /// <see langword="null"/> for the built-in entries.
    /// </summary>
    public string? CustomListId { get; }

    /// <summary>
    /// Title displayed in the sidebar.  For built-in categories this is the
    /// localised name; for user-defined lists it is the list title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Glyph used as a fallback when no <c>ImageSource</c> is provided.
    /// Drawn from the Material / Unicode icon set so the UI is functional
    /// out of the box without shipping raster icons.
    /// </summary>
    public string IconGlyph { get; }

    /// <summary>
    /// Hex tint applied to <see cref="IconGlyph"/>.  Matches the colour
    /// scheme defined in <c>Colors.xaml</c>:
    /// <c>#9C27B0</c> (Important), <c>#2196F3</c> (Reminder),
    /// <c>#F44336</c> (Bin), <c>#757575</c> (default).
    /// </summary>
    public string IconColorHex { get; }

    /// <summary>
    /// Backing store for <see cref="IsSelected"/>.  Updated by the parent
    /// ViewModel when the user taps a sidebar entry or when the active
    /// category changes programmatically (e.g. by a deep-link).
    /// </summary>
    [ObservableProperty]
    private bool isSelected;

    /// <summary>
    /// Backing store for <see cref="TaskCount"/>.  Refreshed whenever the
    /// task service raises its "tasks changed" event.
    /// </summary>
    [ObservableProperty]
    private int taskCount;

    /// <summary>
    /// Convenience flag for the View layer — when <see langword="true"/>
    /// the corresponding sidebar row should render its count badge in
    /// the brand colour rather than the muted gray.  Set to
    /// <see langword="true"/> for the active selection so the badge colour
    /// matches the sidebar highlight.
    /// </summary>
    public bool IsBadgeAccent => IsSelected || TaskCount > 0;

    /// <summary>
    /// Initialises a new instance of the <see cref="SidebarNavigationItem"/>
    /// class.
    /// </summary>
    /// <param name="key">Stable identifier for the entry.</param>
    /// <param name="category">Top-level category the entry represents.</param>
    /// <param name="title">User-visible title.</param>
    /// <param name="iconGlyph">Fallback Unicode / Material glyph.</param>
    /// <param name="iconColorHex">Tint applied to <paramref name="iconGlyph"/>.</param>
    /// <param name="customListId">
    /// Identifier of the underlying custom list.  Required when
    /// <paramref name="category"/> is <see cref="Enums.ListCategory.Custom"/>;
    /// <see langword="null"/> otherwise.
    /// </param>
    public SidebarNavigationItem(
        string key,
        Enums.ListCategory category,
        string title,
        string iconGlyph,
        string iconColorHex,
        string? customListId = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(iconGlyph);
        ArgumentNullException.ThrowIfNull(iconColorHex);

        if (category == Enums.ListCategory.Custom && string.IsNullOrWhiteSpace(customListId))
        {
            throw new ArgumentException(
                "Custom-list entries must supply a CustomListId.",
                nameof(customListId));
        }

        Key = key;
        Category = category;
        Title = title;
        IconGlyph = iconGlyph;
        IconColorHex = iconColorHex;
        CustomListId = customListId;
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit source generator
    /// whenever <see cref="IsSelected"/> changes.  Re-raises a
    /// <c>PropertyChanged</c> for <see cref="IsBadgeAccent"/> so the View
    /// can re-evaluate its badge colour without taking a hard dependency
    /// on the source generator's emit order.
    /// </summary>
    partial void OnIsSelectedChanged(bool value) => OnPropertyChanged(nameof(IsBadgeAccent));

    /// <summary>
    /// Partial method invoked whenever <see cref="TaskCount"/> changes.
    /// Re-raises <see cref="IsBadgeAccent"/> so the badge accent flips
    /// when the very first task is added.
    /// </summary>
    partial void OnTaskCountChanged(int value) => OnPropertyChanged(nameof(IsBadgeAccent));

    /// <summary>
    /// Returns a string representation of the entry for diagnostic logs.
    /// </summary>
    public override string ToString() => $"SidebarItem({Key}, {Title})";
}