using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartArticleGenerator
{
    /// <summary>
    /// Represents a structured section of an AI response
    /// </summary>
    public class ResponseSection
    {
        #region Properties

        /// <summary>
        /// Gets or sets the section heading
        /// </summary>
        public string Heading { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the section content (HTML formatted)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the section type (e.g., "Introduction", "Details", "Conclusion", "Points")
        /// </summary>
        public string SectionType { get; set; } = "Details";

        /// <summary>
        /// Gets or sets the order in which sections should be displayed
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets whether the section is expanded
        /// </summary>
        public bool IsExpanded { get; set; } = true;

        #endregion
    }
}
