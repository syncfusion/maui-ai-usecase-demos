using System.ClientModel;
using System.Diagnostics;
using System.Text;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AIFileSearchTreeView.Services;

/// <summary>
/// Wraps the Azure OpenAI chat client used to rank file metadata against a
/// natural-language search query.
/// </summary>
/// <remarks>
/// Credentials are read from the following environment variables:
/// <list type="bullet">
///   <item><c>AZURE_OPENAI_ENDPOINT</c> – e.g. <c>https://your-resource.openai.azure.com/</c></item>
///   <item><c>AZURE_OPENAI_DEPLOYMENT_NAME</c> – name of the chat deployment</item>
///   <item><c>AZURE_OPENAI_API_KEY</c> – API key for the Azure OpenAI resource</item>
/// </list>
/// If any value is missing or the endpoint is malformed, <see cref="IsCredentialValid"/>
/// is <c>false</c> and <see cref="SearchFilesSemanticAsync"/> returns an empty list.
/// </remarks>
public sealed class SemanticSearchService : ISemanticSearchService
{
    /// <summary>
    /// System prompt that instructs the model to return only file paths.
    /// </summary>
    private const string SystemPrompt =
        """
        You are an intelligent natural-language file search assistant.

        Your goal is to identify and rank the most relevant files from the supplied TreeView records.

        Search Rules

        1. Compare the user's complete query against all supplied file records.

        2. Determine relevance using all available metadata together:
           - File Name
           - Folder Path
           - Keywords
           - File Type
           - Created Date
           - Modified Date

        3. Understand the user's overall intent.
           Do not perform simple keyword matching.

        4. Use reasonable semantic relationships and synonyms when supported by the metadata.

           Examples:
           - wifi → wireless, access point, network connectivity
           - internet → broadband, connectivity, network
           - coverage → survey, test, assessment
           - invoice → bill, statement, billing record
           - security → firewall, policy, protection, compliance
           - design → architecture, topology, diagram

        5. Consider the complete meaning of the query.
           A file is relevant only when its overall metadata supports the user's intent.

        6. Do not return a file simply because it shares a single broad keyword.

        7. When the query contains words such as:
           - recent
           - latest
           - newest
           - current

           prefer newer relevant files first.

        8. Recency affects ranking only.
           It must not make unrelated files relevant.

        9. Return all genuinely relevant files when multiple files help satisfy the user's request.

        10. Return fewer files when only a small number of files are strongly relevant.

        11. Folder records provide context only.
            Never return folder paths as results.

        12. Return only exact leaf-file paths that exist in the supplied records.

        13. Order results from most relevant to least relevant.

        Output Rules

        14. Return one exact file path per line.

        15. Do not return:
            - explanations
            - reasoning
            - confidence scores
            - markdown
            - bullets
            - numbering
            - folder names
            - invented paths

        16. Return an empty response when no supplied file is relevant.
        """;

    private readonly AzureOpenAIClient? _client;
    private readonly string _deploymentName;

    public SemanticSearchService()
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        _deploymentName = deploymentName?.Trim() ?? string.Empty;

        if (TryCreateClient(endpoint, _deploymentName, apiKey, out var client))
        {
            _client = client;
            IsCredentialValid = true;
        }
    }

    /// <inheritdoc />
    public bool IsCredentialValid { get; }

    public async Task<List<string>> SearchFilesSemanticAsync(
        string query,
        List<FileMetadataContext> fileMetadata,
        CancellationToken cancellationToken = default)
    {
        if (_client is null || string.IsNullOrWhiteSpace(query) || fileMetadata.Count == 0)
        {
            return new List<string>();
        }

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(BuildUserPrompt(query, fileMetadata))
            };

            var response = await _client
                .GetChatClient(_deploymentName)
                .CompleteChatAsync(messages, cancellationToken: cancellationToken);

            var content = string.Join(
                Environment.NewLine,
                response.Value.Content
                    .Select(part => part.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));

            return ValidateReturnedPaths(ParseSearchResults(content), fileMetadata);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (RequestFailedException ex)
        {
            // Surface the real Azure error in debug output; the UI just shows
            // "No matches found" because a failed call has no matches to render.
            Debug.WriteLine($"Semantic search failed (HTTP {(int)ex.Status} {ex.Status}): {ex.Message}");
            return new List<string>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Semantic search failed: {ex.Message}");
            return new List<string>();
        }
    }

    private static string BuildUserPrompt(string query, IEnumerable<FileMetadataContext> fileMetadata)
    {
        var fileList = new StringBuilder();
        foreach (var file in fileMetadata)
        {
            var recordType = string.IsNullOrWhiteSpace(file.FileType) ? "File" : file.FileType;
            fileList.Append("- Path: ").Append(file.FullPath);
            fileList.Append(" | Name: ").Append(file.FileName);
            fileList.Append(" | Type: ").Append(recordType);
            fileList.Append(" | Created: ").Append(file.CreatedDate.ToString("yyyy-MM-dd"));
            fileList.Append(" | Modified: ").Append(file.ModifiedDate.ToString("yyyy-MM-dd"));

            if (file.Keywords?.Length > 0)
            {
                fileList.Append(" | Keywords: ").Append(string.Join(", ", file.Keywords));
            }

            fileList.AppendLine();
        }

        return $"""
            User query:
            {query.Trim()}

            Files in the current TreeView:
            {fileList}
            """;
    }

    private static List<string> ParseSearchResults(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return new List<string>();
        }

        return response
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim().TrimStart('-', '*').Trim())
            .Where(line => line.StartsWith('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> ValidateReturnedPaths(
        IEnumerable<string> returnedPaths,
        IEnumerable<FileMetadataContext> fileMetadata)
    {
        var canonicalPaths = fileMetadata
            .Where(file => !string.IsNullOrWhiteSpace(file.FullPath))
            .ToDictionary(file => NormalizePath(file.FullPath), file => file.FullPath, StringComparer.OrdinalIgnoreCase);

        return returnedPaths
            .Where(path => canonicalPaths.ContainsKey(NormalizePath(path)))
            .Select(path => canonicalPaths[NormalizePath(path)])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizePath(string path) =>
        path.Replace('\\', '/').Trim().ToLowerInvariant();

    private static bool TryCreateClient(
        string? endpoint,
        string deploymentName,
        string? apiKey,
        out AzureOpenAIClient? client)
    {
        client = null;

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(deploymentName))
        {
            return false;
        }

        if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var endpointUri)
            || (endpointUri.Scheme != Uri.UriSchemeHttp && endpointUri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        try
        {
            client = new AzureOpenAIClient(endpointUri, new ApiKeyCredential(apiKey));
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AzureOpenAIClient construction failed: {ex.Message}");
            return false;
        }
    }
}