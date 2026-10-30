using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using TravelPlanner.Models;
using TravelPlanner.Services;
using Syncfusion.Maui.AIAssistView;
using Syncfusion.Maui.Core;

namespace TravelPlanner.ViewModels;

public class TravelPlannerViewModel : INotifyPropertyChanged
{
    private readonly OpenAIServices _aiService;
    private AssistItem? _interestsPromptItem;

    // Controls visibility of the interests popup (bound from XAML).
    // The setter only raises PropertyChanged when the value actually changes so that
    // duplicate assignments (e.g. suggestion tap + AssistView SuggestionItemSelected)
    // do not retrigger the popup's open animation.
    public bool IsInterestsPopupVisible
    {
        get => _isInterestsPopupVisible;
        set
        {
            if (_isInterestsPopupVisible == value)
                return;
            _isInterestsPopupVisible = value;
            OnPropertyChanged();
        }
    }
    private bool _isInterestsPopupVisible;

    // Remove the latest matching user request item to avoid clutter during multi-select flows
    void RemoveLatestUserRequest(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        // Find the most recent AssistItem that matches (ignore case/whitespace)
        var target = AssistItems
            .OfType<AssistItem>()
            .Reverse()
            .FirstOrDefault(ai => ai.IsRequested && string.Equals(ai.Text?.Trim(), text.Trim(), StringComparison.OrdinalIgnoreCase));

        if (target == null) return;

        // Remove on main thread immediately to avoid visible flicker
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (AssistItems.Contains(target))
                AssistItems.Remove(target);
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    // Conversation
    public ObservableCollection<IAssistItem> AssistItems { get; } = new();
    public ConversationStep Step { get => _step; set { _step = value; OnPropertyChanged(); } }
    private ConversationStep _step = ConversationStep.None;

    // Inputs
    public string Destination { get => _destination; set { _destination = value; OnPropertyChanged(); } }
    private string _destination = string.Empty;

    public DateTime? StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }
    private DateTime? _startDate;

