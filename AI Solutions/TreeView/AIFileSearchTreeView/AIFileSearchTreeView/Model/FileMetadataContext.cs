using System;

namespace AIFileSearchTreeView
{
    /// <summary>
    /// Metadata context for files used by AI semantic search
    /// </summary>
    public class FileMetadataContext
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string FileType { get; set; }
        public string[] Keywords { get; set; }
        public string[] RelatedClients { get; set; }
        public string Quarter { get; set; }
        public decimal? Amount { get; set; }
    }
}
