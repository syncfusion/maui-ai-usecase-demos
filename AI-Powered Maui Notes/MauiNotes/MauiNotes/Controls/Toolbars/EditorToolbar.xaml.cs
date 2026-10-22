using System.Diagnostics;
using MauiNotes.ViewModels;

namespace MauiNotes.Controls.Toolbars;

public partial class EditorToolbar : ContentView
{
    public event EventHandler? AiRequested;
    public event EventHandler? FormatRequested;
    public event EventHandler? TableRequested;

    public View AiButton => ToolbarAI;

    public EditorToolbar()
    {
        Debug.WriteLine("[EditorToolbar] ctor:start; inflating XAML tree.");
        InitializeComponent();
        Debug.WriteLine("[EditorToolbar] ctor:done; XAML inflated.");
    }

    private void OnFormatClicked(object? sender, TappedEventArgs e)
    {
        FormatRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnAiTapped(object? sender, TappedEventArgs e)
    {
        AiRequested?.Invoke(sender as View ?? ToolbarAI, EventArgs.Empty);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Border { Content: Label { IsEnabled: true } } border)
            VisualStateManager.GoToState(border, "PointerOver");
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Border { Content: Label label } border)
            VisualStateManager.GoToState(border, label.IsEnabled ? "Normal" : "Disabled");
    }

    private void OnRedoClicked(object? sender, TappedEventArgs e) => ExecuteToolbarAction("Redo");

    private void OnUndoClicked(object? sender, TappedEventArgs e) => ExecuteToolbarAction("Undo");

    private void OnTableClicked(object? sender, TappedEventArgs e) => TableRequested?.Invoke(this, EventArgs.Empty);

    private void ExecuteToolbarAction(string action)
    {
        if (BindingContext is NotesViewModel viewModel)
            viewModel.ToolbarActionCommand.Execute(action);
    }
}