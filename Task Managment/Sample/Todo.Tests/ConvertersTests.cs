using System.Globalization;
using Todo.Converters;
using Todo.Utilities;
using Xunit;

namespace Todo.Tests;

/// <summary>
/// Unit tests for the four XAML value converters introduced in
/// Task 1-08 (Create Utility Classes).  Each converter is exercised
/// against its positive case, its negative case and a null / unknown
/// source so that XAML bindings are guaranteed to never crash.
/// </summary>
public class ConvertersTests
{
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [Fact]
    public void BoolToVisibilityConverter_true_maps_to_true()
    {
        var converter = new BoolToVisibilityConverter();
        Assert.True((bool)converter.Convert(true, typeof(bool), null, _culture));
    }

    [Fact]
    public void BoolToVisibilityConverter_false_maps_to_false()
    {
        var converter = new BoolToVisibilityConverter();
        Assert.False((bool)converter.Convert(false, typeof(bool), null, _culture));
    }

    [Fact]
    public void BoolToVisibilityConverter_invert_flips_the_result()
    {
        var converter = new BoolToVisibilityConverter();
        Assert.False((bool)converter.Convert(true, typeof(bool), BoolToVisibilityConverter.InvertParameter, _culture));
        Assert.True((bool)converter.Convert(false, typeof(bool), BoolToVisibilityConverter.InvertParameter, _culture));
    }

    [Fact]
    public void BoolToVisibilityConverter_non_bool_defaults_to_false()
    {
        var converter = new BoolToVisibilityConverter();
        Assert.False((bool)converter.Convert("not a bool", typeof(bool), null, _culture));
        Assert.False((bool)converter.Convert(null, typeof(bool), null, _culture));
    }

    [Fact]
    public void BoolToVisibilityConverter_ConvertBack_returns_input_bool()
    {
        var converter = new BoolToVisibilityConverter();
        Assert.True((bool)converter.ConvertBack(true, typeof(bool), null, _culture));
        Assert.False((bool)converter.ConvertBack(false, typeof(bool), null, _culture));
    }

    [Fact]
    public void InvertedBoolConverter_inverts_input()
    {
        var converter = new InvertedBoolConverter();
        Assert.False((bool)converter.Convert(true, typeof(bool), null, _culture));
        Assert.True((bool)converter.Convert(false, typeof(bool), null, _culture));
    }

    [Fact]
    public void InvertedBoolConverter_non_bool_defaults_to_true()
    {
        var converter = new InvertedBoolConverter();
        Assert.True((bool)converter.Convert("anything", typeof(bool), null, _culture));
        Assert.True((bool)converter.Convert(null, typeof(bool), null, _culture));
    }

    [Fact]
    public void InvertedBoolConverter_ConvertBack_inverts_input()
    {
        var converter = new InvertedBoolConverter();
        Assert.False((bool)converter.ConvertBack(true, typeof(bool), null, _culture));
        Assert.True((bool)converter.ConvertBack(false, typeof(bool), null, _culture));
    }

    [Fact]
    public void DateTimeToStringConverter_formats_using_short_date_by_default()
    {
        var converter = new DateTimeToStringConverter();
        var date = new DateTime(2026, 10, 21);
        object result = converter.Convert(date, typeof(string), null, _culture);
        Assert.IsType<string>(result);
        Assert.Equal(date.ToString("d", _culture), result);
    }

    [Fact]
    public void DateTimeToStringConverter_uses_explicit_format_parameter()
    {
        var converter = new DateTimeToStringConverter();
        var date = new DateTime(2026, 10, 21, 14, 30, 0);
        object result = converter.Convert(date, typeof(string), "T", _culture);
        Assert.Equal(date.ToString("T", _culture), result);
    }

    [Fact]
    public void DateTimeToStringConverter_handles_nullable_and_DateTimeOffset()
    {
        var converter = new DateTimeToStringConverter();
        DateTime? nullable = new DateTime(2026, 10, 21);
        Assert.Equal(nullable.Value.ToString("d", _culture), converter.Convert(nullable, typeof(string), null, _culture));

        var offset = new DateTimeOffset(2026, 10, 21, 14, 30, 0, TimeSpan.Zero);
        Assert.Contains("2026", (string)converter.Convert(offset, typeof(string), "d", _culture));
    }

    [Fact]
    public void DateTimeToStringConverter_null_returns_empty_string()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal(string.Empty, converter.Convert(null, typeof(string), null, _culture));
        Assert.Equal(string.Empty, converter.Convert("not a date", typeof(string), null, _culture));
    }

    [Fact]
    public void DateTimeToStringConverter_ConvertBack_parses_exact_format()
    {
        var converter = new DateTimeToStringConverter();
        var date = new DateTime(2026, 10, 21);
        string text = date.ToString("d", _culture);
        object back = converter.ConvertBack(text, typeof(DateTime), "d", _culture);
        Assert.Equal(date, back);
    }

    [Fact]
    public void DateTimeToStringConverter_ConvertBack_returns_MinValue_for_invalid_input()
    {
        var converter = new DateTimeToStringConverter();
        Assert.Equal(DateTime.MinValue, converter.ConvertBack(null, typeof(DateTime), null, _culture));
        Assert.Equal(DateTime.MinValue, converter.ConvertBack("definitely not a date", typeof(DateTime), null, _culture));
    }

    [Fact]
    public void EnumToStringConverter_returns_Description_when_present()
    {
        var converter = new EnumToStringConverter();
        Assert.Equal("Completed", converter.Convert(Enums.TaskStatus.Completed, typeof(string), null, _culture));
        Assert.Equal("My Notes", converter.Convert(Enums.ListCategory.MyNotes, typeof(string), null, _culture));
    }

    [Fact]
    public void EnumToStringConverter_falls_back_to_member_name()
    {
        var converter = new EnumToStringConverter();
        // We exercise the fallback by converting a synthetic enum value
        // (DayOfWeek) that has no [Description] on its members.
        Assert.Equal("Monday", converter.Convert(DayOfWeek.Monday, typeof(string), null, _culture));
    }

    [Fact]
    public void EnumToStringConverter_null_or_non_enum_returns_safe_value()
    {
        var converter = new EnumToStringConverter();
        Assert.Equal(string.Empty, converter.Convert(null, typeof(string), null, _culture));
        Assert.Equal("hello", converter.Convert("hello", typeof(string), null, _culture));
    }

    [Fact]
    public void EnumToStringConverter_ConvertBack_parses_named_and_numeric_strings()
    {
        var converter = new EnumToStringConverter();
        Assert.Equal(Enums.TaskStatus.Completed, converter.ConvertBack("Completed", typeof(Enums.TaskStatus), null, _culture));
        Assert.Equal(Enums.TaskStatus.Completed, converter.ConvertBack("completed", typeof(Enums.TaskStatus), null, _culture));
        Assert.Equal(Enums.TaskStatus.Active, converter.ConvertBack("0", typeof(Enums.TaskStatus), null, _culture));
    }

    [Fact]
    public void EnumToStringConverter_ConvertBack_returns_default_for_invalid_input()
    {
        var converter = new EnumToStringConverter();
        // First value of TaskStatus is Active.
        Assert.Equal(Enums.TaskStatus.Active, converter.ConvertBack("not-a-valid-value", typeof(Enums.TaskStatus), null, _culture));
        Assert.Equal(Enums.TaskStatus.Active, converter.ConvertBack(string.Empty, typeof(Enums.TaskStatus), null, _culture));
    }
}
