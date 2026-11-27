using System.ClientModel;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;

namespace DataGrid_Duplicate_Sample;

/// <summary>
/// Sends candidate customer pairs to Azure OpenAI and asks the model to act as a
/// duplicate-record detection engine. The full system prompt lives in
/// <see cref="SystemPrompt"/> and is intentionally kept verbatim from the
/// product spec so behavior is deterministic and auditable.
/// </summary>
public sealed class AzureOpenAIDuplicateAnalyzer : IDuplicateAnalyzer
{
    public string Endpoint { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
    public string ApiKey { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
    public string DeploymentName { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? string.Empty;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new CasingEnumConverter<DuplicateClassification>(),
            new CasingEnumConverter<RowHighlight>()
        }
    };

    public const string SystemPrompt = """
        You are an AI duplicate-record detection engine used inside a Syncfusion SfDataGrid customer-management application.

        Your purpose is to identify and explain potentially duplicated customer records using advanced similarity matching rather than simple string Contains comparisons.

        The application displays customer records in SfDataGrid, supports inline editing, allows users to add new records, performs live duplicate detection while typing, and can scan the entire grid when the user clicks "Identify Duplicates".

        You must produce structured, explainable duplicate-detection results that can be displayed directly in an SfDataGrid review panel.

        IMPORTANT RULES

        1. Evaluate all available evidence.
        2. Never rely on a simple substring or Contains check.
        3. Prefer false negatives over weak duplicate claims.
        4. Missing values are unknown evidence, not mismatches.
        5. Different record IDs are never evidence.
        6. Phonetic similarity alone is not enough.
        7. Embedding similarity alone is not enough.
        8. A duplicate decision requires at least two independent supporting signals.
        9. Contradictory evidence must reduce confidence.
        10. Do not invent missing values.

        MATCHING SIGNALS

        NAME
        - Handle spelling variations.
        - Handle reordered names.
        - Handle initials.
        - Handle nicknames and aliases.
        - Handle phonetic similarity.
        - Treat phonetic similarity as supporting evidence only.

        EMAIL
        - Normalize case.
        - Compare local part and domain separately.
        - Detect minor typographical errors.
        - Shared company domains alone are not sufficient.

        PHONE
        - Ignore spaces.
        - Ignore punctuation.
        - Ignore country-code formatting differences.
        - Detect transposed digits.
        - Detect single-digit omissions.
        - Shared company switchboard numbers alone are not sufficient.

        ADDRESS
        - Normalize common abbreviations:
          - St = Street
          - Rd = Road
          - Ave = Avenue
          - Apt = Apartment
          - Blvd = Boulevard
          - Dr = Drive
        - Compare:
          - House/building number
          - Street
          - City
          - State
          - Postal code
          - Address token similarity
        - Use provided address embedding similarity when available.

        COMPANY
        - Ignore casing.
        - Ignore punctuation.
        - Ignore legal suffixes:
          - Ltd
          - LLC
          - Inc
          - Pvt Ltd
          - Corporation
          - Corp
        - Use provided company embedding similarity when available.

        AI EMBEDDINGS

        AddressEmbeddingSimilarity and CompanyEmbeddingSimilarity may be supplied.

        Interpretation guidance:

        0.95 - 1.00 = very strong semantic similarity
        0.90 - 0.94 = strong similarity
        0.80 - 0.89 = moderate similarity
        Below 0.80 = weak similarity

        Embedding similarity can strengthen evidence but must never be the sole reason for a duplicate decision.

        CONFIDENCE SCORING

        90-100
        Likely duplicate

        Requirements:
        - At least 3 independent supporting signals
        OR
        - 2 exceptionally strong supporting signals with no meaningful contradiction

        75-89
        Possible duplicate

        Requirements:
        - At least 2 supporting signals
        - Evidence is meaningful but not conclusive

        0-74
        Not duplicate

        Requirements:
        - Insufficient evidence
        OR
        - Contradictory evidence outweighs supporting evidence

        SFDATAGRID HIGHLIGHTING BEHAVIOR

        Use confidence levels to drive UI highlighting:

        Confidence >= 90
        RowHighlight = "LikelyDuplicate"

        Confidence 75-89
        RowHighlight = "PossibleDuplicate"

        Confidence < 75
        RowHighlight = "None"

        Duplicate groups should share the same GroupId when multiple records belong to the same duplicate cluster.

        OUTPUT REQUIREMENTS

        Return valid JSON only.

        Do not return markdown.
        Do not return explanations outside JSON.
        Do not return comments.

        Output format:

        {
          "results": [
            {
              "pairId": "string",
              "recordAId": "string",
              "recordBId": "string",
              "confidence": 0,
              "classification": "likely_duplicate | possible_duplicate | not_duplicate",
              "rowHighlight": "LikelyDuplicate | PossibleDuplicate | None",
              "groupId": "optional duplicate group identifier",
              "supportingSignals": [
                {
                  "field": "name | email | phone | address | company",
                  "strength": "strong | moderate | weak",
                  "reason": "short explanation"
                }
              ],
              "conflictingSignals": [
                {
                  "field": "name | email | phone | address | company",
                  "reason": "short explanation"
                }
              ],
              "summary": "short human-readable explanation suitable for SfDataGrid reviewers"
            }
          ]
        }
        """;

    public async Task<List<DuplicateResult>> AnalyzeAsync(IReadOnlyList<CustomerPair> candidatePairs, CancellationToken cancellationToken = default)
    {
        if (candidatePairs == null || candidatePairs.Count == 0)
            return new List<DuplicateResult>();

        var client = new AzureOpenAIClient(new Uri(Endpoint), new AzureKeyCredential(ApiKey));
        var chat = client.GetChatClient(DeploymentName);

        var userPayload = JsonSerializer.Serialize(candidatePairs, new JsonSerializerOptions
        {
            WriteIndented = false
        });

        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            new OpenAI.Chat.SystemChatMessage(SystemPrompt),
            new OpenAI.Chat.UserChatMessage(
                "Analyze the following candidate customer-record pairs and return only the JSON response described above:\n\n"
                + userPayload)
        };

        var options = new OpenAI.Chat.ChatCompletionOptions
        {
            Temperature = 0f,
            ResponseFormat = OpenAI.Chat.ChatResponseFormat.CreateJsonObjectFormat()
        };

        OpenAI.Chat.ChatCompletion completion;
        try
        {
            completion = await chat.CompleteChatAsync(messages, options, cancellationToken);
        }
        catch (ClientResultException ex) when (ex.Status == 401)
        {
            throw new InvalidOperationException(
                "Azure OpenAI returned 401 Access Denied. The API key and endpoint usually come from " +
                "different resources — paste a key that was copied from the SAME resource whose " +
                "hostname is in the Endpoint setting.", ex);
        }
        catch (ClientResultException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException(
                $"Azure OpenAI returned 404 ({ex.Data}). The deployment name '{DeploymentName}' " +
                "probably doesn't exist on this resource. Check Azure portal → your resource → " +
                "Model deployments and use the deployment NAME (not the model name).", ex);
        }

        var json = string.Concat(completion.Content.Select(p => p.Text ?? ""));
        JsonElement envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<JsonElement>(json, JsonOpts);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Azure OpenAI returned JSON that could not be parsed at all. " +
                $"Underlying error: {ex.Message}. " +
                $"Full response: {Truncate(json, 4000)}", ex);
        }

        if (envelope.ValueKind != JsonValueKind.Object ||
            !envelope.TryGetProperty("results", out var resultsEl) ||
            resultsEl.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                $"Azure OpenAI response did not contain a 'results' array. " +
                $"Full response: {Truncate(json, 4000)}");
        }

        var results = new List<DuplicateResult>(resultsEl.GetArrayLength());
        var badEntries = 0;
        foreach (var entry in resultsEl.EnumerateArray())
        {
            try
            {
                var r = entry.Deserialize<DuplicateResult>(JsonOpts);
                if (r is not null) results.Add(r);
            }
            catch (JsonException)
            {
                badEntries++;
            }
        }

        if (badEntries > 0)
        {
            // Surface a hint to the UI without crashing the whole scan.
            throw new InvalidOperationException(
                $"Azure OpenAI returned {badEntries} result(s) the deserializer could not " +
                $"parse. The other {results.Count} pair(s) were processed. " +
                $"Full response: {Truncate(json, 4000)}");
        }

        return results;
    }

    private static string Truncate(string strName, int max) => strName.Length <= max ? strName : strName[..max] + "…";
}
