using Todo.Common;

namespace Todo.Common.Views;

public partial class ToastHost : ContentView
{
    private CancellationTokenSource? _hideCts;

    public ToastHost()
    {
        InitializeComponent();
        CloseButton.Clicked += (_, _) => Hide();
    }

    /// <summary>
    /// Convenience: displays a toast raised by an <see cref="IToastService"/>.
    /// </summary>
    public void Show(ToastMessage message) => Show(message.Message, message.Duration);

    public void Show(string message, TimeSpan? duration = null)
    {
        _hideCts?.Cancel();
        _hideCts = new CancellationTokenSource();
        var token = _hideCts.Token;

        MessageLabel.Text = message;
        Pill.IsVisible = true;
        _ = Pill.AnimateAsync(() => Pill.Opacity, 0.0, 1.0, 150, Easing.Linear);
        _ = Pill.AnimateAsync(() => Pill.TranslationY, 12.0, 0.0, 150, Easing.CubicInOut);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(duration ?? TimeSpan.FromSeconds(4), token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            if (Dispatcher.IsDispatchRequired)
                Dispatcher.Dispatch(Hide);
            else
                Hide();
        });
    }

    private void Hide()
    {
        if (!Pill.IsVisible) return;
        _ = Pill.AnimateAsync(() => Pill.Opacity, Pill.Opacity, 0.0, 200, Easing.Linear, () =>
        {
            Pill.IsVisible = false;
            Pill.TranslationY = 0;
        });
    }
}
