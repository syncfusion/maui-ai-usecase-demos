using System.Text.RegularExpressions;

namespace SkyBook.Core;

public enum StageId { Search, Results, Booking }

/// <summary>Mirrors the STAGES / SAMPLES tables in App.tsx (copy is identical to React).</summary>
public sealed record Stage(
    StageId Id, int Step, string Icon, string Cta, string Title, string Tagline,
    string Copy, IReadOnlyList<string> Highlights, IReadOnlyList<string> Samples, string StarterPrompt)
{
    public string ShortLabel => Id switch { StageId.Booking => "Confirmation", StageId.Results => "Results", _ => "Search" };

    /// <summary>Stepper label used in the surface pane (App.tsx: title minus "Flights"/"Flight ").</summary>
    public string StepperLabel => Id == StageId.Booking ? "Confirmation" : Title.Replace("Flights", "").Replace("Flight ", "").Trim();

    public static readonly IReadOnlyList<Stage> All =
    [
        new(StageId.Search, 1, "🔎", "Search Flights", "Search Flights", "Find the best route",
            "Trip type, dates, cabin & travellers.",
            ["Round trip / One way / Multi-city", "Premium cabin filters"],
            ["New York → London", "Economy / Business", "Mar 12 – 18"],
            "Show me the flight search form."),
        new(StageId.Results, 2, "🛫", "View Results", "Flight Results", "Compare and choose",
            "Compare available flights by airline, departure time and pricing.",
            ["Sortable fare grid", "Select the best flight in seconds."],
            ["24 matching flights found", "Lowest fare from $549"],
            "Show me the flight results page."),
        new(StageId.Booking, 3, "🔒", "Review Booking", "Booking & Confirmation", "Confirm & get ticket",
            "Review trip & confirm.",
            ["Review traveler details and trip summary before booking.", "Confirmation message"],
            ["Passenger details verified", "Payment ready"],
            "Show me the booking and Confirmation page."),
    ];

    /// <summary>Same rules as App.tsx <c>currentStep</c>: stage3/book/confirm =&gt; 3, stage2/result =&gt; 2, else 1.</summary>
    public static int StepFromSurfaceId(string surfaceId)
    {
        var id = surfaceId.ToLowerInvariant();
        if (Regex.IsMatch(id, "stage[_-]?3|book|confirm")) return 3;
        if (Regex.IsMatch(id, "stage[_-]?2|result")) return 2;
        return 1;
    }

    /// <summary>
    /// Stepper position. React derives it from the surface id only, but every bundled design uses the id "workspace",
    /// so the stepper would never advance; fall back to the component ids the design contains.
    /// </summary>
    public static int InferStep(string surfaceId, IEnumerable<string> componentIds)
    {
        var fromId = StepFromSurfaceId(surfaceId);
        if (fromId != 1) return fromId;
        var ids = componentIds.ToHashSet();
        if (ids.Contains("passengerGrid")) return 3;
        if (ids.Contains("flightsGrid")) return 2;
        return 1;
    }
}