    public DateTime? EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }
    private DateTime? _endDate;

    public string TripType { get => _tripType; set { _tripType = value; OnPropertyChanged(); } }
    private string _tripType = string.Empty;

    public ObservableCollection<string> Interests { get; } = new();
    
    // Interest options for SfChipGroup
    public ObservableCollection<string> InterestOptions { get; } = new()
    {
        "🍴 Food", " 🗺️ Hidden Places", " 🧗 Adventure", " 🛍️ Shopping", " 🏛️ Culture", " 🏖️ Beaches",
        " 🏔️ Mountains", " 🍸 Nightlife", " 🏰 History", " 🛋️ Relaxation", " 🎨 Art", " 🌿 Nature", " 📷 Photography"
    };
    
    // Selected interests for SfChipGroup
    public ObservableCollection<string> SelectedInterests { get; } = new();
    
    // Error message shown on the interests popup when validation fails
    public string InterestsErrorMessage { get => _interestsErrorMessage; set { _interestsErrorMessage = value; OnPropertyChanged(); } }
    private string _interestsErrorMessage = string.Empty;
    // Suggestions shown in SfAIAssistView
    public ObservableCollection<ISuggestion> Suggestions { get; } = new();

    // Profile for SfAIAssistView (bind from XAML)
    public Profile AssistantProfile { get; }

    // Outputs
    public ObservableCollection<PlaceCard> TopPlaces { get; } = new();
    public ObservableCollection<PlaceCard> Hotels { get; } = new();
    public ObservableCollection<PlaceCard> FoodSpots { get; } = new();
    public string DestinationSummary { get => _destinationSummary; set { _destinationSummary = value; OnPropertyChanged(); } }
    private string _destinationSummary = string.Empty;

    public string ConsolidatedDetails { get => _consolidatedDetails; set { _consolidatedDetails = value; OnPropertyChanged(); } }
    private string _consolidatedDetails = string.Empty;

    public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
    private bool _isBusy;

    // Date range staged by the SfCalendar popup; committed to StartDate/EndDate only on OK.
    public DateTime? PendingStartDate { get => _pendingStartDate; set { _pendingStartDate = value; OnPropertyChanged(); } }
    private DateTime? _pendingStartDate;

    public DateTime? PendingEndDate { get => _pendingEndDate; set { _pendingEndDate = value; OnPropertyChanged(); } }
    private DateTime? _pendingEndDate;

    public bool HasPendingDateRange => PendingStartDate.HasValue && PendingEndDate.HasValue;

    // Random generator for fallback sample data
    private static readonly System.Random _rng = new System.Random();
    public ICommand StartConversationCommand { get; }
    public ICommand HandleUserInputCommand { get; }
    public ICommand RestartTripCommand { get; }
    public ICommand CommitDateRangeCommand { get; }
    public ICommand CancelDateRangeCommand { get; }
    public ICommand ConfirmInterestsCommand { get; }
    public ICommand CancelInterestsCommand { get; }

    // Raised after the trip is reset so views can clear view-only state (calendar selection, map pins).
    public event EventHandler? TripRestarted;

    // Raised when a new date range should be reflected in the conversation.
    public event EventHandler<DateTimeRangeCommitted>? DateRangeCommitted;

    public TravelPlannerViewModel(OpenAIServices aiService)
    {
        _aiService = aiService;

        // Set only the response avatar via profile (image from Resources/Images)
        AssistantProfile = new Profile();

        StartConversationCommand = new Command(() =>
        {
            AssistItems.Clear();
            Step = ConversationStep.Destination;
            AddBotText("Which place do you want to plan a trip to?");
        });

        HandleUserInputCommand = new Command<string>(async text => await HandleUserInputAsync(text));

        RestartTripCommand = new Command(RestartTrip);

        CommitDateRangeCommand = new Command(() =>
        {
            if (!PendingStartDate.HasValue || !PendingEndDate.HasValue)
                return;

            var sText = PendingStartDate.Value.ToString("MM dd, yyyy");
            var eText = PendingEndDate.Value.ToString("MM dd, yyyy");

            StartDate = PendingStartDate.Value;
            EndDate = PendingEndDate.Value;

            // Advance the state machine to TripType so the guided flow continues.
            AddUserText($"Schedule a trip between from {sText} \n to {eText}");
            Step = ConversationStep.TripType;
            AddBotTextWithSuggestions(
                "What kind of trip is this? Pick a suggestion below.",
                new[] { "Couple", "Solo", "Friends", "Family" }
            );

            ClearPendingDateRange();
            DateRangeCommitted?.Invoke(this, new DateTimeRangeCommitted(StartDate, EndDate));
        });

        CancelDateRangeCommand = new Command(() => ClearPendingDateRange());

        ConfirmInterestsCommand = new Command(async () => await ConfirmInterestsAsync());

        CancelInterestsCommand = new Command(() =>
        {
            SelectedInterests.Clear();
            InterestsErrorMessage = string.Empty;
            IsInterestsPopupVisible = false;
        });
    }

    /// <summary>
    /// Stages the range coming out of the SfCalendar popup. Invalid or incomplete
    /// ranges clear the pending state so the OK button stays inert.
    /// </summary>
    public void StageDateRange(DateTime? start, DateTime? end)
    {
        if (!start.HasValue || !end.HasValue)
        {
            ClearPendingDateRange();
            return;
        }

        var min = DateTime.Today.AddDays(1);
        if (start.Value.Date < min || end.Value.Date < min || end.Value.Date <= start.Value.Date)
        {
            ClearPendingDateRange();
            return;
        }

        PendingStartDate = start.Value.Date;
        PendingEndDate = end.Value.Date;
        OnPropertyChanged(nameof(HasPendingDateRange));
    }

    void ClearPendingDateRange()
    {
        PendingStartDate = null;
        PendingEndDate = null;
        OnPropertyChanged(nameof(HasPendingDateRange));
    }

    /// <summary>
    /// Resets every piece of conversation and trip state so a new plan can be started.
    /// </summary>
    public void RestartTrip()
    {
        AssistItems.Clear();
        TopPlaces.Clear();
        Hotels.Clear();
        FoodSpots.Clear();
        Suggestions.Clear();
        Destination = string.Empty;
        StartDate = null;
        EndDate = null;
        TripType = string.Empty;
        Interests.Clear();
        SelectedInterests.Clear();
        DestinationSummary = string.Empty;
        ConsolidatedDetails = string.Empty;
        InterestsErrorMessage = string.Empty;
        IsInterestsPopupVisible = false;
        ClearPendingDateRange();

        Step = ConversationStep.Destination;
        AddBotText("Which place do you want to plan a trip to?");

        TripRestarted?.Invoke(this, EventArgs.Empty);
    }

    async Task ConfirmInterestsAsync()
    {
        var texts = SelectedInterests.ToList();

        if (texts.Count == 0)
        {
            InterestsErrorMessage = "Please select at least one interest.";
            IsInterestsPopupVisible = true;
            return;
        }

        IsInterestsPopupVisible = false;

        var input = string.Join(", ", texts);

        AddUserText(input);
        await HandleUserInputAsync(input);

        // The selection is complete; advance to build.
        AddUserText("Done");
        await HandleUserInputAsync("Done");

        InterestsErrorMessage = string.Empty;
        SelectedInterests.Clear();
    }

    public async Task HandleUserInputAsync(string? userText)
    {
        if (string.IsNullOrWhiteSpace(userText)) return;
        // NOTE: The AIAssistView already renders the user's message in AssistItems.
        // Avoid adding a duplicate user entry here.

        switch (Step)
        {
            case ConversationStep.Destination:
                Destination = userText.Trim();
                Step = ConversationStep.Dates;
                Suggestions.Clear();
                AddBotText("Please pick your dates using the calendar popup. Start date must be after today.");
                break;
            case ConversationStep.Dates:
                if (!StartDate.HasValue)
                {
                    if (TryParseFutureDate(userText, out var s)) { StartDate = s; }
                }
                else if (!EndDate.HasValue)
                {
                    if (TryParseFutureDate(userText, out var e) && e > StartDate)
                    {
                        EndDate = e;
                        Step = ConversationStep.TripType;

                        // Show trip-type choices as suggestions attached to this bot response
                        AddBotTextWithSuggestions(
                            "What kind of trip is this? Pick a suggestion below.",
                            new[] { "Couple", "Solo", "Friends", "Family" }
                        );
                    }
                    else { AddBotText("Invalid end date. It must be after the start date."); }
                }
                break;
            case ConversationStep.TripType:
                TripType = userText.Trim();
                Step = ConversationStep.Interests;
                Suggestions.Clear();
                // Use a bot prompt rendered as an assist message with an in-item suggestion
                // so the user can open the interests picker from the conversation.
                IsInterestsPopupVisible = false;
                _interestsPromptItem = AddBotTextWithSuggestions(
                    "Select your interests. Tap 'Select Interests' to open the picker.",
                    new[] { "Select Interests" }
                );
                break;
            case ConversationStep.Interests:
                {
                    var text = userText.Trim();

                    // If the user tapped the in-conversation suggestion to open the picker,
                    // show the interests popup instead of treating it as a selection.
                    if (string.Equals(text, "Select Interests", StringComparison.OrdinalIgnoreCase))
                    {
                        RemoveLatestUserRequest(text);
                        IsInterestsPopupVisible = true;
                        break;
                    }

                    // Remove the just-added user request bubble for interim selections
                    if (!string.Equals(text, "Done", StringComparison.OrdinalIgnoreCase))
                        RemoveLatestUserRequest(text);

                    // If user taps Done and at least one interest selected, proceed to build
                    if (string.Equals(text, "Done", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Interests.Count == 0)
                        {
                            AddBotText("Please select at least one interest, then tap Done.");
                            break;
                        }

                        // Replace the auto-added "Done" user bubble with a user entry showing the selected interests
                        var joined = Interests.Count > 0 ? string.Join(", ", Interests) : "";
                        RemoveLatestUserRequest("Done");
                        if (!string.IsNullOrWhiteSpace(joined))
                        {
                            AddUserText(joined);
                        }

                        Step = ConversationStep.Building;
                        await BuildTripAsync();
                        Step = ConversationStep.Done;
                        Suggestions.Clear();
                        _interestsPromptItem = null;
                        AddBotText("Trip plan generated. Open the drawer to view consolidated details.");
                        break;
                    }

                    // Otherwise, accumulate selected interests (support comma-separated input too)
                    foreach (var i in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        if (!string.IsNullOrWhiteSpace(i) && !Interests.Contains(i))
                            Interests.Add(i);
                    }

                    // No in-item suggestions to update; the popup manages selections.
                }
                break;
        }
    }

    static bool TryParseFutureDate(string text, out DateTime? date)
    {
        if (DateTime.TryParse(text, out var d) && d > DateTime.Today) { date = d; return true; }
        date = null; return false;
    }

    void AddBotText(string text) => AssistItems.Add(new AssistItem
    {
        Text = text,
        IsRequested = false,
    });
    void AddUserText(string text) => AssistItems.Add(new AssistItem { Text = text, IsRequested = true });

    // Adds a bot message with in-item suggestions and returns the created item so callers can update it later
    AssistItem AddBotTextWithSuggestions(string text, IEnumerable<string> suggestions)
    {
        var item = new AssistItem
        {
            Text = text,
            IsRequested = false,
            Suggestion = new AssistItemSuggestion
            {
                Items = new List<ISuggestion>(
                    suggestions.Select(s => new AssistSuggestion() { Text = s })
                )
            }
        };
        AssistItems.Add(item);
        // Populate the control-level Suggestions collection as well so
        // the SfAIAssistView.SuggestionTemplate is used for visualizing
        // the available choices (the view binds to `Suggestions`).
        Suggestions.Clear();
        foreach (var s in suggestions)
        {
            Suggestions.Add(new AssistSuggestion { Text = s });
        }
        return item;
    }

    async Task BuildTripAsync()
    {
        IsBusy = true;
        try
        {
            var input = new TripInput
            {
                Destination = Destination,
                StartDate = StartDate,
                EndDate = EndDate,
                TripType = TripType,
                Interests = Interests.ToList()
            };

            var rec = await _aiService.GetTripRecommendationsAsync(input);

            // Defensive normalization to guarantee UI text presence
            async Task NormalizeList(List<PlaceCard>? list, string fallback)
            {
                if (list == null) return;
                foreach (var it in list)
                {
                    if (it == null) continue;
                    if (string.IsNullOrWhiteSpace(it.Name)) it.Name = fallback;
                    if (string.IsNullOrWhiteSpace(it.Address)) it.Address = "Address not available";
                    if (!await IsValidLink(it.ImageUrl)) it.ImageUrl = "image_not_available.png";
                    if (it.Rating <= 0) it.Rating = 4.2;
                }
            }

            // Helper to generate fallback sample data when API returns none
            List<PlaceCard> GenerateFallbackPlaces(string kind, int count)
            {
                var list = new List<PlaceCard>();
                for (int i = 1; i <= count; i++)
                {
                    list.Add(new PlaceCard
                    {
                        Name = $"{kind} {i} in {(!string.IsNullOrWhiteSpace(Destination) ? Destination : "Destination")}",
                        Address = $"{i} {kind} Street",
                        ImageUrl = "image_not_available.png",
                        Rating = Math.Round(3.5 + _rng.NextDouble() * 1.5, 1)
                    });
                }
                return list;
            }

            // Use API data when present, otherwise generate fallback samples
            var topPlaces = (rec?.TopPlaces != null && rec.TopPlaces.Count > 0) ? rec.TopPlaces : GenerateFallbackPlaces("Attraction", 5);
            var hotels = (rec?.Hotels != null && rec.Hotels.Count > 0) ? rec.Hotels : GenerateFallbackPlaces("Hotel", 4);
            var foods = (rec?.FoodSpots != null && rec.FoodSpots.Count > 0) ? rec.FoodSpots : GenerateFallbackPlaces("Food Spot", 5);

            await NormalizeList(topPlaces, "Attraction");
            await NormalizeList(hotels, "Hotel");
            await NormalizeList(foods, "Food Spot");

            DestinationSummary = string.IsNullOrWhiteSpace(rec?.DestinationSummary)
                ? $"Trip to {Destination}"
                : rec.DestinationSummary;

            ReplaceItems(TopPlaces, topPlaces ?? new());
            ReplaceItems(Hotels, hotels ?? new());
            ReplaceItems(FoodSpots, foods ?? new());

            ConsolidatedDetails = $"Destination: {Destination}\nDates: {StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd}\nTrip Type: {TripType}\nInterests: {string.Join(", ", Interests)}\n\nTop Places: {string.Join(", ", TopPlaces.Select(p => p.Name))}\nHotels: {string.Join(", ", Hotels.Select(h => h.Name))}\nFood: {string.Join(", ", FoodSpots.Select(f => f.Name))}";
        }
        catch (Exception ex)
        {
            AddBotText($"Something went wrong: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> IsValidLink(string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    static void ReplaceItems<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source) target.Add(item);
    }
}

public enum ConversationStep { None, Destination, Dates, TripType, Interests, Building, Done }

public sealed class DateTimeRangeCommitted : EventArgs
{
    public DateTimeRangeCommitted(DateTime? start, DateTime? end)
    {
        Start = start;
        End = end;
    }

    public DateTime? Start { get; }
    public DateTime? End { get; }
}