using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArticleGenerationSample
{
    /// <summary>
    /// Base ViewModel class implementing INotifyPropertyChanged
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Field to store the IsBusy property value
        /// </summary>
        private bool _isBusy = false;

        /// <summary>
        /// Field backing the Title property
        /// </summary>
        private string _title = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the ViewModel is busy
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raise property changed event
        /// </summary>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}
