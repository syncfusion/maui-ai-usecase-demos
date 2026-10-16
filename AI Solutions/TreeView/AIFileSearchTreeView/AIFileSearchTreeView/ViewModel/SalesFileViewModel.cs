using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIFileSearchTreeView.Data;
using AIFileSearchTreeView.Services;

namespace AIFileSearchTreeView.ViewModel;

/// <summary>
/// ViewModel for AI-powered sales file search with semantic understanding.
/// Owns the search state and high-level expand/highlight logic; the page
/// drives the debounce and the request-version guard.
/// </summary>
public sealed class SalesFileViewModel : INotifyPropertyChanged
{
    private readonly ISemanticSearchService _semanticSearchService;

    private ObservableCollection<SalesFileNode> _salesFileNodes = new();
    private ObservableCollection<SalesFileNode> _filteredSalesFileNodes = new();
    private int _matchCount;
    private string _matchCountLabel = string.Empty;

    public SalesFileViewModel(ISemanticSearchService semanticSearchService)
    {
        _semanticSearchService = semanticSearchService ?? throw new ArgumentNullException(nameof(semanticSearchService));
        SalesFileNodes = SalesFileRepository.CreateSampleTree();
    }

    /// <summary>
    /// Full file tree. The TreeView binds to this when no search is active.
    /// </summary>
    public ObservableCollection<SalesFileNode> SalesFileNodes
    {
        get => _salesFileNodes;
        private set => SetField(ref _salesFileNodes, value);
    }

    /// <summary>
    /// Filtered tree containing ONLY matched files inside their ancestor folders.
    /// The TreeView binds here during a search so non-matched files/folders are
    /// completely hidden (not just collapsed). Reset to empty on clear.
    /// </summary>
    public ObservableCollection<SalesFileNode> FilteredSalesFileNodes
    {
        get => _filteredSalesFileNodes;
        private set => SetField(ref _filteredSalesFileNodes, value);
    }

    public int MatchCount
    {
        get => _matchCount;
        private set => SetField(ref _matchCount, value);
    }

    /// <summary>
    /// Short status string (e.g. "✓ Found 3 matches"). Bound to the badge label.
    /// </summary>
    public string MatchCountLabel
    {
        get => _matchCountLabel;
        private set => SetField(ref _matchCountLabel, value);
    }

    /// <summary>
    /// Runs a semantic search and updates <see cref="MatchCountLabel"/> /
    /// <see cref="MatchedFilePaths"/>. The caller is responsible for debouncing
    /// and stamping each call with a request version so a slow response cannot
    /// overwrite a newer one. The cancellation token is forwarded to the AI
    /// service.
    /// </summary>
    public async Task<SearchResult> PerformSemanticSearchAsync(string query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(query))
        {
            ClearSearch();
            return new SearchResult { SearchQuery = query, SearchExecutedAt = DateTime.Now };
        }

        var fileMetadata = BuildFileMetadataList();
        if (fileMetadata.Count == 0)
        {
            MatchCountLabel = "No data to search.";
            MatchCount = 0;
            return new SearchResult { SearchQuery = query, SearchExecutedAt = DateTime.Now };
        }

        var stopwatch = Stopwatch.StartNew();
        var matchedPaths = await _semanticSearchService.SearchFilesSemanticAsync(query, fileMetadata, cancellationToken);
        stopwatch.Stop();

        cancellationToken.ThrowIfCancellationRequested();

        var result = new SearchResult
        {
            SearchQuery = query,
            SearchExecutedAt = DateTime.Now,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        };

        foreach (var path in matchedPaths)
        {
            result.Matches.Add(new SearchResult.MatchedNode
            {
                FilePath = path,
                FileName = ExtractFileName(path)
            });
        }

        MatchCount = result.TotalMatches;
        MatchCountLabel = MatchCount switch
        {
            0 => "No matches found.",
            1 => "✓ Found match in 1 file",
            _ => $"✓ Found {MatchCount} matches in {MatchCount} files"
        };

