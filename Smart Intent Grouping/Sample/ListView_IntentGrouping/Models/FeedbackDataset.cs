using System.Collections.ObjectModel;
using System.Globalization;

namespace ListView_IntentGrouping.Models;

/// <summary>
/// A single dataset description. With the new ticket-based UI
/// there is only one dataset (the unified ticket list), but we
/// keep the lightweight wrapper so the SfListView / header code
/// can keep treating every input as a dataset.
/// </summary>
public sealed class DatasetDefinition
{
    public DatasetDefinition(string domain,
                             string sourceLabel,
                             string emoji,
                             IReadOnlyList<FeedbackItem> items,
                             string? groupTitle = null,
                             string? groupIcon = null)
    {
        Domain = domain;
        SourceLabel = sourceLabel;
        Emoji = emoji;
        Items = items;
        GroupTitle = groupTitle ?? domain;
        GroupIcon = groupIcon ?? emoji;
    }

    public string Domain { get; }
    public string SourceLabel { get; }
    public string Emoji { get; }
    public string GroupTitle { get; }
    public string GroupIcon { get; }
    public IReadOnlyList<FeedbackItem> Items { get; }
}

/// <summary>
/// The single combined ticket list backing the user-driven grouping
/// feature. Every ticket carries rating, priority, ticket name,
/// sender name, month/year and a short description, which is exactly
/// the surface the new ticket card expects.
/// </summary>
public static class FeedbackDataset
{
    /// <summary>
    /// Display title shown on the header above the list.
    /// </summary>
    public const string Domain = "Support Tickets";

    /// <summary>
    /// The unified ticket list. Shown flat below the input field
    /// and grouped dynamically based on the prompt the user types
    /// (e.g. "group by ratings", "group by priority",
    /// "Positive Feedbacks", "Negative Feedbacks",
    /// "group by month").
    /// </summary>
    public static readonly IReadOnlyList<FeedbackItem> Tickets = BuildTickets();

    private static IReadOnlyList<FeedbackItem> BuildTickets()
    {
        // Curated 10-ticket dataset that intentionally spans every
        // priority bucket (High / Medium / Low), every rating
        // value (1..5), every sentiment (Positive / Negative /
        // Neutral) and three months (Mar / Apr / May) so the
        // user-driven grouping (rating / priority / sentiment /
        // month) always has at least one row in each category.
        var seeds = new (string Id, string Name, string Sender, string Month, int Rating, int Priority, string Text)[]
        {
            // 5 Stars -> Positive // Low priority    // Mar
            ("T-2002", "Gorgeous New Dark Theme",              "Liam O'Connor",   "Mar 2024", 5, 1,
                "The new dark theme is gorgeous. The animations feel fluid and the design language is consistent across the whole experience. Loving it."),

            // 4 Stars -> Positive // Medium priority // Apr
            ("T-3001", "Bulk Export to CSV",                   "Priya Shah",      "Apr 2024", 4, 3,
                "Could you please add a bulk export to CSV for the analytics view? Right now we have to copy rows one at a time."),

            // 3 Stars -> Neutral  // Medium priority // May
            ("T-4004", "Pricing Page Needs Tier Comparison",   "Aisha Bello",     "May 2024", 3, 3,
                "Could you add a side-by-side tier comparison on the pricing page? It is hard to figure out what is included at each level."),

            // 2 Stars -> Negative // High priority    // Mar
            ("T-2005", "Double Charge on Monthly Invoice",     "Sophia Nguyen",   "Mar 2024", 2, 5,
                "I was double charged on my credit card for the monthly subscription. The invoice shows two identical payments. Please refund the duplicate charge as soon as possible."),

            // 1 Star  -> Negative // High priority    // Apr
            ("T-3002", "Invoice Formatting Glitch",            "Diego Hernandez", "Apr 2024", 1, 5,
                "The invoice PDF renders the line items in the wrong order. This is breaking our reconciliation workflow with finance."),

            // 5 Stars -> Positive // Low priority     // May
            ("T-4002", "Loving the New Dashboard",             "Mia Johnson",     "May 2024", 5, 1,
                "Absolutely loving the new dashboard layout. The KPI tiles are exactly what we needed for our weekly review."),

            // 1 Star  -> Negative // High priority    // Mar
            ("T-2003", "Reports Load Too Slowly",              "Emma Garcia",     "Mar 2024", 1, 4,
                "Performance is terrible. Reports take 30+ seconds to load, the UI freezes when I scroll, and syncing never completes. I have a 1 Gbps connection."),

            // 4 Stars -> Positive // Medium priority // May
            ("T-4005", "Beta Channel Auto-Upgrade",            "Tomás Álvarez",   "May 2024", 4, 3,
                "I was auto-upgraded from beta to the paid tier without confirmation. Please require explicit consent."),

            // 2 Stars -> Negative // Medium priority // Apr
            ("T-3005", "Role Permissions Confusing",           "Sara Khan",       "Apr 2024", 2, 3,
                "The new role permissions matrix is confusing. We had to grant admin just to share a dashboard view."),

            // 5 Stars -> Positive // Low priority     // Apr
            ("T-3003", "Beautiful Onboarding Redesign",        "Chloe Dubois",    "Apr 2024", 5, 1,
                "The new onboarding screens look gorgeous and the copy is on-brand. Best change you have made this quarter."),
        };

        var list = new List<FeedbackItem>(seeds.Length);
        foreach (var s in seeds)
        {
            string shortDate = s.Month;
            // Derive the short legacy date label from the MonthYear
            // string ("Mar 2024" -> "Mar 04") for the footer chip.
            var parts = s.Month.Split(' ');
            if (parts.Length == 2 && parts[1].Length >= 2)
            {
                shortDate = $"{parts[0]} {parts[1].Substring(parts[1].Length - 2)}";
            }

            list.Add(new FeedbackItem
            {
                Id = s.Id,
                TicketName = s.Name,
                SenderName = s.Sender,
                MonthYear = s.Month,
                Rating = s.Rating,
                Priority = s.Priority,
                ShortDescription = s.Text,
                Text = s.Text,
                Customer = s.Sender,
                Date = shortDate,
                Source = "Support Ticket",
            });
        }
        return new ReadOnlyCollection<FeedbackItem>(list);
    }

    /// <summary>
    /// Default dataset. Backwards-compatible shim: wraps the
    /// unified ticket list in a <see cref="DatasetDefinition"/>.
    /// </summary>
    public static DatasetDefinition DefaultDataset { get; } =
        new(Domain, "Support Ticket", "🎫", Tickets,
            groupTitle: "Support Tickets", groupIcon: "🎫");

    /// <summary>
    /// Convenience: returns the dataset for a given key, falling
    /// back to <see cref="DefaultDataset"/> when no match is found.
    /// </summary>
    public static DatasetDefinition GetDataset(string? key)
    {
        if (!string.IsNullOrWhiteSpace(key) &&
            string.Equals(key, Domain, StringComparison.OrdinalIgnoreCase))
        {
            return DefaultDataset;
        }
        return DefaultDataset;
    }
}
