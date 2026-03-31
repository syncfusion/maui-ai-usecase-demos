namespace SmartArticleGenerator
{
    /// <summary>
    /// Represents a research resource/source.
    /// </summary>
    public class ResourceItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the resource title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source type (e.g., "Article", "Research Paper", "Website", "Book").
        /// </summary>
        public string SourceType { get; set; } = "Article";

        /// <summary>
        /// Gets or sets the relevance score (0-100).
        /// </summary>
        public int RelevanceScore { get; set; } = 85;

        /// <summary>
        /// Gets or sets the resource URL.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author/source name.
        /// </summary>
        public string Author { get; set; } = "Unknown";

        /// <summary>
        /// Gets or sets a value indicating whether the resource is selected.
        /// </summary>
        public bool IsSelected { get; set; } = false;

        /// <summary>
        /// Gets or sets the icon/emoji for the resource type.
        /// </summary>
        public string Icon { get; set; } = "📄";

        #endregion
    }
}
