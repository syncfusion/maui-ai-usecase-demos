using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace PatientCompanion.Controls;

public partial class QuickActionCardView : ContentView
{
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(QuickActionCardView));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public QuickActionCardView()
    {
        InitializeComponent();
    }

    private void QuickActionTapped(object sender, TappedEventArgs e)
    {
        if (Command?.CanExecute(null) == true)
            Command.Execute(null);
    }
}