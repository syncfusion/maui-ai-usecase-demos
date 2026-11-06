using System.Threading.Tasks;

namespace HomeInventoryManager.Behaviors
{
    /// <summary>
    /// Static helper that plays the "press" animation exactly as the
    /// user requested. Safe to call from any code-behind or behavior.
    /// </summary>
    public static class TapAnimationHelper
    {
        /// <summary>
        /// Scale the element down to 0.92, back to 1, and flash the
        /// background to <c>#E5E1E3</c> while the scale is active.
        /// </summary>
        public static async Task AnimateTapEffectAsync(VisualElement visualElement)
        {
            if (visualElement is null) return;

            // Skip animation on elements that are not yet ready
            // (e.g. detached or in the middle of being recycled).
            if (visualElement.Window is null && visualElement.Handler is null) return;

            var originalBackgroundColor = visualElement.BackgroundColor;
            visualElement.BackgroundColor = Color.FromArgb("#E5E1E3");

            try
            {
                await visualElement.ScaleToAsync(0.92, 60, Easing.CubicOut);
                await visualElement.ScaleToAsync(1, 80, Easing.CubicIn);
            }
            finally
            {
                await Task.Delay(40);
                visualElement.BackgroundColor = originalBackgroundColor;
            }
        }
    }
}
