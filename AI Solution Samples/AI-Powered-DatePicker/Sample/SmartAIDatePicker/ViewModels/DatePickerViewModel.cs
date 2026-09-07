using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using SmartAIDatePicker.AIService;

namespace SmartAIDatePicker.ViewModels;

public sealed class DatePickerViewModel : INotifyPropertyChanged
{
    private readonly IAzureOpenAIService azureAIService;

    private string dateRequest = string.Empty;
    private DateTime? selectedDate = DateTime.Today;
    private DateTime? pickerDate = DateTime.Today;
    private bool isBusy;
    private bool isApplyingAIResult;
    private string placeholder = "Try: next leap day, Next christmas...";
    private DateTime? minimumDate;
    private DateTime? maximumDate;
    private int dayInterval = 1;
    private int monthInterval = 1;
    private int yearInterval = 1;
    private bool enableLooping = true;
    private string dateFormat = "dd_MM_yyyy";
    private bool hasLocalDateFilter;
    private bool hasLocalLoopingSetting;

    private sealed record DateResolutionResult(
        DateTime? Date,
        string? ErrorMessage,
        DateTime? MinimumDate = null,
        DateTime? MaximumDate = null,
        IReadOnlyList<DateTime>? BlackoutDates = null,
        int DayInterval = 1,
        int MonthInterval = 1,
        int YearInterval = 1,
        bool EnableLooping = true,
        string DateFormat = "dd_MM_yyyy",
        bool HasPickerConfiguration = false)
    {
        public bool IsSuccess => Date.HasValue;
    }

    private sealed record AIResolution(
        string? Date,
        string? MinimumDate,
        string? MaximumDate,
        List<string>? BlackoutDates,
        int? DayInterval,
        int? MonthInterval,
        int? YearInterval,
        bool? EnableLooping,
        string? Format,
        string? Error);

