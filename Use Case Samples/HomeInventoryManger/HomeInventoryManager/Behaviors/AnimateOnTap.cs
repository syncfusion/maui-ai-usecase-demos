using System.Threading.Tasks;

namespace HomeInventoryManager.Behaviors
{
    /// <summary>
    /// XAML-friendly helper that wires a <see cref="TapAnimationBehavior"/>
    /// to any <see cref="VisualElement"/> via a single attached property.
    ///
    ///     &lt;Border behaviors:AnimateOnTap.IsEnabled="True" ... /&gt;
    ///
    /// is equivalent to adding the behavior explicitly.
    /// </summary>
    public static class AnimateOnTap
    {
        public static readonly BindableProperty IsEnabledProperty = BindableProperty.CreateAttached(
            "IsEnabled",
            typeof(bool),
            typeof(AnimateOnTap),
            defaultValue: false,
            propertyChanged: OnIsEnabledChanged);

        public static bool GetIsEnabled(BindableObject view) =>
            (bool)view.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(BindableObject view, bool value) =>
            view.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not VisualElement view) return;

            // Remove any previously-added animation behavior so toggling
            // the property back to false cleanly disconnects it.
            for (int i = view.Behaviors.Count - 1; i >= 0; i--)
            {
                if (view.Behaviors[i] is TapAnimationBehavior)
                    view.Behaviors.RemoveAt(i);
            }

            if (newValue is bool enabled && enabled)
            {
                view.Behaviors.Add(new TapAnimationBehavior());
            }
        }
    }
}
