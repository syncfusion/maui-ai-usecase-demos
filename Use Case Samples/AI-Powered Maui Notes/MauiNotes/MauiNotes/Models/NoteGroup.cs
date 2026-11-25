using System.Collections.ObjectModel;

namespace MauiNotes.Models;

/// <summary>
/// A group of notes (e.g. "Today", "Yesterday"). Deriving from
/// ObservableCollection&lt;NoteModel&gt; makes it directly usable as an
/// ItemsSource for the grouped SfListView.
/// </summary>
public class NoteGroup : ObservableCollection<NoteModel>
{
    public string GroupName { get; }

    public NoteGroup(string groupName, IEnumerable<NoteModel> notes)
        : base(notes)
    {
        GroupName = groupName;
    }
}
