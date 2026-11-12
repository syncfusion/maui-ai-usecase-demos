using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SkyBook.Core;

/// <summary>Port of <c>summarizeAction</c> (App.tsx): human-readable chat text shown for a UI action.</summary>
public static class ActionSummarizer
{
    public static string Summarize(JsonObject? action)
    {
        if (action is null) return "Submitted your selection.";
        var name = Str(action["name"]) ?? Str(action["action"]) ?? "";
        var ctx = action["context"] as JsonObject ?? new JsonObject();

        var from = Str(ctx["from"]) ?? Str(ctx["origin"]);
        var to = Str(ctx["to"]) ?? Str(ctx["destination"]);
        var date = Str(ctx["departureDate"]) ?? Str(ctx["date"]);
        var route = from is not null && to is not null ? $" from {from} to {to}" : "";

        var pax = "";
        if (Str(ctx["adults"]) is { } adultsText)
        {
            pax = $"{adultsText} adult{(adultsText == "1" ? "" : "s")}";
            if (Truthy(ctx["children"]))
            {
                var kids = Str(ctx["children"]);
                pax += $", {kids} child{(kids == "1" ? "" : "ren")}";
            }
        }

        var flight = Str(ctx["flightNumber"]) ?? Str(ctx["flight"]) ?? Str(ctx["airline"]) ?? Str(ctx["flightId"]);
        var price = Str(ctx["price"]) ?? Str(ctx["fare"]);
        var dateText = date is not null ? $" on {date}" : "";
        var paxText = pax != "" ? $" for {pax}" : "";

        switch (name)
        {
            case "selectFlight":
                return $"Book{(flight is not null ? $" {flight}" : " this flight")}" +
                       $"{(from is not null && to is not null ? $" ({from} → {to})" : "")}" +
                       $"{(price is not null ? $" at {price}" : "")}.";
            case "searchFlights":
                return $"Search flights{route}{dateText}{paxText}.";
            case "select":
                return $"Book this flight{route}{dateText}{paxText}.";
            default:
                var label = Regex.Replace(string.IsNullOrEmpty(name) ? "request" : name, "([A-Z])", " $1").Trim();
                return char.ToUpperInvariant(label[0]) + label[1..] + ".";
        }
    }

    private static string? Str(JsonNode? n) =>
        n is JsonValue v ? (v.TryGetValue<string>(out var s) ? s : v.ToJsonString().Trim('"')) : null;

    // JS truthiness for the values used here (null/""/0/false are falsy).
    private static bool Truthy(JsonNode? n) => Str(n) is { } s && s != "" && s != "0" && s != "false";
}
