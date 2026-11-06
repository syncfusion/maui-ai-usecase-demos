namespace AIFileSearchTreeView;

/// <summary>
/// Represents the result of an AI-powered semantic search.
/// </summary>
public class SearchResult
{
    public List<MatchedNode> Matches { get; } = new();
    public int TotalMatches => Matches.Count;
    public DateTime SearchExecutedAt { get; set; }
    public double ExecutionTimeMs { get; set; }
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// A single matched file. <c>ConfidenceScore</c> and <c>MatchReasons</c> are
    /// intentionally not populated — the model only returns paths, not
    /// confidence scores, and we don't want to fabricate values.
    /// </summary>
    public class MatchedNode
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}