        return result;
    }

    /// <summary>
    /// Resets the tree and search state.
    /// </summary>
    public void ClearSearch()
    {
        MatchCount = 0;
        MatchCountLabel = string.Empty;
        ClearAllHighlights();
        FilteredSalesFileNodes = new ObservableCollection<SalesFileNode>();
    }

    /// <summary>
    /// Applies the search result to the file tree: marks matched files and
    /// their ancestor folders, then builds a filtered tree containing only the
    /// matched files inside their ancestor folders.
    /// </summary>
    public void ExpandAndHighlightMatches(SearchResult result)
    {
        ClearAllHighlights();

        if (result?.Matches is null || result.Matches.Count == 0)
        {
            FilteredSalesFileNodes = new ObservableCollection<SalesFileNode>();
            return;
        }

        var matchedPaths = result.Matches
            .Where(match => !string.IsNullOrWhiteSpace(match.FilePath))
            .Select(match => match.FilePath.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        MarkNodesRecursive(SalesFileNodes, matchedPaths);
        FilteredSalesFileNodes = BuildFilteredTree(SalesFileNodes, matchedPaths);
    }

    private static void MarkNodesRecursive(IEnumerable<SalesFileNode> nodes, HashSet<string> matchedPaths)
    {
        foreach (var node in nodes)
        {
            if (string.IsNullOrWhiteSpace(node.SearchPath))
            {
                continue;
            }

            var isExactMatch = matchedPaths.Contains(node.SearchPath);
            var hasMatchingDescendant = matchedPaths.Any(path =>
                path.StartsWith(node.SearchPath + "/", StringComparison.OrdinalIgnoreCase));

            if (isExactMatch)
            {
                node.IsMatched = true;
                node.IsHighlighted = true;
                node.IsExpanded = true;
            }
            else if (hasMatchingDescendant)
            {
                node.IsHighlighted = true;
                node.IsExpanded = true;
            }
            else
            {
                node.IsExpanded = false;
            }

            if (node.SubFiles?.Count > 0)
            {
                MarkNodesRecursive(node.SubFiles.OfType<SalesFileNode>(), matchedPaths);
            }
        }
    }

    private static ObservableCollection<SalesFileNode> BuildFilteredTree(
        IEnumerable<SalesFileNode> sourceNodes,
        HashSet<string> matchedPaths)
    {
        var filteredRoot = new ObservableCollection<SalesFileNode>();

        foreach (var node in sourceNodes)
        {
            var filtered = BuildFilteredNode(node, matchedPaths);
            if (filtered is not null)
            {
                filteredRoot.Add(filtered);
            }
        }

        return filteredRoot;
    }

    private static SalesFileNode? BuildFilteredNode(SalesFileNode node, HashSet<string> matchedPaths)
    {
        if (string.IsNullOrWhiteSpace(node.SearchPath))
        {
            return null;
        }

        var isExactMatch = matchedPaths.Contains(node.SearchPath);
        var hasMatchingDescendant = matchedPaths.Any(path =>
            path.StartsWith(node.SearchPath + "/", StringComparison.OrdinalIgnoreCase));

        if (isExactMatch)
        {
            return CloneNode(node);
        }

        if (hasMatchingDescendant)
        {
            var filteredChildren = new ObservableCollection<FileManager>();
            foreach (var child in node.SubFiles?.OfType<SalesFileNode>() ?? Enumerable.Empty<SalesFileNode>())
            {
                var collected = BuildFilteredNode(child, matchedPaths);
                if (collected is not null)
                {
                    filteredChildren.Add(collected);
                }
            }

            if (filteredChildren.Count == 0)
            {
                return null;
            }

            var folder = CloneNode(node);
            folder.SubFiles = filteredChildren;
            folder.IsExpanded = true;
            folder.IsHighlighted = true;
            return folder;
        }

        return null;
    }

    private static SalesFileNode CloneNode(SalesFileNode node)
    {
        return new SalesFileNode
        {
            ItemName = node.ItemName,
            ImageIcon = node.ImageIcon,
            SearchPath = node.SearchPath,
            FileType = node.FileType,
            Keywords = node.Keywords?.ToArray() ?? Array.Empty<string>(),
            ClientNames = node.ClientNames?.ToArray() ?? Array.Empty<string>(),
            Quarter = node.Quarter,
            InvoiceAmount = node.InvoiceAmount,
            CreatedDate = node.CreatedDate,
            ModifiedDate = node.ModifiedDate,
            IsMatched = node.IsMatched,
            IsHighlighted = node.IsHighlighted,
            IsExpanded = node.IsExpanded,
            SubFiles = new ObservableCollection<FileManager>()
        };
    }

    private void ClearAllHighlights()
    {
        void ClearRecursive(IEnumerable<SalesFileNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.IsMatched = false;
                node.IsHighlighted = false;
                node.IsExpanded = false;

                if (node.SubFiles?.Count > 0)
                {
                    ClearRecursive(node.SubFiles.OfType<SalesFileNode>());
                }
            }
        }

        ClearRecursive(SalesFileNodes);
    }

    private List<FileMetadataContext> BuildFileMetadataList()
    {
        var metadata = new List<FileMetadataContext>();

        void CollectMetadata(IEnumerable<SalesFileNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.FileType != "Folder")
                {
                    metadata.Add(new FileMetadataContext
                    {
                        FullPath = node.SearchPath,
                        FileName = node.ItemName,
                        CreatedDate = node.CreatedDate,
                        ModifiedDate = node.ModifiedDate,
                        FileType = node.FileType ?? string.Empty,
                        Keywords = node.Keywords ?? Array.Empty<string>(),
                        RelatedClients = node.ClientNames ?? Array.Empty<string>(),
                        Quarter = node.Quarter ?? string.Empty,
                        Amount = node.InvoiceAmount
                    });
                }

                if (node.SubFiles?.Count > 0)
                {
                    CollectMetadata(node.SubFiles.OfType<SalesFileNode>());
                }
            }
        }

        CollectMetadata(SalesFileNodes);
        return metadata;
    }

    private static string ExtractFileName(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return string.Empty;
        }

        var segments = fullPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0 ? segments[segments.Length - 1] : fullPath;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}