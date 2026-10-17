using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ListView_IntentGrouping.Models;
using ListView_IntentGrouping.Services;

namespace ListView_IntentGrouping.ViewModels;

/// <summary>
/// Drives the MainPage.
///
/// The page exposes a single input field plus a "Group" button.
/// Users type simple prompts like
///   * "group by feedbacks"
///   * "group by ratings"
///   * "group by priority"
///   * "group by month"
/// and the view-model rebuilds the visible groups accordingly,
/// re-stamping each item's <see cref="FeedbackItem.GroupKey"/>
/// with a <see cref="GroupHeaderInfo"/> the SfListView can group
/// by reference. The page code-behind installs and removes the
/// single <c>GroupDescriptor</c> based on the
/// <see cref="CurrentGrouping"/> value.
///
/// The backing ticket list comes from
/// <see cref="FeedbackDataset.Tickets"/>; each ticket carries the
/// required fields (Ticket Name, Sender Name, Month and Year,
/// Rating, Priority, Short Description) so the card template can
/// render them directly.
/// </summary>
public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly IIntentAnalyzer _analyzer;
    private string _prompt = string.Empty;
    private string _statusMessage = "Ready";
    private bool _isBusy;
    private GroupingKind _currentGrouping = GroupingKind.None;
    private string _analyzerLabel = string.Empty;
    private string _credentialBanner = string.Empty;

    /// <summary>
    /// Pristine working snapshot of every ticket. The view-model
    /// keeps this list so the Group button can rebuild the visible
    /// groupings repeatedly without needing to clone the dataset
    /// on every click.
    /// </summary>
    private readonly List<FeedbackItem> _sourceTickets = new();

    public MainPageViewModel(IIntentAnalyzer analyzer)
    {
        _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
        _analyzerLabel = string.IsNullOrWhiteSpace(_analyzer.DisplayName)
            ? "AI analyzer"
            : _analyzer.DisplayName;
        GroupCommand = new Command(async () => await GroupAsync());
    }

    /// <summary>
    /// Visible ticket list, in display order, sharing references
    /// with the internal working snapshot. The SfListView binds
    /// directly to this collection.
    /// </summary>
    public ObservableCollection<FeedbackItem> Items { get; } = new();

    /// <summary>
    /// Distinct group headers currently in scope, in display
    /// order. The page's header template drives the per-group
    /// title, icon, accent colour, summary, and item count from
    /// this list's <see cref="GroupHeaderInfo"/> records.
    /// </summary>
    public ObservableCollection<GroupHeaderInfo> GroupHeaders { get; } = new();

    /// <summary>
    /// The user's prompt. Bound to the input field on MainPage.
    /// </summary>
    public string Prompt
    {
        get => _prompt;
        set { if (_prompt != value) { _prompt = value; OnPropertyChanged(); } }
    }

    /// <summary>
    /// Free-form status / hint line shown below the input row.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set { if (_statusMessage != value) { _statusMessage = value; OnPropertyChanged(); } }
    }

    /// <summary>
    /// Friendly label of the analyzer currently driving the
    /// grouping decisions (e.g. "Azure OpenAI (gpt-4o)" or
    /// "Local rule-based"). Surfaced in the status banner so the
    /// user always sees which engine produced the grouping.
    /// </summary>
    public string AnalyzerLabel
    {
        get => _analyzerLabel;
        private set { if (_analyzerLabel != value) { _analyzerLabel = value; OnPropertyChanged(); } }
    }

    /// <summary>
    /// Small banner that surfaces the *currently active*
    /// Azure OpenAI credentials (host + key fingerprint +
    /// deployment). Set on startup and refreshed on every
    /// Group click so the user can always verify the key the
    /// app is sending matches the one in the Azure portal.
    /// </summary>
    public string CredentialBanner
    {
        get => _credentialBanner;
        private set { if (_credentialBanner != value) { _credentialBanner = value; OnPropertyChanged(); } }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Current grouping mode. The page code-behind reads this to
    /// decide whether to install a <c>GroupDescriptor</c> (when
    /// not <see cref="GroupingKind.None"/>) and what the empty-
    /// state should display.
    /// </summary>
    public GroupingKind CurrentGrouping
    {
        get => _currentGrouping;
        private set
        {
            if (_currentGrouping == value) return;
            _currentGrouping = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsGrouped));
            OnPropertyChanged(nameof(IsUngrouped));
            OnPropertyChanged(nameof(GroupTitle));
            OnPropertyChanged(nameof(EmptyStateHint));
        }
    }

    /// <summary>True when the current view has a group descriptor.</summary>
    public bool IsGrouped => _currentGrouping != GroupingKind.None;

    /// <summary>True when the current view is flat (initial / after Reset).</summary>
    public bool IsUngrouped => _currentGrouping == GroupingKind.None;

    /// <summary>
    /// Friendly title reflecting the active grouping mode for the
    /// summary / status card placeholder text.
    /// </summary>
    public string GroupTitle => _currentGrouping switch
    {
        GroupingKind.Rating    => "Grouped by Rating",
        GroupingKind.Priority  => "Grouped by Priority",
        GroupingKind.Sentiment => "Grouped by Feedbacks",
        GroupingKind.Month     => "Grouped by Month",
        _                       => "All Tickets",
    };

    /// <summary>
    /// Hint shown in the empty-state when nothing matches the
    /// current grouping. Drawn from the active mode.
    /// </summary>
    public string EmptyStateHint => _currentGrouping switch
    {
        GroupingKind.Rating    => "No ratings to group by yet.",
        GroupingKind.Priority  => "No priorities to group by yet.",
        GroupingKind.Sentiment => "No sentiment buckets to show yet.",
        GroupingKind.Month     => "No tickets this month yet.",
        _                       => "Type a prompt above (for example: \"group by ratings\") and tap Group."
    };

    public ICommand GroupCommand { get; }

    /// <summary>
    /// First-time entry point. Loads the unified ticket list into
    /// <see cref="Items"/> so the SfListView renders a flat list
    /// below the input row. Runs the injected AI analyzer once so
    /// every ticket gets its intent classification, confidence
    /// score and re-computed priority before the user clicks
    /// "Group".
    /// </summary>
    public async Task LoadInitialItemsAsync()
    {
        try
        {
            IsBusy = true;

            _sourceTickets.Clear();
            foreach (var src in FeedbackDataset.Tickets)
            {
                _sourceTickets.Add(Clone(src));
            }

            // Run the AI once on initial load so its work is
            // visible immediately in the status banner.
            await RunAnalyzerAsync(string.Empty, CancellationToken.None);

            ReloadVisibleItems(GroupingKind.None);

            Prompt = string.Empty;
            StatusMessage = $"Loaded {_sourceTickets.Count} tickets via {_analyzer.DisplayName} — " +
                            "type a prompt such as \"group by ratings\" and tap Group.";
        }
        catch (IntentAnalysisException ex)
        {
            // The analyzer is informational on initial load; do
            // not block the flat list because of a transient AI
            // failure. Surface the message in the status banner
            // so the user can fix the credential issue. Also
            // append the active credential fingerprint so the
            // user can confirm which key the running app is
            // actually using.
            ReloadVisibleItems(GroupingKind.None);
            StatusMessage = $"Loaded {_sourceTickets.Count} tickets — {ex.Message} " +
                            $"[Active: {AzureBaseService.ActiveCredentialSummary}]";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// The "Group" button handler. Resolves the user's prompt
    /// into a <see cref="GroupingKind"/>, runs the AI analyzer
    /// (so it does the grouping work) and rebuilds the visible
    /// groups accordingly. The page code-behind installs a
    /// fresh <c>GroupDescriptor</c> via property notification.
    /// </summary>
    public async Task GroupAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = $"Asking {_analyzer.DisplayName} to group the tickets...";

            if (_sourceTickets.Count == 0)
            {
                // First click before items loaded — fall back to
                // the initial load path so the user always sees
                // a list.
                await LoadInitialItemsAsync();
                return;
            }

            var kind = GroupingPromptResolver.Resolve(Prompt);

            // The AI always runs first so the grouping is driven
            // by the analyzer's results (intent label,
            // confidence, re-computed priority). The requested
            // <see cref="GroupingKind"/> then projects those
            // results into the right buckets below.
            await RunAnalyzerAsync(Prompt, CancellationToken.None);

            // Show the live credential fingerprint after every
            // successful Group click. When the next click returns
            // 401, the Status banner will show the same fingerprint
            // so the user can match it against the key in the
            // Azure portal.
            CredentialBanner = AzureBaseService.GetStartupBanner();

            ReloadVisibleItems(kind);

            StatusMessage = kind switch
            {
                GroupingKind.Rating    => $"AI grouped {_sourceTickets.Count} tickets by rating into {GroupHeaders.Count} bucket(s).",
                GroupingKind.Priority  => $"AI grouped {_sourceTickets.Count} tickets by priority into {GroupHeaders.Count} level(s).",
                GroupingKind.Sentiment => $"AI grouped {_sourceTickets.Count} tickets into {GroupHeaders.Count} sentiment bucket(s).",
                GroupingKind.Month     => $"AI grouped {_sourceTickets.Count} tickets across {GroupHeaders.Count} month(s).",
                _                       => $"Showing {_sourceTickets.Count} tickets in a flat list (analyzed by {_analyzer.DisplayName}). Try \"group by feedbacks\", \"group by ratings\", \"group by priority\", or \"group by month\"."
            };
        }
        catch (IntentAnalysisException ex)
        {
            // Don't blow up the page if the AI call failed —
            // fall back to a flat list and surface the failure in
            // the status banner. The caller's tasks (bucketing)
            // can still proceed locally with the data already on
            // screen so the user gets a usable view.
            ReloadVisibleItems(GroupingKind.None);
            StatusMessage = $"AI grouping failed: {ex.Message} " +
                            $"[Active: {AzureBaseService.ActiveCredentialSummary}]";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Run the injected <see cref="IIntentAnalyzer"/> on the
    /// working ticket list. The analyzer mutates each
    /// <see cref="FeedbackItem.AssignedIntent"/>,
    /// <see cref="FeedbackItem.Confidence"/> and re-computes
    /// <see cref="FeedbackItem.Priority"/>.
    /// </summary>
    private async Task RunAnalyzerAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            await _analyzer.AnalyzeAsync(
                prompt ?? string.Empty,
                FeedbackDataset.Domain,
                _sourceTickets,
                cancellationToken).ConfigureAwait(true);
            AnalyzerLabel = _analyzer.DisplayName;
        }
        catch (IntentAnalysisException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Convert any unexpected analyzer failure into the
            // canonical IntentAnalysisException so the rest of
            // the view-model keeps a uniform catch site.
            throw new IntentAnalysisException(
                $"{_analyzer.DisplayName} call failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Build the visible Items collection for the requested
    /// grouping kind, assigning each item's
    /// <see cref="FeedbackItem.GroupKey"/> to a
    /// <see cref="GroupHeaderInfo"/> reference.
    /// </summary>
    private void ReloadVisibleItems(GroupingKind kind)
    {
        ResetLists();

        if (kind == GroupingKind.None)
        {
            // Flat list: every source ticket in display order,
            // with no group key.
            foreach (var item in _sourceTickets)
            {
                item.GroupKey = null;
                Items.Add(item);
            }
            CurrentGrouping = GroupingKind.None;
            return;
        }

        switch (kind)
        {
            case GroupingKind.Rating:
                BuildRatingGroups();
                break;
            case GroupingKind.Priority:
                BuildPriorityGroups();
                break;
            case GroupingKind.Sentiment:
                BuildSentimentGroups();
                break;
            case GroupingKind.Month:
                BuildMonthGroups();
                break;
        }

        // Flip CurrentGrouping exactly once at the end of the
        // rebuild so the page code-behind listens for a single
        // notification and refreshes the GroupDescriptor.
        CurrentGrouping = kind;
    }

    private void BuildRatingGroups()
    {
        // 5 Stars -> 1 Star (most positive first).
        var buckets = Enumerable.Range(1, 5)
            .Select(r => GroupHeaderInfo.FromBucket(
                title: GroupHeaderInfo.GetRatingBucketTitle(r),
                icon: r >= 4 ? "\uE735" : (r == 3 ? "\uE9CE" : "\uE783"),
                accentColor: r >= 4 ? "#0F9D58" : (r == 3 ? "#5F6368" : "#990000"),
                iconBackground: r >= 4 ? "#D5E8D4" : (r == 3 ? "#E8EAED" : "#F4CCCC"),
                summary: string.Empty,
                itemCount: 0))
            .ToDictionary(h => BuckedRatingFor(h.Title));

        foreach (var item in _sourceTickets)
        {
            int rating = Math.Clamp(item.Rating, 1, 5);
            var header = buckets[rating];
            item.GroupKey = header;
            Items.Add(item);
            header.ItemCount++;
        }

        foreach (var header in buckets.Values
                     .OrderByDescending(h => BuckedRatingFor(h.Title)))
        {
            if (header.ItemCount > 0)
            {
                header.SetSummaryText(
                    $"{header.ItemCount} {(header.ItemCount == 1 ? "ticket" : "tickets")} rated {BuckedRatingFor(header.Title)}.");
                GroupHeaders.Add(header);
            }
        }
    }

    private static int BuckedRatingFor(string title)
    {
        // "5 Stars" / "1 Star" -> int. Used so we can order buckets
        // from most positive (5) to least positive (1) without
        // touching the header string itself.
        foreach (var ch in title)
        {
            if (char.IsDigit(ch))
            {
                if (int.TryParse(ch.ToString(), out int rating))
                {
                    return rating;
                }
            }
        }
        return 0;
    }

    private void BuildPriorityGroups()
    {
        // High (>=4) / Medium (2-3) / Low (1), most urgent first.
        var high = GroupHeaderInfo.FromBucket("High", "\uE7BA", "#A50E0E", "#F4CCCC",
            "Tickets needing urgent attention.", 0);
        var medium = GroupHeaderInfo.FromBucket("Medium", "\uE7F0", "#B45F06", "#FCE5CD",
            "Tickets that are important but not blocking.", 0);
        var low = GroupHeaderInfo.FromBucket("Low", "\uE7F2", "#0F9D58", "#D5E8D4",
            "Tickets that can be addressed in good time.", 0);

        foreach (var item in _sourceTickets)
        {
            GroupHeaderInfo header = item.Priority >= 4 ? high
                : item.Priority >= 2 ? medium
                : low;
            item.GroupKey = header;
            Items.Add(item);
            header.ItemCount++;
        }

        if (high.ItemCount   > 0) GroupHeaders.Add(high);
        if (medium.ItemCount > 0) GroupHeaders.Add(medium);
        if (low.ItemCount    > 0) GroupHeaders.Add(low);
    }

    private void BuildSentimentGroups()
    {
        // Positive (rating >= 4) / Neutral (rating == 3) /
        // Negative (rating <= 2).
        var positive = GroupHeaderInfo.FromSentiment("Positive Feedbacks",
            "\uE735", "#0F9D58", "#D5E8D4", 0);
        var negative = GroupHeaderInfo.FromSentiment("Negative Feedbacks",
            "\uE783", "#990000", "#F4CCCC", 0);
        var neutral = GroupHeaderInfo.FromSentiment("Neutral Feedback",
            "\uE9CE", "#5F6368", "#E8EAED", 0);

        foreach (var item in _sourceTickets)
        {
            GroupHeaderInfo header = item.Rating >= 4 ? positive
                : item.Rating <= 2 ? negative
                : neutral;
            item.GroupKey = header;
            Items.Add(item);
            header.ItemCount++;
        }

        if (positive.ItemCount > 0) GroupHeaders.Add(positive);
        if (negative.ItemCount > 0) GroupHeaders.Add(negative);
        if (neutral.ItemCount  > 0) GroupHeaders.Add(neutral);
    }

    private void BuildMonthGroups()
    {
        var groups = new Dictionary<string, GroupHeaderInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in _sourceTickets)
        {
            var key = string.IsNullOrWhiteSpace(item.MonthYear) ? "Unknown" : item.MonthYear;
            if (!groups.TryGetValue(key, out var header))
            {
                header = GroupHeaderInfo.FromMonth(key, 0);
                groups[key] = header;
            }
            item.GroupKey = header;
            Items.Add(item);
            header.ItemCount++;
        }

        foreach (var header in groups.Values
                     .OrderByDescending(h => ParseMonthYearSortKey(h.Title)))
        {
            GroupHeaders.Add(header);
        }
    }

    private static int ParseMonthYearSortKey(string monthYear)
    {
        // "Mar 2024" -> 202403. Lets us show the newest month first
        // without depending on any DateTime.Parse on short strings.
        var parts = monthYear.Split(' ');
        if (parts.Length == 2 &&
            int.TryParse(parts[1], out int year))
        {
            int month = 0;
            for (int m = 1; m <= 12; m++)
            {
                if (CultureInfo.InvariantCulture.DateTimeFormat
                    .GetAbbreviatedMonthName(m)
                    .Equals(parts[0], StringComparison.OrdinalIgnoreCase))
                {
                    month = m;
                    break;
                }
            }
            return year * 100 + month;
        }
        return 0;
    }

    private void ResetLists()
    {
        Items.Clear();
        GroupHeaders.Clear();
    }

    /// <summary>
    /// Defensive copy of a <see cref="FeedbackItem"/>. The
    /// Grouping button may be clicked several times in a row;
    /// making fresh copies on every click keeps the source data
    /// pristine so later grouping passes start from a clean slate.
    /// </summary>
    private static FeedbackItem Clone(FeedbackItem src) => new()
    {
        Id = src.Id,
        TicketName = src.TicketName,
        SenderName = src.SenderName,
        MonthYear = src.MonthYear,
        Rating = src.Rating,
        Priority = src.Priority,
        ShortDescription = src.ShortDescription,
        Text = src.Text,
        Customer = src.Customer,
        Date = src.Date,
        Source = src.Source,
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}