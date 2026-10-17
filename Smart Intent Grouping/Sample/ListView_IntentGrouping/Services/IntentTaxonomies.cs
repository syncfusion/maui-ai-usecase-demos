using System.Collections.ObjectModel;
using ListView_IntentGrouping.Models;

namespace ListView_IntentGrouping.Services;

/// <summary>
/// A single intent the AI can detect. Each intent has a
/// curated keyword list that the analyzer scores against, plus a
/// display name, icon, accent colour, and a summarization template.
/// </summary>
public class IntentDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Keywords { get; init; } = Array.Empty<string>();
    public string Icon { get; init; } = "\uE73E";
    public string AccentColor { get; init; } = "#6750A4";
    public string IconBackground { get; init; } = "#EADDFF";
    public string SummaryTemplate { get; init; } = string.Empty;
}

/// <summary>
/// Result of an intent analysis pass: a list of intent categories
/// with the original items placed into the most appropriate group.
/// </summary>
public class IntentAnalysisResult
{
    public string DetectedDomain { get; set; } = "Customer Feedback";
    public int TotalItems { get; set; }
    public int UnclassifiedCount { get; set; }
    public List<IntentCategory> Categories { get; set; } = new();
    public List<string> TopThemes { get; set; } = new();
    public string OverallSummary { get; set; } = string.Empty;
}

/// <summary>
/// Common English stop words used when extracting per-category
/// keyword themes. Shared between the local rule-based analyzer and
/// the Azure OpenAI analyzer so summaries are consistent regardless
/// of which path is active.
/// </summary>
public static class IntentTaxonomies
{
    // ------------------------------------------------------------------
    // Customer Feedback taxonomy (default; richest taxonomy)
    // ------------------------------------------------------------------
    public static readonly IReadOnlyList<IntentDefinition> CustomerFeedback = new ReadOnlyCollection<IntentDefinition>(
        new List<IntentDefinition>
        {
            new IntentDefinition
            {
                Name = "User Experience",
                Description = "UI, navigation, design, onboarding, and accessibility.",
                Keywords = new[] {
                    "design", "ui", "ux", "user interface", "user experience", "navigation", "menu",
                    "onboarding", "tour", "empty state", "tooltip", "dark mode", "dark theme",
                    "accessibility", "voice over", "screen reader", "redesign", "settings page",
                    "look and feel", "delightful", "beautiful", "gorgeous", "modern",
                    "navigation", "delightful", "wonderful", "inconsistent"
                },
                Icon = "\uE7F4", AccentColor = "#134F5C", IconBackground = "#A4C2F4",
                SummaryTemplate = "{count} items discuss UI, navigation, or onboarding. Recurring feedback: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Billing",
                Description = "Payments, invoices, refunds, pricing, and subscription changes.",
                Keywords = new[] {
                    "charge", "charged", "payment", "invoice", "bill", "billing", "refund", "subscription",
                    "plan", "price", "pricing", "tier", "discount", "quote", "receipt", "overcharge",
                    "double charge", "credit card", "renewal", "auto-upgrade", "downgrade", "student discount"
                },
                Icon = "\uE8C7", AccentColor = "#0B5394", IconBackground = "#CFE2F3",
                SummaryTemplate = "{count} items mention payments, invoices, refunds, or pricing. Common concerns: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Performance",
                Description = "App speed, crashes, freezes, lag, and resource usage.",
                Keywords = new[] {
                    "slow", "lag", "laggy", "freeze", "freezes", "crash", "crashes", "crashed",
                    "performance", "speed", "fast", "loading", "load", "hang", "stuck", "battery",
                    "memory", "cpu", "drain", "optimize", "optimisation", "optimization", "responsive",
                    "unresponsive", "10 seconds", "30 seconds", "sync never", "drains", "flicker"
                },
                Icon = "\uE945", AccentColor = "#B45F06", IconBackground = "#FCE5CD",
                SummaryTemplate = "{count} items describe speed, crashes, or stability issues. Recurring themes: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Account Management",
                Description = "Login, password, SSO, 2FA, OAuth, and account state.",
                Keywords = new[] {
                    "login", "log in", "sign in", "signin", "logout", "log out", "password", "reset",
                    "2fa", "two factor", "two-factor", "authenticator", "oauth", "google account",
                    "sso", "saml", "account", "suspended", "locked", "session", "timeout",
                    "reset link", "expired"
                },
                Icon = "\uE77B", AccentColor = "#38761D", IconBackground = "#D9EAD3",
                SummaryTemplate = "{count} items relate to login, authentication, or account state. Key topics: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Feature Requests",
                Description = "Suggestions, improvements, and roadmap ideas.",
                Keywords = new[] {
                    "would love", "would be great", "could you", "please add", "feature request",
                    "roadmap", "wish", "should add", "missing feature", "suggestion", "suggest",
                    "upvote", "public roadmap", "would like to", "it would", "could you add",
                    "csv export", "pdf", "export"
                },
                Icon = "\uE710", AccentColor = "#351C75", IconBackground = "#E1D5E7",
                SummaryTemplate = "{count} items contain new feature ideas or roadmap requests. Frequently requested: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Product Quality",
                Description = "Bugs, regressions, data loss, and quality regressions.",
                Keywords = new[] {
                    "bug", "bugs", "regression", "regressions", "broken", "data loss", "lost data",
                    "issue", "flicker", "inconsistent", "quality has dropped", "small bugs", "out of date",
                    "does not work", "doesn't work", "doesnt work", "stopped working", "stopped working after"
                },
                Icon = "\uE783", AccentColor = "#990000", IconBackground = "#F4CCCC",
                SummaryTemplate = "{count} items report bugs or quality regressions. Notable issues: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Documentation",
                Description = "Docs, guides, tutorials, API references, and help content.",
                Keywords = new[] {
                    "documentation", "docs", "guide", "tutorial", "help center", "api example",
                    "api examples", "changelog", "release notes", "out of date", "outdated",
                    "article", "faq", "out of the documentation", "cannot find"
                },
                Icon = "\uE82D", AccentColor = "#0F4C81", IconBackground = "#C9DAF8",
                SummaryTemplate = "{count} items mention documentation or help content. Key issues: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Support",
                Description = "Customer support experience, response time, and bots.",
                Keywords = new[] {
                    "support", "support team", "customer service", "service", "agent", "responded",
                    "response time", "responsive", "unhelpful", "useless bot", "chat bot", "chatbot",
                    "reach a human", "no one replies", "help desk", "ticket", "no way to"
                },
                Icon = "\uE8FE", AccentColor = "#783F04", IconBackground = "#FCE4D6",
                SummaryTemplate = "{count} items discuss customer support or help-desk interactions. Themes: {keywords}."
            },
            new IntentDefinition
            {
                Name = "General Feedback",
                Description = "General praise, thanks, and other comments.",
                Keywords = new[] {
                    "love", "loved", "loving", "great", "amazing", "awesome", "excellent", "fantastic",
                    "thank you", "thanks", "appreciate", "good job", "nice work", "happy", "delighted",
                    "best", "wonderful", "impressed"
                },
                Icon = "\uE735", AccentColor = "#0F9D58", IconBackground = "#D5E8D4",
                SummaryTemplate = "{count} items are general praise, thanks, or positive feedback. Highlights: {keywords}."
            },
        });

