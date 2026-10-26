using System.ComponentModel;
using System.Windows.Input;

namespace AIChatSample.Controls;

public partial class AIChatInputEditor : ContentView
{
    private const string SendEnabledColorLight = "#7633DA";
    private const string SendEnabledColorDark = "#A572F7";
    private const string SendDisabledColorLight = "#C9C6C8";
    private const string SendDisabledColorDark = "#858385";

    // Text
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(AIChatInputEditor),
            string.Empty,
            BindingMode.TwoWay,
            propertyChanged: OnTextPropertyChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // Placeholder
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(AIChatInputEditor),
            "Type Message...");

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // Send command
    public static readonly BindableProperty SendCommandProperty =
        BindableProperty.Create(
            nameof(SendCommand),
            typeof(ICommand),
            typeof(AIChatInputEditor),
            default(ICommand),
            propertyChanged: OnSendCommandChanged);

    public ICommand SendCommand
    {
        get => (ICommand)GetValue(SendCommandProperty);
        set => SetValue(SendCommandProperty, value);
    }

    private static void OnSendCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AIChatInputEditor editor)
        {
            // When SendCommand is set or changes, refresh the send button state
            editor.UpdateSendVisualState();
        }
    }

    // Attach (+ file) command: invoked when the user taps the plus icon.
    public static readonly BindableProperty AttachCommandProperty =
        BindableProperty.Create(
            nameof(AttachCommand),
            typeof(ICommand),
            typeof(AIChatInputEditor),
            default(ICommand));

    public ICommand AttachCommand
    {
        get => (ICommand)GetValue(AttachCommandProperty);
        set => SetValue(AttachCommandProperty, value);
    }

    // Mic command: invoked when the user taps the microphone icon.
    public static readonly BindableProperty MicCommandProperty =
        BindableProperty.Create(
            nameof(MicCommand),
            typeof(ICommand),
            typeof(AIChatInputEditor),
            default(ICommand));

    public ICommand MicCommand
    {
        get => (ICommand)GetValue(MicCommandProperty);
        set => SetValue(MicCommandProperty, value);
    }

    // AttachCommand binding for the host page to bind to a "remove" action.
    public static readonly BindableProperty RemoveAttachmentCommandProperty =
        BindableProperty.Create(
            nameof(RemoveAttachmentCommand),
            typeof(ICommand),
            typeof(AIChatInputEditor),
            default(ICommand));

    public ICommand RemoveAttachmentCommand
    {
        get => (ICommand)GetValue(RemoveAttachmentCommandProperty);
        set => SetValue(RemoveAttachmentCommandProperty, value);
    }

    // Currently-attached file name (display only). Toggles the chip above the editor.
    public static readonly BindableProperty AttachedFileProperty =
        BindableProperty.Create(
            nameof(AttachedFile),
            typeof(string),
            typeof(AIChatInputEditor),
            default(string?),
            propertyChanged: (b, _, _) => ((AIChatInputEditor)b).OnAttachedFileChanged());

    public string? AttachedFile
    {
        get => (string?)GetValue(AttachedFileProperty);
        set => SetValue(AttachedFileProperty, value);
    }

    public bool HasAttachedFile => !string.IsNullOrEmpty(this.AttachedFile);

    // True while the mic is actively capturing. Drives the mic-icon tint and glyph.
    public static readonly BindableProperty IsListeningProperty =
        BindableProperty.Create(
            nameof(IsListening),
            typeof(bool),
            typeof(AIChatInputEditor),
            false,
            propertyChanged: (b, _, _) => ((AIChatInputEditor)b).OnIsListeningChanged());

    public bool IsListening
    {
        get => (bool)GetValue(IsListeningProperty);
        set => SetValue(IsListeningProperty, value);
    }

    public string MicGlyph => this.IsListening ? "\ue71f" /* stop */ : "\ue7ec" /* mic */;

    public Color MicTint => this.IsListening
        ? Color.FromArgb("#D93025")
        : Color.FromArgb(Application.Current?.RequestedTheme == AppTheme.Dark ? "#C9C6C8" : "#474648");

    private void OnAttachedFileChanged()
    {
        this.OnPropertyChanged(nameof(HasAttachedFile));
    }

    private void OnIsListeningChanged()
    {
        this.OnPropertyChanged(nameof(MicGlyph));
        this.OnPropertyChanged(nameof(MicTint));
    }

    // IsSendEnabled (read-only) - true when text is non-empty AND command can execute
    public static readonly BindableProperty IsSendEnabledProperty =
        BindableProperty.Create(
            nameof(IsSendEnabled),
            typeof(bool),
            typeof(AIChatInputEditor),
            false);

    public bool IsSendEnabled
    {
        get => (bool)GetValue(IsSendEnabledProperty);
        private set => SetValue(IsSendEnabledProperty, value);
    }

    public AIChatInputEditor()
    {
        InitializeComponent();
        UpdateSendVisualState();
    }

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AIChatInputEditor editor)
        {
            editor.UpdateSendVisualState();
        }
    }

    private void UpdateSendVisualState()
    {
        bool hasText = !string.IsNullOrWhiteSpace(this.Text);
        bool canExecute = this.SendCommand?.CanExecute(this.Text) ?? false;
        bool isEnabled = hasText && canExecute;

        if (this.IsSendEnabled != isEnabled)
        {
            this.IsSendEnabled = isEnabled;
        }

        // Visually grey out / activate the icon.
        if (this.SendIconLabel is not null)
        {
            bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            this.SendIconLabel.TextColor = isEnabled
                ? Color.FromArgb(isDark ? SendEnabledColorDark : SendEnabledColorLight)
                : Color.FromArgb(isDark ? SendDisabledColorDark : SendDisabledColorLight);
        }
    }

    private void OnSendTapped(object? sender, TappedEventArgs e)
    {
        if (!this.IsSendEnabled)
        {
            return;
        }

        var command = this.SendCommand;
        if (command is null)
        {
            return;
        }

        if (command.CanExecute(this.Text))
        {
            command.Execute(this.Text);
        }
    }

    private void OnAttachTapped(object? sender, TappedEventArgs e)
    {
        // The + button on the editor is a thin wrapper around whichever
        // command the host page supplies. The default editor doesn't know
        // what attachments mean in any particular screen, so the
        // execution is forwarded to the bound AttachCommand (falling
        // back to no-op when none is provided).
        var command = this.AttachCommand;
        if (command is null) return;
        if (command.CanExecute(this.Text))
        {
            command.Execute(this.Text);
        }
    }

    private void OnMicTapped(object? sender, TappedEventArgs e)
    {
        // The mic icon is just a button — any voice-input flow is owned
        // by the host page. Forward the tap to the bound MicCommand.
        var command = this.MicCommand;
        if (command is null) return;
        if (command.CanExecute(this.Text))
        {
            command.Execute(this.Text);
        }
    }

    private void OnRemoveAttachmentTapped(object? sender, TappedEventArgs e)
    {
        // The host page owns the SelectedFile state; we just forward the
        // tap so the VM can clear it. Fall back to clearing the bindable
        // property when no command is bound.
        var command = this.RemoveAttachmentCommand;
        if (command is not null && command.CanExecute(this.AttachedFile))
        {
            command.Execute(this.AttachedFile);
            return;
        }
        this.AttachedFile = null;
    }

    private void SendIcon_PointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border border && this.IsSendEnabled)
        {
            border.BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#3A393B")
                : Color.FromArgb("#F2F2F2");

            border.Scale = 1.05;
        }
    }

    private void SendIcon_PointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            border.BackgroundColor = Colors.Transparent;
            border.Scale = 1.0;
        }
    }

    private void Icon_PointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            border.BackgroundColor = Application.Current!.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#3A393B")
                : Color.FromArgb("#F2F2F2");

            border.Scale = 1.05;
        }
    }

    private void Icon_PointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            border.BackgroundColor = Colors.Transparent;
            border.Scale = 1.0;
        }
    }

    private void EditorBorder_PointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            border.Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.15f,
                Radius = 12,
                Offset = new Point(0, 2)
            };
        }
    }

    private void InputEditor_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is not VisualElement editor)
            return;

        var border = GetParentBorder(editor);

        if (border != null)
        {
            border.Stroke = Application.Current!.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#5A5A5A")
                : Color.FromArgb("#D1D1D1");

            border.StrokeThickness = 1;
            border.Shadow = null!;
        }
    }

    private void InputEditor_Focused(object sender, FocusEventArgs e)
    {
        if (sender is not VisualElement editor)
            return;

        var border = GetParentBorder(editor);

        if (border != null)
        {
            border.Stroke = Application.Current!.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#8A8A8A")
                : Color.FromArgb("#777777");

            border.StrokeThickness = 1.5;

            border.Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.18f,
                Radius = 12,
                Offset = new Point(0, 2)
            };
        }
    }

    private void EditorBorder_PointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            if (border.StrokeThickness > 1)
                return;

            border.Shadow = null!;
        }
    }

    private Border? GetParentBorder(Element element)
    {
        var parent = element.Parent;

        while (parent != null)
        {
            if (parent is Border border)
                return border;

            parent = parent.Parent;
        }

        return null;
    }

    private void InputEditor_PropertyChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not VisualElement editor)
            return;

        var border = GetParentBorder(editor);

        if (border == null)
            return;

        bool hasText = !string.IsNullOrWhiteSpace(e.NewTextValue);

        if (hasText)
        {
            border.Stroke = Application.Current!.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#8A8A8A")
                : Color.FromArgb("#7A7A7A");

            border.StrokeThickness = 1;
        }
        else
        {
            border.Stroke = Application.Current!.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#5A5A5A")
                : Color.FromArgb("#D1D1D1");

            border.StrokeThickness = 1;
        }

        // Keep the send button state in sync with the latest text.
        this.UpdateSendVisualState();
    }
}
