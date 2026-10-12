using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Todo.Converters;

/// <summary>
/// Converts an enum value to a user-facing display string.  The converter
/// first looks for a <see cref="DescriptionAttribute"/> on the enum member
/// (the recommended pattern for the project's enums) and falls back to
/// <see cref="Enum.GetName(Type, object)"/> when no description is present.
/// </summary>
/// <remarks>
/// <para>
/// <c>ConvertBack</c> parses the supplied string back into the target
/// enum.  Matching is case-insensitive and uses
/// <see cref="Enum.TryParse{TEnum}(string?, out TEnum, bool)"/>.  Both
/// numeric strings (e.g. <c>"1"</c>) and named values
/// (e.g. <c>"Completed"</c> or <c>"completed"</c>) are accepted.
/// </para>
/// <para>
/// A non-enum input, an unparseable string, or <see langword="null"/>
/// returns <see cref="string.Empty"/> on the way out and the default value
/// for the target enum on the way back, so XAML bindings never receive a
/// <see langword="null"/> string.
/// </para>
/// </remarks>
public sealed class EnumToStringConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        Type type = value.GetType();
        if (!type.IsEnum)
        {
            return value.ToString() ?? string.Empty;
        }

        string? name = value.ToString();
        if (name is null)
        {
            return string.Empty;
        }

        FieldInfo? field = type.GetField(name);
        DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? name;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!targetType.IsEnum)
        {
            // Non-enum target — fall back to a default-int so the binding
            // does not crash.  Callers that bind two-way to a non-enum
            // property should not be using this converter in the first
            // place, but we still guard against the runtime exception.
            return 0;
        }

        // First defined enum value (used as a safe default when the input
        // string is empty, null, or cannot be parsed).
        object defaultValue = Enum.GetValues(targetType).GetValue(0)!;

        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return defaultValue;
        }

        if (Enum.TryParse(targetType, text, ignoreCase: true, out object? parsed) &&
            parsed is not null &&
            Enum.IsDefined(targetType, parsed))
        {
            return parsed;
        }

        return defaultValue;
    }
}
