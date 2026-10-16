using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIFileSearchTreeView
{
    /// <summary>
    /// Interface for semantic search service that performs AI-powered searches on file metadata.
    /// </summary>
    public interface ISemanticSearchService
    {
        /// <summary>
        /// Gets a value indicating whether the Azure OpenAI credentials are valid.
        /// </summary>
        bool IsCredentialValid { get; }

        /// <summary>
        /// Searches for files based on a semantic query.
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="fileMetadata">List of file metadata to search through</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of matching file paths</returns>
        Task<List<string>> SearchFilesSemanticAsync(
            string query,
            List<FileMetadataContext> fileMetadata,
            CancellationToken cancellationToken = default);
    }
}