    // ------------------------------------------------------------------
    // Support Tickets taxonomy
    // ------------------------------------------------------------------
    public static readonly IReadOnlyList<IntentDefinition> SupportTickets = new ReadOnlyCollection<IntentDefinition>(
        new List<IntentDefinition>
        {
            new IntentDefinition
            {
                Name = "Login Issues",
                Description = "Password resets, MFA, OAuth, SSO, and account lockouts.",
                Keywords = new[] {
                    "login", "log in", "sign in", "signin", "password", "reset", "reset link",
                    "expired", "2fa", "two factor", "two-factor", "authenticator", "oauth",
                    "google account", "sso", "saml", "locked", "session", "timeout"
                },
                Icon = "\uE77B", AccentColor = "#38761D", IconBackground = "#D9EAD3",
                SummaryTemplate = "{count} support tickets involve login, authentication, or account state. Topics: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Payment Issues",
                Description = "Charges, refunds, invoices, subscription billing problems.",
                Keywords = new[] {
                    "charge", "charged", "double charge", "payment", "invoice", "bill", "billing",
                    "refund", "credit card", "overcharge", "fraud", "auto-upgrade", "downgrade",
                    "subscription", "renewal", "receipt", "honor the quote", "discount", "student discount"
                },
                Icon = "\uE8C7", AccentColor = "#0B5394", IconBackground = "#CFE2F3",
                SummaryTemplate = "{count} support tickets report payment, billing, or subscription issues. Recurring terms: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Technical Issues",
                Description = "Crashes, slowdowns, mobile bugs, push notifications, and outages.",
                Keywords = new[] {
                    "crash", "crashes", "crashed", "slow", "lag", "laggy", "freeze", "freezes",
                    "hang", "stuck", "performance", "speed", "loading", "load", "battery",
                    "memory", "cpu", "drain", "push notification", "push notifications", "stopped working",
                    "broken", "bug", "regression", "doesn't work", "does not work", "unsubscribe"
                },
                Icon = "\uE945", AccentColor = "#B45F06", IconBackground = "#FCE5CD",
                SummaryTemplate = "{count} support tickets describe technical problems (crashes, performance, mobile). Themes: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Account Management",
                Description = "Account state, suspension, plan changes, and access requests.",
                Keywords = new[] {
                    "account", "suspended", "workspace", "teammate", "invite", "permission",
                    "downgrade", "plan", "tier", "enterprise", "upgrade", "lockout"
                },
                Icon = "\uE716", AccentColor = "#134F5C", IconBackground = "#A4C2F4",
                SummaryTemplate = "{count} support tickets are about account state, plans, or workspace access. Key terms: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Feature Requests",
                Description = "Suggestions for new features, integrations, or improvements.",
                Keywords = new[] {
                    "would love", "would be great", "could you", "please add", "feature request",
                    "roadmap", "wish", "should add", "missing feature", "suggestion", "suggest",
                    "public roadmap", "would like to", "it would", "could you add",
                    "csv export", "pdf", "export", "saml", "sso", "conditional logic", "automation"
                },
                Icon = "\uE710", AccentColor = "#351C75", IconBackground = "#E1D5E7",
                SummaryTemplate = "{count} support tickets contain feature or product suggestions. Top asks: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Customer Support",
                Description = "Feedback on the support experience, response time, and bots.",
                Keywords = new[] {
                    "support", "support team", "customer service", "service", "agent", "responded",
                    "response time", "responsive", "unhelpful", "useless bot", "chat bot", "chatbot",
                    "reach a human", "no one replies", "help desk", "no way to", "no one"
                },
                Icon = "\uE8FE", AccentColor = "#783F04", IconBackground = "#FCE4D6",
                SummaryTemplate = "{count} support tickets complain about the support experience itself. Recurring issues: {keywords}."
            },
        });

