namespace Todo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Land on the empty My Notes page (Module-02 spec).
        // The page also sets the ViewModel's SelectedCategory
        // to MyNotes in its OnAppearing override.
    }
}
