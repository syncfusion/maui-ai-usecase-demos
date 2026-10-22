using System.Text;
using System.Text.Json;
using WardrobeAI.AI.Contracts;

namespace WardrobeAI.AI;

/// <summary>
/// Builds the user-prompt JSON for <see cref="IAISmartShoppingService"/>.
/// Kept as a tiny static helper so the service implementation stays focused
/// on the HTTP call + JSON-schema plumbing. The system prompt itself is
/// held in <c>AI/PromptTemplates/SmartShopping.md</c> for review and is
/// also inlined into <see cref="SystemPrompt"/> below so the build does
/// not need to wire up embedded resources.
/// </summary>
internal static class SmartShoppingPromptBuilder
{
    /// <summary>
    /// System prompt inlined verbatim from
    /// <c>AI/PromptTemplates/SmartShopping.md</c>. Kept in sync with the
    /// markdown file — any change must touch both.
    /// </summary>
    public const string SystemPrompt =
        "You are Wardrobe AI, a personal stylist and a wardrobe-gap analyst. Your job is to look at the " +
        "user's actual closet, compare it to a curated 21-item \"essential closet\" catalog, and recommend the items they are missing — " +
        "focusing on pieces that genuinely pair with what they already own, not the first 5 entries in the catalog.\n\n" +

        "Hard rules:\n" +
        "1. Use ONLY items from the `catalog[]` list. Do not invent pieces.\n" +
        "2. The `name` you return MUST match a `catalog[].name` exactly (case-insensitive is fine). The Shopping page joins the response back to the catalog by `name`.\n" +
        "3. Do NOT return the first N catalog items in catalog order. Vary your picks across categories, weight the selection by what the user is actually missing, and RANDOMIZE the order of `picks[]` so the Shopping page is not deterministic on the catalog's order.\n" +
        "4. For each pick, list 1-3 names of the user's existing items (from `closet[]`) that the new piece would pair with. Use the user's own item names verbatim.\n" +
        "5. Return JSON only. Response MUST match the `smart_shopping_picks` schema. No prose outside the JSON. No markdown fences.\n" +
        "6. `priority` is your honest assessment of how urgently the user needs this piece. A foundational slot the user has zero of -> HIGH. A situational slot the user could grow into -> MEDIUM. A nice-to-have -> LOW.\n" +
        "7. `reason` is 1 sentence, 30-140 chars, and must mention at least one of the user's existing items by name (e.g. \"Pairs with your White Oxford Shirt and Indigo Jeans for an instant smart-casual outfit.\").\n" +
        "8. `analysisSummary` is 1-2 sentences, 60-220 chars, summarising the closet gap (mention specific owned categories + the highest-priority missing one).\n\n" +

        "Style of reasoning:\n" +
        "- User has 5 T-Shirts and 2 pairs of Jeans but no formal Shirt -> HIGH on White Oxford Shirt because it elevates their existing denim.\n" +
        "- User has 0 Sneakers and only casual wear -> HIGH on White Sneakers because it unlocks every casual outfit they already own.\n" +
        "- User has plenty of Footwear and a Kurta but no Blazer -> MEDIUM on Navy Blazer because it anchors smart-casual but they can borrow / rent one in the meantime.\n" +
        "Avoid restating the catalog. Every reason must reference the user's owned items.\n\n" +

        "When the user owns 0 items: return the 5 most foundational pieces from the catalog (T-Shirts, Shirts, Jeans, Trousers, Sneakers) in a RANDOMIZED order, and make `analysisSummary` a friendly \"build your foundational wardrobe\" message.";

