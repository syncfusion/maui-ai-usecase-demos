using SmartArticleGenerator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleGenerationSampl
{
    /// <summary>
    /// Service for managing research resources.
    /// </summary>
    public class ResourcesService
    {
        // Backing store for the resources list used across the app.
        private readonly ObservableCollection<ResourceItem> resources = new();

        public ResourcesService()
        {
            InitializeDefaultResources();
        }

        /// <summary>
        /// Get all resources
        /// </summary>
        public ObservableCollection<ResourceItem> GetResources()
        {
            return resources;
        }

        /// <summary>
        /// Add a new resource
        /// </summary>
        public void AddResource(ResourceItem resource)
        {
            resource.Id = resources.Count + 1;
            resources.Add(resource);
        }

        /// <summary>
        /// Remove a resource
        /// </summary>
        public void RemoveResource(ResourceItem resource)
        {
            resources.Remove(resource);
        }

        /// <summary>
        /// Clear all resources
        /// </summary>
        public void ClearResources()
        {
            resources.Clear();
        }

        /// <summary>
        /// Update resources based on query
        /// </summary>
        public void UpdateResources(List<ResourceItem> newResources)
        {
            resources.Clear();
            foreach (var resource in newResources)
            {
                resources.Add(resource);
            }
        }

        /// <summary>
        /// Initialize with default resources
        /// </summary>
        private void InitializeDefaultResources()
        {
            var defaultResources = new List<ResourceItem>
            {
                new ResourceItem
                {
                    Id = 1,
                    Title = "Syncfusion MAUI Documentation",
                    Description = "Official documentation for Syncfusion MAUI controls and components",
                    SourceType = "Documentation",
                    RelevanceScore = 95,
                    Author = "Syncfusion",
                    Icon = "📚",
                    Url = "https://www.syncfusion.com/maui-controls"
                },
                new ResourceItem
                {
                    Id = 2,
                    Title = "Getting Started with AI AssistView",
                    Description = "Complete guide to implementing AI features in MAUI applications",
                    SourceType = "Tutorial",
                    RelevanceScore = 88,
                    Author = "Syncfusion Team",
                    Icon = "🎓",
                    Url = "https://www.syncfusion.com/maui-controls/maui-aiassistview"
                },
                new ResourceItem
                {
                    Id = 3,
                    Title = "MAUI Best Practices",
                    Description = "Learn best practices for building high-performance MAUI applications",
                    SourceType = "Guide",
                    RelevanceScore = 82,
                    Author = "Microsoft",
                    Icon = "💼",
                    Url = "https://learn.microsoft.com/en-us/dotnet/maui/"
                }
            };

            UpdateResources(defaultResources);
        }

        /// <summary>
        /// Search resources by keyword
        /// </summary>
        public List<ResourceItem> SearchResources(string keyword)
        {
            return resources
                .Where(r => r.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                           r.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.RelevanceScore)
                .ToList();
        }
    }
}