    // ------------------------------------------------------------------
    // Survey Responses taxonomy
    // ------------------------------------------------------------------
    public static readonly IReadOnlyList<IntentDefinition> SurveyResponses = new ReadOnlyCollection<IntentDefinition>(
        new List<IntentDefinition>
        {
            new IntentDefinition
            {
                Name = "Highly Satisfied",
                Description = "Strong positive feedback, delight, advocacy.",
                Keywords = new[] {
                    "satisfied", "extremely satisfied", "love", "loved", "loving", "amazing",
                    "excellent", "fantastic", "wonderful", "impressed", "delighted", "best",
                    "appreciate", "thank you", "thanks", "good job", "nice work", "happy"
                },
                Icon = "\uE735", AccentColor = "#0F9D58", IconBackground = "#D5E8D4",
                SummaryTemplate = "{count} survey responses are strongly positive. Highlights: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Generally Positive",
                Description = "Mildly positive responses with minor suggestions.",
                Keywords = new[] {
                    "good", "great", "helpful", "clear", "productive", "intuitive", "fast",
                    "appreciate", "recommend", "enjoy", "love the"
                },
                Icon = "\uE73E", AccentColor = "#274E13", IconBackground = "#B6D7A8",
                SummaryTemplate = "{count} survey responses are generally positive with light feedback. Common terms: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Neutral",
                Description = "Mixed or neutral responses, often with one or two small asks.",
                Keywords = new[] {
                    "okay", "fine", "average", "neutral", "decent", "missing", "would recommend",
                    "if that gets added", "small ask", "would be better", "could be improved"
                },
                Icon = "\uE9CE", AccentColor = "#5F6368", IconBackground = "#E8EAED",
                SummaryTemplate = "{count} survey responses are neutral with a small ask. Common themes: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Dissatisfied",
                Description = "Negative responses pointing to pain points.",
                Keywords = new[] {
                    "unhappy", "dissatisfied", "frustrated", "annoyed", "disappointed",
                    "missing", "broken", "issue", "issues", "problem", "problems", "slow", "bad",
                    "stopped working", "doesn't work", "does not work"
                },
                Icon = "\uE783", AccentColor = "#990000", IconBackground = "#F4CCCC",
                SummaryTemplate = "{count} survey responses are dissatisfied. Top pain points: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Feature Requests",
                Description = "Suggestions, missing features, or improvements mentioned in surveys.",
                Keywords = new[] {
                    "would love", "would be great", "could you", "please add", "feature request",
                    "roadmap", "wish", "should add", "missing feature", "suggestion", "suggest",
                    "would like to", "it would", "could you add", "csv export", "pdf", "export"
                },
                Icon = "\uE710", AccentColor = "#351C75", IconBackground = "#E1D5E7",
                SummaryTemplate = "{count} survey responses request a new feature or improvement. Top asks: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Support Experience",
                Description = "Feedback specifically about the support team, bots, or response time.",
                Keywords = new[] {
                    "support", "customer service", "agent", "responded", "response time",
                    "responsive", "unhelpful", "useless bot", "chat bot", "chatbot",
                    "reach a human", "no one replies", "help desk", "support team"
                },
                Icon = "\uE8FE", AccentColor = "#783F04", IconBackground = "#FCE4D6",
                SummaryTemplate = "{count} survey responses mention the support experience. Themes: {keywords}."
            },
        });

