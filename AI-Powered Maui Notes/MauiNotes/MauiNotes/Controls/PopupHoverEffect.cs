namespace MauiNotes.Controls;

public static class PopupHoverEffect
{
    private static readonly BindableProperty IsPointerInsideProperty = BindableProperty.CreateAttached(
        "IsPointerInside",
        typeof(bool),
        typeof(PopupHoverEffect),
        false);

    private static readonly Color HoverBackgroundColor = Color.FromArgb("#E8E8EB");
    private static readonly Color ActiveBackgroundColor = Color.FromArgb("#bebebe");

    public static readonly BindableProperty IsHoverEnabledProperty = BindableProperty.CreateAttached(
        "IsHoverEnabled",
        typeof(bool),
        typeof(PopupHoverEffect),
        false,
        propertyChanged: OnIsHoverEnabledChanged);

    public static readonly BindableProperty IsActiveProperty = BindableProperty.CreateAttached(
        "IsActive",
        typeof(bool),
        typeof(PopupHoverEffect),
        false,
        propertyChanged: OnIsActiveChanged);

    public static readonly BindableProperty ClearBackgroundOnPointerExitProperty = BindableProperty.CreateAttached(
        "ClearBackgroundOnPointerExit",
        typeof(bool),
        typeof(PopupHoverEffect),
        false);

    public static bool GetIsHoverEnabled(BindableObject bindable) =>
        (bool)bindable.GetValue(IsHoverEnabledProperty);

    public static void SetIsHoverEnabled(BindableObject bindable, bool value) =>
        bindable.SetValue(IsHoverEnabledProperty, value);

    public static bool GetIsActive(BindableObject bindable) =>
        (bool)bindable.GetValue(IsActiveProperty);

    public static void SetIsActive(BindableObject bindable, bool value) =>
        bindable.SetValue(IsActiveProperty, value);

    public static bool GetClearBackgroundOnPointerExit(BindableObject bindable) =>
        (bool)bindable.GetValue(ClearBackgroundOnPointerExitProperty);

    public static void SetClearBackgroundOnPointerExit(BindableObject bindable, bool value) =>
        bindable.SetValue(ClearBackgroundOnPointerExitProperty, value);

    private static void OnIsHoverEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is not true || bindable is not View view)
            return;

        Brush? originalBackground = view.Background;
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) =>
        {
            view.SetValue(IsPointerInsideProperty, true);
            ApplyPointerBackground(view);
        };
        pointer.PointerExited += (_, _) =>
        {
            view.SetValue(IsPointerInsideProperty, false);
            if (GetIsActive(view))
                view.Background = new SolidColorBrush(ActiveBackgroundColor);
            else if (GetClearBackgroundOnPointerExit(view))
                view.ClearValue(VisualElement.BackgroundProperty);
            else
                view.Background = originalBackground;
        };
        view.GestureRecognizers.Add(pointer);
    }

    private static void OnIsActiveChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View view)
            return;

        if ((bool)newValue)
            view.Background = new SolidColorBrush(ActiveBackgroundColor);
        else if ((bool)view.GetValue(IsPointerInsideProperty))
            view.Background = new SolidColorBrush(HoverBackgroundColor);
        else
            view.ClearValue(VisualElement.BackgroundProperty);
    }

    private static void ApplyPointerBackground(View view) =>
        view.Background = new SolidColorBrush(
            GetIsActive(view) ? ActiveBackgroundColor : HoverBackgroundColor);
}