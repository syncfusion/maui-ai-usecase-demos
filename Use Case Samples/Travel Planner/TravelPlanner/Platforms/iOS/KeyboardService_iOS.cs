using Foundation;
using UIKit;

namespace TravelPlanner
{
    /// <summary>
    /// Raises <see cref="KeyboardVisibilityChanged"/> when the iOS/Mac Catalyst keyboard shows or hides.
    /// </summary>
    class KeyboardService_iOS : IKeyboardService
    {
        public event EventHandler<bool>? KeyboardVisibilityChanged;

        public KeyboardService_iOS()
        {
            UIKeyboard.Notifications.ObserveDidShow(HandleDidShow);
            UIKeyboard.Notifications.ObserveDidHide(HandleDidHide);
        }

        private void HandleDidShow(object? sender, UIKeyboardEventArgs e)
            => KeyboardVisibilityChanged?.Invoke(this, true);

        private void HandleDidHide(object? sender, UIKeyboardEventArgs e)
            => KeyboardVisibilityChanged?.Invoke(this, false);

        public void Dispose()
        {
            UIKeyboard.Notifications.ObserveDidShow(HandleDidShow);
            UIKeyboard.Notifications.ObserveDidHide(HandleDidHide);
        }
    }
}