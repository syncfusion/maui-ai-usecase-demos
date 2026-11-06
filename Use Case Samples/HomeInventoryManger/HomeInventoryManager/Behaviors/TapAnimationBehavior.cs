using System.Threading.Tasks;

namespace HomeInventoryManager.Behaviors
{
    /// <summary>
    /// Reusable attached behavior that plays a "press" animation
    /// (scale-down + scale-up + brief background flash) whenever the
    /// host <see cref="View"/> is tapped. Apply it with:
    ///
    ///     &lt;Border ...&gt;
    ///         &lt;Border.Behaviors&gt;
    ///             &lt;behaviors:TapAnimationBehavior /&gt;
    ///         &lt;/Border.Behaviors&gt;
    ///     &lt;/Border&gt;
    ///
    /// The behavior works for any <see cref="View"/>: Border, Button,
    /// Label, Image, etc. Only <see cref="View"/> is supported here
    /// (rather than <see cref="VisualElement"/>) because
    /// <see cref="View.GestureRecognizers"/> is the only place MAUI
    /// exposes a generic gesture-collection surface.
    ///
    /// On desktop / Windows the behavior also hooks the pointer so
    /// hovering over the element scales it up slightly to mirror the
    /// press feedback.
    /// </summary>
    public class TapAnimationBehavior : Behavior<View>
    {
        private View? _host;
        private TapGestureRecognizer? _tapRecognizer;
        private PointerGestureRecognizer? _pointerRecognizer;

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);
            _host = bindable;

            // Tapped → run the requested scale / background flash.
            _tapRecognizer = new TapGestureRecognizer();
            _tapRecognizer.Tapped += OnTapped;
            bindable.GestureRecognizers.Add(_tapRecognizer);

            // Hover / press feedback (only meaningful on platforms with
            // a real pointer, e.g. Windows / Mac Catalyst). The events
            // are no-ops on touch-only platforms.
            _pointerRecognizer = new PointerGestureRecognizer();
            _pointerRecognizer.PointerEntered += OnPointerEntered;
            _pointerRecognizer.PointerExited += OnPointerExited;
            _pointerRecognizer.PointerPressed += OnPointerPressed;
            _pointerRecognizer.PointerReleased += OnPointerReleased;
            bindable.GestureRecognizers.Add(_pointerRecognizer);
        }

        protected override void OnDetachingFrom(View bindable)
        {
            if (_tapRecognizer is not null)
            {
                _tapRecognizer.Tapped -= OnTapped;
                bindable.GestureRecognizers.Remove(_tapRecognizer);
                _tapRecognizer = null;
            }
            if (_pointerRecognizer is not null)
            {
                _pointerRecognizer.PointerEntered -= OnPointerEntered;
                _pointerRecognizer.PointerExited -= OnPointerExited;
                _pointerRecognizer.PointerPressed -= OnPointerPressed;
                _pointerRecognizer.PointerReleased -= OnPointerReleased;
                bindable.GestureRecognizers.Remove(_pointerRecognizer);
                _pointerRecognizer = null;
            }
            _host = null;
            base.OnDetachingFrom(bindable);
        }

        private async void OnTapped(object? sender, TappedEventArgs e)
        {
            if (_host is null) return;
            await TapAnimationHelper.AnimateTapEffectAsync(_host);
        }

        private async void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            if (_host is null) return;
            try
            {
                await _host.ScaleToAsync(1.02, 90, Easing.CubicOut);
            }
            catch
            {
                // Animations can race with element disposal; ignore.
            }
        }

        private async void OnPointerExited(object? sender, PointerEventArgs e)
        {
            if (_host is null) return;
            try
            {
                await _host.ScaleToAsync(1.0, 120, Easing.CubicIn);
            }
            catch
            {
                // Animations can race with element disposal; ignore.
            }
        }

        private async void OnPointerPressed(object? sender, PointerEventArgs e)
        {
            if (_host is null) return;
            try
            {
                await _host.ScaleToAsync(0.98, 50, Easing.CubicOut);
            }
            catch
            {
                // Animations can race with element disposal; ignore.
            }
        }

        private async void OnPointerReleased(object? sender, PointerEventArgs e)
        {
            if (_host is null) return;
            try
            {
                await _host.ScaleToAsync(1.0, 70, Easing.CubicIn);
            }
            catch
            {
                // Animations can race with element disposal; ignore.
            }
        }
    }
}
