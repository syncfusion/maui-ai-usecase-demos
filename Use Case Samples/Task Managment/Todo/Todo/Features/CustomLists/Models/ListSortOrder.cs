namespace Todo.Features.CustomLists.Models;

/// <summary>
/// Sort orders offered by the per-list "Sort by" menu.
/// </summary>
public enum ListSortOrder
{
    Manual,
    Important,
    Reminder,
    DueDate,
    Alphabetical
}