    public DatePickerViewModel(IAzureOpenAIService azureAIService)
    {
        this.azureAIService = azureAIService;

        SearchCommand = new Command(async () => await ResolveDateRequestAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand SearchCommand { get; }

    public string DateRequest
    {
        get => dateRequest;
        set => SetProperty(ref dateRequest, value ?? string.Empty);
    }

    public DateTime? SelectedDate
    {
        get => selectedDate;
        set
        {
            if (SetProperty(ref selectedDate, value) && value.HasValue)
            {
                OnPropertyChanged(nameof(SelectedDateText));
            }
        }
    }

    public DateTime? PickerDate
    {
        get => pickerDate;
        set
        {
            if (SetProperty(ref pickerDate, value) &&
                !isApplyingAIResult &&
                value.HasValue)
            {
                SelectedDate = value;
            }
        }
    }

    public DateTime? MinimumDate
    {
        get => minimumDate;
        private set => SetProperty(ref minimumDate, value);
    }

    public DateTime? MaximumDate
    {
        get => maximumDate;
        private set => SetProperty(ref maximumDate, value);
    }

    public ObservableCollection<DateTime> BlackoutDates { get; } = new();

    public int DayInterval
    {
        get => dayInterval;
        private set => SetProperty(ref dayInterval, value);
    }

    public int MonthInterval
    {
        get => monthInterval;
        private set => SetProperty(ref monthInterval, value);
    }

    public int YearInterval
    {
        get => yearInterval;
        private set => SetProperty(ref yearInterval, value);
    }

    public bool EnableLooping
    {
        get => enableLooping;
        private set => SetProperty(ref enableLooping, value);
    }

    public string DateFormat
    {
        get => dateFormat;
        private set => SetProperty(ref dateFormat, value);
    }

    public string SelectedDateText =>
        SelectedDate?.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture) ?? string.Empty;

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetProperty(ref isBusy, value))
            {
                OnPropertyChanged(nameof(IsSearchIconVisible));
                OnPropertyChanged(nameof(IsLoadingVisible));
                OnPropertyChanged(nameof(IsEditorEnabled));
                OnPropertyChanged(nameof(IsPickerEnabled));
                OnPropertyChanged(nameof(SearchButtonOpacity));
            }
        }
    }

    public string Placeholder
    {
        get => placeholder;
        private set => SetProperty(ref placeholder, value);
    }

    public bool IsSearchIconVisible => !IsBusy;

    public bool IsLoadingVisible => IsBusy;

    public bool IsEditorEnabled => !IsBusy;

    public bool IsPickerEnabled => !IsBusy;

    public double SearchButtonOpacity =>
        IsBusy || string.IsNullOrWhiteSpace(DateRequest) ? 0.6 : 1;

    public Color SearchButtonBackgroundColor =>
        string.IsNullOrWhiteSpace(DateRequest)
            ? Color.FromArgb("#DFD8F7")
            : Color.FromArgb("#6B4FD3");

    private async Task ResolveDateRequestAsync()
    {
        var request = DateRequest.Trim();

        if (IsBusy || string.IsNullOrWhiteSpace(request))
        {
            return;
        }

        IsBusy = true;
        DateRequest = string.Empty;
        Placeholder = "Finding date...";
        ResetPickerState();
        ApplyDateFilter(request);
        ApplyLocalPickerSettings(request);

        try
        {
            var result = await GetDateFromAIAsync(request);

            if (result.IsSuccess || result.HasPickerConfiguration)
            {
                ApplyPickerConfiguration(result);

                var dateToSelect = hasLocalDateFilter && MinimumDate.HasValue
                    ? MinimumDate.Value
                    : result.Date;

                if (dateToSelect.HasValue)
                {
                    isApplyingAIResult = true;
                    PickerDate = dateToSelect.Value;
                    isApplyingAIResult = false;
                    SelectedDate = dateToSelect.Value;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (HttpRequestException)
        {
        }

        isApplyingAIResult = false;
        Placeholder = "Try: next leap day, Next christmas...";
        IsBusy = false;
    }

    private async Task<DateResolutionResult> GetDateFromAIAsync(string request)
    {
        var prompt =
            $"Reference date (today): {DateTime.Today:yyyy-MM-dd} ({DateTime.Today:dddd}); " +
            $"user locale: {CultureInfo.CurrentCulture.Name}. " +
            "Resolve the following natural-language question to one specific Gregorian calendar date. " +
            "Return a JSON object with date and any requested picker settings. Supported settings are " +
            "minimumDate, maximumDate, blackoutDates, dayInterval, monthInterval, yearInterval, " +
            "enableLooping, and format. Dates must be yyyy-MM-dd. Use null for settings not requested. " +
            "For filters such as before 2010 or before December 2010, set maximumDate to the end of " +
            "the requested year or month. For after February 2023, set minimumDate to 2023-03-01. " +
            "For 'select this date' or 'choose today', set date to the reference date. " +
            "It may refer to a historical event, war, holiday, weekday, leap day, anniversary, or relative date. " +
            "For a war, use its start date unless the user asks for its end. Apply the exact meaning of " +
            "next, upcoming, this, last, from now, and after; next/upcoming must be strictly after the reference date. " +
            "Use the next occurrence for recurring holidays without a year. Interpret Independence Day without a country " +
            "as US Independence Day on July 4. Return only the JSON object or INVALID_REQUEST. " +
            $"Question: {request}";

        var completion = await azureAIService.GetCompletion(prompt);

        if (!string.IsNullOrWhiteSpace(completion))
        {
            var normalized = completion.Trim();

            if (string.Equals(normalized, "INVALID_REQUEST", StringComparison.OrdinalIgnoreCase))
            {
                return new DateResolutionResult(null, "The request is too vague to resolve to a single valid date.");
            }

            var json = Regex.Match(normalized, @"\{.*\}", RegexOptions.Singleline);
            if (json.Success)
            {
                try
                {
                    var resolution = System.Text.Json.JsonSerializer.Deserialize<AIResolution>(
                        json.Value,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (resolution is not null &&
                        TryParseConfiguration(resolution, out var configuration) &&
                        IsValidConfigurationRange(configuration))
                    {
                        if (TryParseDate(resolution.Date, out var aiDate) &&
                            IsValidAIResult(
                                request,
                                aiDate,
                                GetEffectiveMinimum(configuration),
                                GetEffectiveMaximum(configuration)))
                        {
                            return configuration with
                            {
                                Date = aiDate,
                                MinimumDate = GetEffectiveMinimum(configuration),
                                MaximumDate = GetEffectiveMaximum(configuration)
                            };
                        }

                        if (HasRequestedConfiguration(resolution))
                        {
                            return configuration with
                            {
                                MinimumDate = GetEffectiveMinimum(configuration),
                                MaximumDate = GetEffectiveMaximum(configuration),
                                HasPickerConfiguration = true
                            };
                        }
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // Use the local fallback when the model does not return valid JSON.
                }
            }

            var match = Regex.Match(normalized, @"\d{4}-\d{2}-\d{2}");
            if (match.Success &&
                TryParseDate(match.Value, out var legacyDate) &&
                IsValidAIResult(request, legacyDate))
            {
                return new DateResolutionResult(legacyDate, null);
            }
        }

        var fallbackDate = CalculateDate(request);
        return fallbackDate.HasValue
            && IsValidAIResult(request, fallbackDate.Value)
            ? new DateResolutionResult(fallbackDate, null)
            : new DateResolutionResult(null, "Unable to resolve the requested date.");
    }

    private void ApplyDateFilter(string request)
    {
        var numericDateMatch = Regex.Match(
            request,
            @"(?<!\w)(?<operator><=|>=|<|>|before|after|prior\s+to|earlier\s+than|later\s+than)\s*(?<first>\d{1,2})[/-](?<second>\d{1,2})[/-](?<year>\d{4})(?!\d)",
            RegexOptions.IgnoreCase);

        if (numericDateMatch.Success &&
            TryParseUserDate(
                numericDateMatch.Groups["first"].Value,
                numericDateMatch.Groups["second"].Value,
                numericDateMatch.Groups["year"].Value,
                out var numericDate))
        {
            hasLocalDateFilter = true;
            ApplyDateBoundary(
                numericDateMatch.Groups["operator"].Value,
                numericDate,
                request,
                exactDate: true);
            ApplyCurrentDateRange();
            return;
        }

        var match = Regex.Match(
            request,
            @"(?<!\w)(?:date|dates?|year)?\s*(?<operator><=|>=|<|>|before|after|prior\s+to|earlier\s+than|later\s+than)\s*(?:(?<month>\d{1,2}|January|February|March|April|May|June|July|August|September|October|November|December)\s+)?(?<year>\d{4})(?!\d)",
            RegexOptions.IgnoreCase);

        if (!match.Success || !int.TryParse(match.Groups["year"].Value, out var year))
        {
            return;
        }

        var comparison = match.Groups["operator"].Value;
        var hasMonth = match.Groups["month"].Success;
        var month = hasMonth &&
                    int.TryParse(match.Groups["month"].Value, out var numericMonth)
            ? numericMonth
            : hasMonth && TryParseMonth(match.Groups["month"].Value, out var namedMonth)
                ? namedMonth
                : 0;

        if (hasMonth && month is < 1 or > 12)
        {
            return;
        }

        var date = hasMonth
            ? new DateTime(year, month, 1)
            : new DateTime(year, 1, 1);

        ApplyDateBoundary(comparison, date, request, hasMonth);
        hasLocalDateFilter = true;
        ApplyCurrentDateRange();
    }

    private void ResetPickerState()
    {
        hasLocalDateFilter = false;
        hasLocalLoopingSetting = false;
        MinimumDate = null;
        MaximumDate = null;
        BlackoutDates.Clear();
        DayInterval = 1;
        MonthInterval = 1;
        YearInterval = 1;
        EnableLooping = true;
        DateFormat = "dd_MM_yyyy";

        isApplyingAIResult = true;
        PickerDate = DateTime.Today;
        SelectedDate = DateTime.Today;
        isApplyingAIResult = false;
    }

    private void ApplyLocalPickerSettings(string request)
    {
        if (Regex.IsMatch(
                request,
                @"\b(?:disable|turn\s+off|switch\s+off|stop|remove)\b.*\b(?:looping|loop)\b",
                RegexOptions.IgnoreCase))
        {
            EnableLooping = false;
            hasLocalLoopingSetting = true;
        }
        else if (Regex.IsMatch(
                     request,
                     @"\b(?:enable|turn\s+on|switch\s+on|start)\b.*\b(?:looping|loop)\b",
                     RegexOptions.IgnoreCase))
        {
            EnableLooping = true;
            hasLocalLoopingSetting = true;
        }
    }

    private void ApplyDateBoundary(
        string comparison,
        DateTime date,
        string request,
        bool hasMonth = false,
        bool exactDate = false)
    {
        var hidesDatesAfterBoundary = Regex.IsMatch(
            request,
            @"\b(?:do\s*not|don't|dont|no|without|remove|removes|removed|hide|hides|hidden|exclude|excludes|excluding|omit|omits)\b.*\bafter\b",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var hidesDatesBeforeBoundary = Regex.IsMatch(
            request,
            @"\b(?:do\s*not|don't|dont|no|without|remove|removes|removed|hide|hides|hidden|exclude|excludes|excluding|omit|omits)\b.*\bbefore\b",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        switch (comparison.ToLowerInvariant())
        {
            case "<":
            case "before":
            case "<=":
                if (hidesDatesBeforeBoundary)
                {
                    MinimumDate = exactDate
                        ? date
                        : hasMonth
                            ? date
                            : new DateTime(date.Year, 1, 1);
                }
                else
                {
                    MaximumDate = exactDate
                        ? date
                        : hasMonth
                    ? new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month))
                    : new DateTime(date.Year, 12, 31);
                }
                break;
            case ">":
            case "after":
                if (hidesDatesAfterBoundary)
                {
                    MaximumDate = exactDate
                        ? date
                        : hasMonth
                        ? new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month))
                        : new DateTime(date.Year, 12, 31);
                }
                else
                {
                    MinimumDate = exactDate
                        ? date.AddDays(1)
                        : hasMonth
                            ? date.AddMonths(1)
                        : new DateTime(date.Year + 1, 1, 1);
                }
                break;
            case ">=":
                MinimumDate = date;
                break;
        }
    }

    private void ApplyCurrentDateRange()
    {
        var dateToSelect = MinimumDate ?? MaximumDate;

        if (!dateToSelect.HasValue)
        {
            return;
        }

        ApplyDateRange(MinimumDate, MaximumDate, dateToSelect);
    }

    private void ApplyDateRange(
        DateTime? minimum,
        DateTime? maximum,
        DateTime? dateToSelect)
    {
        if (!IsValidConfigurationRange(minimum, maximum))
        {
            return;
        }

        isApplyingAIResult = true;
        PickerDate = null;
        MinimumDate = minimum;
        MaximumDate = maximum;

        if (dateToSelect.HasValue)
        {
            var clampedDate = dateToSelect.Value;

            if (minimum.HasValue && clampedDate < minimum.Value)
            {
                clampedDate = minimum.Value;
            }

            if (maximum.HasValue && clampedDate > maximum.Value)
            {
                clampedDate = maximum.Value;
            }

            PickerDate = clampedDate;
            SelectedDate = clampedDate;
        }

        isApplyingAIResult = false;
    }

    private void AdjustPickerDateToRange()
    {
        if (PickerDate.HasValue &&
            ((MinimumDate.HasValue && PickerDate.Value < MinimumDate.Value) ||
             (MaximumDate.HasValue && PickerDate.Value > MaximumDate.Value)))
        {
            isApplyingAIResult = true;
            PickerDate = MinimumDate ?? MaximumDate;
            SelectedDate = PickerDate;
            isApplyingAIResult = false;
        }
    }

    private static bool TryParseUserDate(
        string first,
        string second,
        string year,
        out DateTime date)
    {
        return DateTime.TryParse(
            $"{first}/{second}/{year}",
            CultureInfo.CurrentCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out date);
    }

    private static bool TryParseMonth(string value, out int month)
    {
        if (DateTime.TryParseExact(
                value,
                new[] { "MMMM", "MMM" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            month = date.Month;
            return true;
        }

        month = 0;
        return false;
    }

    private void ApplyPickerConfiguration(DateResolutionResult result)
    {
        var configuredMinimum = hasLocalDateFilter
            ? MinimumDate
            : LaterDate(MinimumDate, result.MinimumDate);
        var configuredMaximum = hasLocalDateFilter
            ? MaximumDate
            : EarlierDate(MaximumDate, result.MaximumDate);

        if (!IsValidConfigurationRange(configuredMinimum, configuredMaximum))
        {
            return;
        }

        ApplyDateRange(configuredMinimum, configuredMaximum, PickerDate);
        BlackoutDates.Clear();

        if (result.BlackoutDates is not null)
        {
            foreach (var date in result.BlackoutDates)
            {
                BlackoutDates.Add(date);
            }
        }

        DayInterval = result.DayInterval;
        MonthInterval = result.MonthInterval;
        YearInterval = result.YearInterval;
        if (!hasLocalLoopingSetting)
        {
            EnableLooping = result.EnableLooping;
        }
        DateFormat = result.DateFormat;
        ApplyCurrentDateRange();
    }

    private DateTime? GetEffectiveMinimum(DateResolutionResult configuration) =>
        hasLocalDateFilter
            ? MinimumDate
            : LaterDate(MinimumDate, configuration.MinimumDate);

    private DateTime? GetEffectiveMaximum(DateResolutionResult configuration) =>
        hasLocalDateFilter
            ? MaximumDate
            : EarlierDate(MaximumDate, configuration.MaximumDate);

    private static bool IsValidConfigurationRange(DateResolutionResult result) =>
        IsValidConfigurationRange(result.MinimumDate, result.MaximumDate);

    private static bool IsValidConfigurationRange(DateTime? minimum, DateTime? maximum) =>
        !minimum.HasValue || !maximum.HasValue || minimum <= maximum;

    private static DateTime? LaterDate(DateTime? first, DateTime? second) =>
        first.HasValue && second.HasValue
            ? first.Value >= second.Value ? first : second
            : first ?? second;

    private static DateTime? EarlierDate(DateTime? first, DateTime? second) =>
        first.HasValue && second.HasValue
            ? first.Value <= second.Value ? first : second
            : first ?? second;

    private static bool TryParseDate(string? value, out DateTime date) =>
        DateTime.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);

    private static bool TryParseConfiguration(AIResolution resolution, out DateResolutionResult result)
    {
        result = new DateResolutionResult(null, null);

        if ((resolution.DayInterval is < 1 or > 31) ||
            (resolution.MonthInterval is < 1 or > 12) ||
            (resolution.YearInterval is < 1 or > 100) ||
            (!string.IsNullOrWhiteSpace(resolution.Format) &&
             !AllowedFormats.Contains(resolution.Format, StringComparer.Ordinal)))
        {
            return false;
        }

        DateTime? minimum = TryParseDate(resolution.MinimumDate, out var minimumValue)
            ? minimumValue
            : null;
        DateTime? maximum = TryParseDate(resolution.MaximumDate, out var maximumValue)
            ? maximumValue
            : null;

        if (minimum.HasValue && maximum.HasValue && minimum > maximum)
        {
            return false;
        }

        var blackoutDates = new List<DateTime>();
        if (resolution.BlackoutDates is not null)
        {
            foreach (var value in resolution.BlackoutDates)
            {
                if (!TryParseDate(value, out var date))
                {
                    return false;
                }

                blackoutDates.Add(date);
            }
        }

        result = new DateResolutionResult(
            null,
            null,
            minimum,
            maximum,
            blackoutDates,
            resolution.DayInterval ?? 1,
            resolution.MonthInterval ?? 1,
            resolution.YearInterval ?? 1,
            resolution.EnableLooping ?? true,
            resolution.Format ?? "dd_MM_yyyy",
            false);

        return true;
    }

    private static bool HasRequestedConfiguration(AIResolution resolution) =>
        resolution.MinimumDate is not null ||
        resolution.MaximumDate is not null ||
        resolution.BlackoutDates is not null ||
        resolution.DayInterval.HasValue ||
        resolution.MonthInterval.HasValue ||
        resolution.YearInterval.HasValue ||
        resolution.EnableLooping.HasValue ||
        resolution.Format is not null;

    private static readonly string[] AllowedFormats =
    {
        "dd_MM_yyyy",
        "MM_dd_yyyy",
        "yyyy_MM_dd",
        "dd/MM/yyyy",
        "MM/dd/yyyy",
        "yyyy-MM-dd",
        "dd MMM yyyy",
        "dd MMMM yyyy"
    };

    private static DateTime? CalculateDate(string request)
    {
        var text = request.ToLowerInvariant();
        var today = DateTime.Today;

        if (text.Contains("today")) return today;
        if (text.Contains("tomorrow")) return today.AddDays(1);
        if (text.Contains("next friday")) return NextWeekday(today, DayOfWeek.Friday);
        if (text.Contains("two weeks")) return today.AddDays(14);
        if (text.Contains("three weeks")) return today.AddDays(21);
        if (text.Contains("30 days")) return today.AddDays(30);
        if (text.Contains("last working day of this month")) return LastWorkingDay(today.Year, today.Month);

        if (text.Contains("first business day of next month"))
        {
            var nextMonth = today.AddMonths(1);
            return FirstBusinessDay(nextMonth.Year, nextMonth.Month);
        }

        if (text.Contains("independence day")) return Holiday(today.Year, 7, 4);
        if (text.Contains("christmas")) return Holiday(today.Year, 12, 25);
        if (text.Contains("new year's") || text.Contains("new years")) return Holiday(today.Year, 1, 1);
        if (text.Contains("thanksgiving")) return NthWeekday(today.Year, 11, DayOfWeek.Thursday, 4);
        if (text.Contains("diwali")) return Diwali(today.Year);

        return null;
    }

    private bool IsValidAIResult(
        string request,
        DateTime date,
        DateTime? minimumDateOverride = null,
        DateTime? maximumDateOverride = null)
    {
        var text = request.ToLowerInvariant();
        var requiresFutureDate = text.Contains("next") ||
                                 text.Contains("upcoming") ||
                                 text.Contains("coming");

        if (requiresFutureDate && date.Date <= DateTime.Today)
        {
            return false;
        }

         var minimum = minimumDateOverride ?? MinimumDate;
         var maximum = maximumDateOverride ?? MaximumDate;

         return (!minimum.HasValue || date.Date >= minimum.Value.Date) &&
             (!maximum.HasValue || date.Date <= maximum.Value.Date);
    }

    private static DateTime NextWeekday(DateTime date, DayOfWeek weekday)
    {
        var days = ((int)weekday - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(days == 0 ? 7 : days);
    }

    private static DateTime FirstBusinessDay(int year, int month)
    {
        var date = new DateTime(year, month, 1);

        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static DateTime LastWorkingDay(int year, int month)
    {
        var date = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(-1);
        }

        return date;
    }

    private static DateTime FirstWeekday(DateTime month, DayOfWeek weekday)
    {
        var first = new DateTime(month.Year, month.Month, 1);
        return first.AddDays(((int)weekday - (int)first.DayOfWeek + 7) % 7);
    }

    private static DateTime NthWeekday(int year, int month, DayOfWeek weekday, int occurrence) =>
        FirstWeekday(new DateTime(year, month, 1), weekday).AddDays((occurrence - 1) * 7);

    private static DateTime Holiday(int year, int month, int day)
    {
        var date = new DateTime(year, month, day);
        return date < DateTime.Today ? date.AddYears(1) : date;
    }

    private static DateTime Diwali(int year)
    {
        var dates = new Dictionary<int, DateTime>
        {
            [2024] = new(2024, 11, 1),
            [2025] = new(2025, 10, 20),
            [2026] = new(2026, 11, 8),
            [2027] = new(2027, 10, 29),
            [2028] = new(2028, 10, 17),
            [2029] = new(2029, 11, 5),
            [2030] = new(2030, 10, 26)
        };

        return dates.TryGetValue(year, out var date) && date >= DateTime.Today
            ? date
            : dates.TryGetValue(year + 1, out date)
                ? date
                : new DateTime(year, 10, 31);
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);

        if (propertyName is nameof(DateRequest))
        {
            OnPropertyChanged(nameof(SearchButtonBackgroundColor));
            OnPropertyChanged(nameof(SearchButtonOpacity));
        }

        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
