# Converters

XAML value converters used across the application. Each converter
implements `Microsoft.Maui.Controls.IValueConverter` so it can be wired
into XAML bindings via `Converter={StaticResource <Key>}`.

The four converters shipped with the foundation layer (Task 1-08) are:

| Converter | Default `ConverterParameter` | Purpose |
| --- | --- | --- |
| `BoolToVisibilityConverter` | — (pass `"invert"` to flip) | Maps `bool` → `IsVisible` value. |
| `InvertedBoolConverter` | — | Returns `!value`. Useful for `IsEnabled` opposite a busy flag. |
| `DateTimeToStringConverter` | Short date format (`"d"`) | Renders `DateTime` using a culture-aware format string. |
| `EnumToStringConverter` | — | Returns the `[Description]` of an enum, falling back to the member name. |

All four converters are registered in `App.xaml` so any page can reference
them with `Converter={StaticResource BoolToVisibilityConverter}` (etc.).
They tolerate `null` and unknown source types so XAML bindings never
crash even when the underlying ViewModel property is unset.
