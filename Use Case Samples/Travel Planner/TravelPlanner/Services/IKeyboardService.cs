using System;

namespace TravelPlanner
{
    /// <summary>
    /// Reports soft-keyboard visibility changes so views can react (e.g. expand the bottom sheet).
    /// Implemented per-platform; see Platforms/Android and Platforms/iOS.
    /// </summary>
    public interface IKeyboardService : IDisposable
    {
        event EventHandler<bool>? KeyboardVisibilityChanged;
    }
}