namespace Todo.Features.Notes.Views.Templates;

/// <summary>
/// Reusable card template for a single <see cref="Models.Note"/>.
/// All bindings and commands live in the XAML; this code-behind is
/// intentionally empty so the view stays MVVM-clean.
/// </summary>
public partial class NoteItemTemplate : DataTemplate
{
    public NoteItemTemplate()
    {
        InitializeComponent();
    }
}
