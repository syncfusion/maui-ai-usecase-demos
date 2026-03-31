using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AIPoweredWritingAssistant
{
    /// <summary>
    /// Class representing the view model for a combo box. It manages a collection of features that can be displayed in the combo box.
    /// </summary>
    public class ComboBoxViewModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the collection of features associated with the current instance.
        /// </summary>
        public ObservableCollection<Feature> Features { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the ComboBoxViewModel class with a predefined set of features.
        /// </summary>
        public ComboBoxViewModel()
        {
            this.Features = new ObservableCollection<Feature>();
            this.Features.Add(new Feature() { FeatureName = "Paraphraser" });
            this.Features.Add(new Feature() { FeatureName = "Grammar Checker" });
            this.Features.Add(new Feature() { FeatureName = "Elaborate" });
            this.Features.Add(new Feature() { FeatureName = "Shorten" });
        }

        #endregion
    }
}
