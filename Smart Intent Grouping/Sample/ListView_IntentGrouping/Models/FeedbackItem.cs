namespace ListView_IntentGrouping.Models;

/// <summary>
/// Represents a single feedback entry rendered as a ticket card and
/// grouped by the user-driven grouping prompts (ratings, priority,
/// positive / negative feedback, month, ...).
///
/// <para>
/// The model preserves backwards-compatible aliases for the legacy
/// AI-intent property names (<see cref="Text"/>, <see cref="Source"/>,
/// <see cref="Customer"/>, <see cref="Rating"/>...) while also
/// exposing the ticket-style fields the new UI surfaces:
/// <see cref="TicketName"/>, <see cref="SenderName"/>,
/// <see cref="MonthYear"/>, <see cref="Priority"/>,
/// <see cref="ShortDescription"/>.
/// </para>
/// </summary>
public class FeedbackItem
{
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Short, friendly title of the ticket (e.g. "App Crash on
    /// Dashboard"). Surfaced as the ticket name on the card.
    /// </summary>
    public string TicketName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the person / customer who raised the ticket.
    /// Surfaced as the sender name on the card and reused for
    /// the avatar initials and the legacy <see cref="Customer"/>
    /// alias.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Pre-formatted "MMM yyyy" string used by the ticket card.
    /// The legacy <see cref="Date"/> field keeps a short version
    /// (e.g. "Mar 05") so existing UI bits still render.
    /// </summary>
    public string MonthYear { get; set; } = string.Empty;

    /// <summary>
    /// Rating between 1 and 5 used both for the star pill on the
    /// card and as the key for "group by ratings" requests.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// 1-5 priority bucket used for "group by priority" requests
    /// (mapped to Low / Medium / High via
    /// <c>PriorityToLabelConverter</c>).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// One-paragraph summary of the ticket. Surfaced as the
    /// "Short Description" on the card. Backed by the longer
    /// <see cref="Text"/> field for the AI intent analyzer.
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Legacy body text field used by the AI intent analyzer.
    /// Kept in sync with <see cref="ShortDescription"/>.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Legacy "source" label (e.g. "Support Ticket"). Surfaced
    /// as the type chip on the card and kept for backwards-
    /// compatible grouping metadata.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Legacy customer / sender label. Mirrors
    /// <see cref="SenderName"/> for backwards-compatible
    /// grouping metadata.
    /// </summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// Legacy short date label (e.g. "Mar 04"). Mirrors
    /// <see cref="MonthYear"/> so existing footer chips still
    /// render.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// The intent label assigned by the AI analyzer
    /// (e.g. "Billing", "Performance").
    /// </summary>
    public string AssignedIntent { get; set; } = string.Empty;

    /// <summary>
    /// The header record this item is grouped under (ratings
    /// bucket, priority level, month, intent category, ...).
    /// Exposed as <c>object</c> so the SfListView can group by
    /// any reference without the page code-behind needing to
    /// know which kind of grouping is active.
    /// </summary>
    public object? GroupKey { get; set; }

    /// <summary>
    /// Confidence score in the range [0, 1] for any AI-assigned
    /// grouping.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Two-letter initials used for the avatar circle.
    /// </summary>
    public string Initials
    {
        get
        {
            var name = string.IsNullOrWhiteSpace(SenderName) ? Customer : SenderName;
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant();
        }
    }
}
