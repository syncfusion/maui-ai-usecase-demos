using System.Collections.ObjectModel;

namespace AIFileSearchTreeView;

/// <summary>
/// Extended <see cref="FileManager"/> for sales-related files with AI search
/// metadata (keywords, dates, invoice amount, and search-match flags).
/// </summary>
public class SalesFileNode : FileManager
{
    private DateTime _createdDate;
    private DateTime _modifiedDate;
    private string? _fileType;
    private string[]? _keywords;
    private string[]? _clientNames;
    private string? _quarter;
    private decimal? _invoiceAmount;
    private bool _isMatched;
    private bool _isHighlighted;
    private bool _isExpanded;
    private string _searchPath = string.Empty;

    public DateTime CreatedDate
    {
        get => _createdDate;
        set => SetField(ref _createdDate, value);
    }

    public DateTime ModifiedDate
    {
        get => _modifiedDate;
        set => SetField(ref _modifiedDate, value);
    }

    public string? FileType
    {
        get => _fileType;
        set => SetField(ref _fileType, value);
    }

    public string[]? Keywords
    {
        get => _keywords;
        set => SetField(ref _keywords, value);
    }

    public string[]? ClientNames
    {
        get => _clientNames;
        set => SetField(ref _clientNames, value);
    }

    public string? Quarter
    {
        get => _quarter;
        set => SetField(ref _quarter, value);
    }

    public decimal? InvoiceAmount
    {
        get => _invoiceAmount;
        set => SetField(ref _invoiceAmount, value);
    }

    public bool IsMatched
    {
        get => _isMatched;
        set => SetField(ref _isMatched, value);
    }

    public bool IsHighlighted
    {
        get => _isHighlighted;
        set => SetField(ref _isHighlighted, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public string SearchPath
    {
        get => _searchPath;
        set => SetField(ref _searchPath, value);
    }

    public SalesFileNode()
    {
        CreatedDate = DateTime.Now;
        ModifiedDate = DateTime.Now;
        FileType = string.Empty;
        Keywords = Array.Empty<string>();
        ClientNames = Array.Empty<string>();
        Quarter = string.Empty;
    }
}