    /// <summary>
    /// JSON schema sent to the LLM via response_format.json_schema. The
    /// schema is hand-written (not generated via JsonSchemaExporter) so it
    /// stays stable across SDK upgrades and can be copy-pasted into the
    /// Azure OpenAI "Structured outputs" playground for testing.
    /// </summary>
    public const string ResponseJsonSchema = @"{
        ""type"": ""object"",
        ""additionalProperties"": false,
        ""required"": [""picks"", ""analysisSummary""],
        ""properties"": {
            ""analysisSummary"": { ""type"": ""string"", ""minLength"": 1, ""maxLength"": 320 },
            ""picks"": {
                ""type"": ""array"",
                ""minItems"": 1,
                ""maxItems"": 12,
                ""items"": { ""$ref"": ""#/$defs/pick"" }
            }
        },
        ""$defs"": {
            ""pick"": {
                ""type"": ""object"",
                ""additionalProperties"": false,
                ""required"": [""name"", ""category"", ""priority"", ""reason"", ""pairsWithNames""],
                ""properties"": {
                    ""name"":          { ""type"": ""string"", ""minLength"": 1, ""maxLength"": 80 },
                    ""category"":      { ""type"": ""string"", ""minLength"": 1, ""maxLength"": 32 },
                    ""priority"":      { ""type"": ""string"", ""enum"": [""HIGH"", ""MEDIUM"", ""LOW""] },
                    ""reason"":        { ""type"": ""string"", ""minLength"": 30, ""maxLength"": 240 },
                    ""pairsWithNames"": {
                        ""type"": ""array"",
                        ""minItems"": 1,
                        ""maxItems"": 3,
                        ""items"": { ""type"": ""string"", ""minLength"": 1, ""maxLength"": 80 }
                    }
                }
            }
        }
    }";

    /// <summary>
    /// Serializes the request as a single user message. The catalog is
    /// the biggest chunk — we keep it as a JSON array of
    /// <see cref="SmartShoppingCatalogItem"/> records so the LLM can scan
    /// it linearly. The closet is projected down to its
    /// <see cref="SmartShoppingClosetItem"/> shape so PII (image, name,
    /// email) never reaches the model.
    /// </summary>
    public static string BuildUserPrompt(AISmartShoppingRequest request)
    {
        var sb = new StringBuilder(2048);
        sb.AppendLine("## Context");
        sb.Append("Style preference: ").AppendLine(string.IsNullOrWhiteSpace(request.StylePreference) ? "(none)" : request.StylePreference);
        sb.Append("Budget level:    ").AppendLine(string.IsNullOrWhiteSpace(request.BudgetLevel) ? "(unspecified)" : request.BudgetLevel);
        sb.Append("Monthly budget:  $").Append(request.MonthlyBudgetUsd.ToString("0")).AppendLine(" USD");
        sb.Append("Max picks:       ").Append(request.MaxResults).AppendLine();

        // Summarise the closet so the model can see the per-category gap
        // without us pre-filtering the catalog.
        var byCategory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var anchorByCategory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in request.Closet)
        {
            if (string.IsNullOrWhiteSpace(item.Category))
            {
                continue;
            }
            byCategory[item.Category] = byCategory.TryGetValue(item.Category, out var n) ? n + 1 : 1;
            if (!anchorByCategory.ContainsKey(item.Category))
            {
                anchorByCategory[item.Category] = item.Name;
            }
        }
        sb.AppendLine();
        sb.Append("Closet size:     ").Append(request.Closet.Count).Append(" items across ")
          .Append(byCategory.Count).AppendLine(" categories");
        if (byCategory.Count > 0)
        {
            sb.AppendLine("Per-category counts:");
            foreach (var kv in byCategory.OrderByDescending(kv => kv.Value))
            {
                var anchor = anchorByCategory.TryGetValue(kv.Key, out var a) ? a : "(no name)";
                sb.Append("  - ").Append(kv.Key).Append(": ").Append(kv.Value)
                  .Append("  (anchor: \"").Append(anchor).AppendLine("\")");
            }
        }
        else
        {
            sb.AppendLine("Closet is empty — recommend foundational pieces in a randomized order.");
        }

        sb.AppendLine();
        sb.AppendLine("## Closet (anchor items the new pieces should pair with)");
        sb.AppendLine("```json");
        sb.AppendLine(JsonSerializer.Serialize(request.Closet, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        }));
        sb.AppendLine("```");

        sb.AppendLine();
        sb.AppendLine("## Catalog (the 21 essential items — pick from THIS list only)");
        sb.AppendLine("```json");
        sb.AppendLine(JsonSerializer.Serialize(request.Catalog, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        }));
        sb.AppendLine("```");

        sb.AppendLine();
        sb.AppendLine("## Response (strict JSON, matches the smart_shopping_picks schema)");

        return sb.ToString();
    }
}
