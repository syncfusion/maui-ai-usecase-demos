namespace Todo.Common;

/// <summary>
/// Built-in lists that appear in the sidebar and represent the
/// four filter views of the To-Do List sample.
/// </summary>
public enum NotesListCategory
{
    MyNotes,
    Important,
    Reminder,
    Bin
}

public static class NotesListCategoryExtensions
{
    public static string ToDisplayName(this NotesListCategory category) => category switch
    {
        NotesListCategory.MyNotes => "My Notes",
        NotesListCategory.Important => "Important",
        NotesListCategory.Reminder => "Reminder",
        NotesListCategory.Bin => "Bin",
        _ => category.ToString()
    };

    public static string ToAccentColorKey(this NotesListCategory category) => category switch
    {
        NotesListCategory.MyNotes => "MyNotesColor",
        NotesListCategory.Important => "ImportantColor",
        NotesListCategory.Reminder => "ReminderColor",
        NotesListCategory.Bin => "BinColor",
        _ => "MyNotesColor"
    };

    public static string ToBackgroundImage(this NotesListCategory category) => category switch
    {
        NotesListCategory.MyNotes => "mynotes_bg.png",
        NotesListCategory.Important => "important_bg.png",
        NotesListCategory.Reminder => "remainder_bg.png",
        NotesListCategory.Bin => "bin_bg.png",
        _ => "mynotes_bg.png"
    };
}