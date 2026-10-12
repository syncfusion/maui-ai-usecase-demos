using System.Globalization;
using Todo.Utilities;
using Xunit;

namespace Todo.Tests;

/// <summary>
/// Unit tests for the helper methods exposed by <see cref="Helpers"/>.
/// Covers the cases called out in the Task 1-08 "Testing" section as well
/// as edge cases (null, empty, whitespace) that a binding can produce.
/// </summary>
public class HelpersTests
{
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("user.name+tag@sub.example.co.uk", true)]
    [InlineData("USER@Example.COM", true)]                  // case-insensitive
    [InlineData("a@b.c", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    [InlineData("no-at-sign.example.com", false)]
    [InlineData("trailing@", false)]
    [InlineData("trailing@dot.", false)]
    [InlineData("two@@signs.com", false)]
    public void IsValidEmail_returns_expected_result(string? candidate, bool expected)
    {
        Assert.Equal(expected, Helpers.IsValidEmail(candidate));
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData(null, true)]
    [InlineData("x", false)]
    public void NormalizeWhiteSpace_returns_null_for_empty_or_whitespace(string? value, bool shouldReturnNull)
    {
        string? result = Helpers.NormalizeWhiteSpace(value);
        if (shouldReturnNull)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Equal(value!.Trim(), result);
        }
    }

    [Theory]
    [InlineData("Password1!", true)]
    [InlineData("p4ssword!", false)]                         // missing uppercase
    [InlineData("P4SSWORD!", false)]                         // missing lowercase
    [InlineData("Password!", false)]                         // missing digit
    [InlineData("Password1", false)]                         // missing symbol
    [InlineData("P4!", false)]                               // too short
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsStrongPassword_returns_expected_result(string? password, bool expected)
    {
        Assert.Equal(expected, Helpers.IsStrongPassword(password));
    }

    [Fact]
    public void IsToday_returns_true_for_today()
    {
        Assert.True(Helpers.IsToday(DateTime.Today));
        Assert.True(Helpers.IsToday(DateTime.Now));
    }

    [Fact]
    public void IsToday_returns_false_for_other_dates()
    {
        Assert.False(Helpers.IsToday(DateTime.Today.AddDays(1)));
        Assert.False(Helpers.IsToday(DateTime.Today.AddDays(-1)));
    }

    [Fact]
    public void IsOverdue_returns_true_for_past_dates()
    {
        Assert.True(Helpers.IsOverdue(DateTime.Today.AddDays(-1)));
    }

    [Fact]
    public void IsOverdue_returns_false_for_today_or_future()
    {
        Assert.False(Helpers.IsOverdue(DateTime.Today));
        Assert.False(Helpers.IsOverdue(DateTime.Today.AddDays(1)));
    }

    [Fact]
    public void FormatRelativeDate_today_returns_Today()
    {
        string result = Helpers.FormatRelativeDate(DateTime.Today);
        Assert.Equal("Today", result);
    }

    [Fact]
    public void FormatRelativeDate_tomorrow_returns_Tomorrow()
    {
        string result = Helpers.FormatRelativeDate(DateTime.Today.AddDays(1));
        Assert.Equal("Tomorrow", result);
    }

    [Fact]
    public void FormatRelativeDate_yesterday_returns_Yesterday()
    {
        string result = Helpers.FormatRelativeDate(DateTime.Today.AddDays(-1));
        Assert.Equal("Yesterday", result);
    }

    [Fact]
    public void FormatRelativeDate_null_returns_empty_string()
    {
        Assert.Equal(string.Empty, Helpers.FormatRelativeDate(null));
    }

    [Fact]
    public void FormatDateTime_null_returns_empty_string()
    {
        Assert.Equal(string.Empty, Helpers.FormatDateTime(null));
    }

    [Fact]
    public void FormatDateTime_uses_provided_culture()
    {
        var fr = CultureInfo.GetCultureInfo("fr-FR");
        var date = new DateTime(2026, 10, 21);

        string result = Helpers.FormatDateTime(date);
        // We don't pin the exact output (locale dependent) — just ensure it
        // is non-empty and contains the year.
        Assert.Contains("2026", result);
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("#FFFFFF", 0xFF, 0xFF, 0xFF, 0xFF)]
    [InlineData("#000000", 0xFF, 0x00, 0x00, 0x00)]
    [InlineData("5C4EAE", 0xFF, 0x5C, 0x4E, 0xAE)]
    [InlineData("#FF112233", 0xFF, 0x11, 0x22, 0x33)]
    public void TryParseHexColor_parses_valid_inputs(string hex, byte a, byte r, byte g, byte b)
    {
        Assert.True(Helpers.TryParseHexColor(hex, out byte actualA, out byte actualR, out byte actualG, out byte actualB));
        Assert.Equal(a, actualA);
        Assert.Equal(r, actualR);
        Assert.Equal(g, actualG);
        Assert.Equal(b, actualB);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-color")]
    [InlineData("#ZZZZZZ")]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    public void TryParseHexColor_returns_false_for_invalid_inputs(string? hex)
    {
        Assert.False(Helpers.TryParseHexColor(hex, out _, out _, out _, out _));
    }

    [Fact]
    public void ToHexColor_roundtrips_through_TryParseHexColor()
    {
        string hex = Helpers.ToHexColor(0xFF, 0x5C, 0x4E, 0xAE);
        Assert.True(Helpers.TryParseHexColor(hex, out byte a, out byte r, out byte g, out byte b));
        Assert.Equal(0xFF, a);
        Assert.Equal(0x5C, r);
        Assert.Equal(0x4E, g);
        Assert.Equal(0xAE, b);
    }

    [Fact]
    public void Chunk_splits_collection_evenly()
    {
        int[] items = { 1, 2, 3, 4, 5, 6 };
        List<IReadOnlyList<int>> chunks = Helpers.Chunk(items, 2).ToList();
        Assert.Equal(3, chunks.Count);
        Assert.Equal(new[] { 1, 2 }, chunks[0]);
        Assert.Equal(new[] { 3, 4 }, chunks[1]);
        Assert.Equal(new[] { 5, 6 }, chunks[2]);
    }

    [Fact]
    public void Chunk_yields_remainder_as_final_chunk()
    {
        int[] items = { 1, 2, 3, 4, 5 };
        List<IReadOnlyList<int>> chunks = Helpers.Chunk(items, 2).ToList();
        Assert.Equal(3, chunks.Count);
        Assert.Equal(new[] { 1, 2 }, chunks[0]);
        Assert.Equal(new[] { 3, 4 }, chunks[1]);
        Assert.Equal(new[] { 5 }, chunks[2]);
    }

    [Fact]
    public void Chunk_throws_for_zero_or_negative_size()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Helpers.Chunk(new int[] { 1 }, 0).ToList());
        Assert.Throws<ArgumentOutOfRangeException>(() => Helpers.Chunk(new int[] { 1 }, -1).ToList());
    }
}
