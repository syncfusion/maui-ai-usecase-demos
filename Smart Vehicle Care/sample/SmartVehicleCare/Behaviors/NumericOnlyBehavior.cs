namespace SmartVehicleCare.Behaviors;

/// <summary>Restricts an Entry (typically paired with Keyboard="Numeric") to digit characters only.</summary>
public class NumericOnlyBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry entry)
    {
        base.OnAttachedTo(entry);
        entry.TextChanged += OnTextChanged;
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnTextChanged;
        base.OnDetachingFrom(entry);
    }

    private static void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry || e.NewTextValue == null) return;
        var cleaned = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
        if (cleaned != e.NewTextValue)
            entry.Text = cleaned;
    }
}