    // ------------------------------------------------------------------
    // Product Reviews taxonomy
    // ------------------------------------------------------------------
    public static readonly IReadOnlyList<IntentDefinition> ProductReviews = new ReadOnlyCollection<IntentDefinition>(
        new List<IntentDefinition>
        {
            new IntentDefinition
            {
                Name = "Positive Feedback",
                Description = "Strongly positive reviews: design, setup, polish.",
                Keywords = new[] {
                    "love", "loved", "loving", "amazing", "excellent", "fantastic", "wonderful",
                    "impressed", "delighted", "best", "gorgeous", "painless", "modern",
                    "beautifully designed", "delightful", "modern", "intuitive"
                },
                Icon = "\uE735", AccentColor = "#0F9D58", IconBackground = "#D5E8D4",
                SummaryTemplate = "{count} reviews are strongly positive. Highlights: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Negative Feedback",
                Description = "Strongly negative reviews: performance, bugs, broken features.",
                Keywords = new[] {
                    "terrible", "awful", "horrible", "unusable", "broken", "hate", "worst",
                    "doesn't work", "does not work", "stopped working", "flicker", "overwhelming",
                    "quality has dropped", "lost data", "data loss", "bug", "bugs", "small bugs",
                    "regression", "regressions", "drains", "battery", "slow", "lag", "laggy"
                },
                Icon = "\uE783", AccentColor = "#990000", IconBackground = "#F4CCCC",
                SummaryTemplate = "{count} reviews are strongly negative. Common complaints: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Feature Requests",
                Description = "Suggestions, missing features, or roadmap ideas.",
                Keywords = new[] {
                    "would love", "would be great", "could you", "please add", "feature request",
                    "roadmap", "wish", "should add", "missing feature", "suggestion", "suggest",
                    "public roadmap", "would like to", "it would", "could you add",
                    "csv export", "pdf", "export", "conditional logic", "automation", "pomodoro"
                },
                Icon = "\uE710", AccentColor = "#351C75", IconBackground = "#E1D5E7",
                SummaryTemplate = "{count} reviews contain new feature ideas. Top asks: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Usability Concerns",
                Description = "UX, navigation, settings, and overall design feedback.",
                Keywords = new[] {
                    "settings", "redesign", "navigation", "menu", "design", "ui", "ux",
                    "user interface", "user experience", "onboarding", "empty state",
                    "tooltip", "dark mode", "dark theme", "accessibility", "look and feel",
                    "overwhelming", "too many", "unclear categories", "modern"
                },
                Icon = "\uE7F4", AccentColor = "#134F5C", IconBackground = "#A4C2F4",
                SummaryTemplate = "{count} reviews mention usability, navigation, or design. Recurring feedback: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Performance & Bugs",
                Description = "Reports of slowness, crashes, regressions, or stability issues.",
                Keywords = new[] {
                    "performance", "slow", "lag", "laggy", "freeze", "freezes", "crash",
                    "crashes", "crashed", "speed", "loading", "load", "hang", "stuck",
                    "battery", "memory", "cpu", "drain", "flicker", "regression",
                    "quality has dropped", "small bugs"
                },
                Icon = "\uE945", AccentColor = "#B45F06", IconBackground = "#FCE5CD",
                SummaryTemplate = "{count} reviews report performance or bug issues. Themes: {keywords}."
            },
        });

    // ------------------------------------------------------------------
    // Employee Feedback taxonomy
    // ------------------------------------------------------------------
    public static readonly IReadOnlyList<IntentDefinition> EmployeeFeedback = new ReadOnlyCollection<IntentDefinition>(
        new List<IntentDefinition>
        {
            new IntentDefinition
            {
                Name = "Engineering",
                Description = "Engineering tooling, CI/CD, on-call, and dev workflow.",
                Keywords = new[] {
                    "ci", "pipeline", "build", "test", "on-call", "paged", "runbook", "runbooks",
                    "incident", "incidents", "deployment", "deploy", "release", "engineering",
                    "caching", "parallel"
                },
                Icon = "\uE756", AccentColor = "#0B5394", IconBackground = "#CFE2F3",
                SummaryTemplate = "{count} items are about engineering workflow and tooling. Themes: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Sales & CRM",
                Description = "Sales tooling, CRM, quotas, and customer-facing workflows.",
                Keywords = new[] {
                    "crm", "sales", "quota", "deal", "deals", "pipeline", "lead", "leads",
                    "filters", "open deals"
                },
                Icon = "\uE7BF", AccentColor = "#274E13", IconBackground = "#B6D7A8",
                SummaryTemplate = "{count} items are about sales tooling or CRM. Topics: {keywords}."
            },
            new IntentDefinition
            {
                Name = "People & Culture",
                Description = "Onboarding, HR, wellness, and employee experience.",
                Keywords = new[] {
                    "onboarding", "buddy", "wellness", "snacks", "lunch", "stipend",
                    "hr", "people", "culture", "energy"
                },
                Icon = "\uE716", AccentColor = "#351C75", IconBackground = "#E1D5E7",
                SummaryTemplate = "{count} items are about people, HR, or culture. Recurring terms: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Design Process",
                Description = "Design-to-engineering handoff and design operations.",
                Keywords = new[] {
                    "design", "figma", "handoff", "component", "components", "design system",
                    "design ops", "workflow", "designers"
                },
                Icon = "\uE7F4", AccentColor = "#134F5C", IconBackground = "#A4C2F4",
                SummaryTemplate = "{count} items are about the design process or design ops. Themes: {keywords}."
            },
            new IntentDefinition
            {
                Name = "Operations",
                Description = "Office, facilities, perks, and internal operations.",
                Keywords = new[] {
                    "office", "snacks", "lunch", "stipend", "perks", "operations", "facilities",
                    "wellness day", "policy"
                },
                Icon = "\uE8B7", AccentColor = "#783F04", IconBackground = "#FCE4D6",
                SummaryTemplate = "{count} items are about internal operations or perks. Common terms: {keywords}."
            },
        });

    // ------------------------------------------------------------------
    // Domain → taxonomy dispatch
    // ------------------------------------------------------------------
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<IntentDefinition>> _byDomain =
        new Dictionary<string, IReadOnlyList<IntentDefinition>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Customer Feedback"] = CustomerFeedback,
            ["Support Tickets"]   = SupportTickets,
            ["Survey Responses"]  = SurveyResponses,
            ["Product Reviews"]   = ProductReviews,
            ["Employee Feedback"] = EmployeeFeedback,
        };

    /// <summary>
    /// Look up the intent taxonomy for a domain. Falls back to
    /// <see cref="CustomerFeedback"/> when the domain is unknown.
    /// </summary>
    public static IReadOnlyList<IntentDefinition> ForDomain(string? domain)
    {
        if (!string.IsNullOrWhiteSpace(domain) && _byDomain.TryGetValue(domain, out var list))
        {
            return list;
        }
        return CustomerFeedback;
    }

    public static readonly IReadOnlyList<string> StopWords = new ReadOnlyCollection<string>(
        new List<string>
        {
            "the", "and", "for", "with", "this", "that", "have", "has", "was", "were", "but",
            "not", "you", "your", "are", "from", "all", "can", "could", "would", "should",
            "our", "out", "get", "got", "any", "one", "two", "into", "than", "then", "when",
            "what", "how", "why", "who", "where", "which", "there", "their", "they", "them",
            "i", "me", "my", "we", "us", "he", "she", "his", "her", "it", "its", "of", "to",
            "in", "on", "at", "as", "is", "am", "be", "been", "being", "do", "does", "did",
            "so", "if", "or", "an", "a", "by", "up", "no", "yes", "too", "very", "more", "most",
            "some", "such", "only", "own", "same", "also", "just", "about", "after", "before"
        });
}
