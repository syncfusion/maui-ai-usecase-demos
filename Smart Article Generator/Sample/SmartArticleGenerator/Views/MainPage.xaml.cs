using Microsoft.Maui.Controls;
using Syncfusion.Maui.AIAssistView;

namespace SmartArticleGenerator
{
    /// <summary>
    /// Main page hosting the two-column layout and the custom AI AssistView.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Custom chat implementation used by <see cref="CustomAssistView"/> to render the assistant panel.
    /// </summary>
    public partial class CustomAssistViewChat : AssistViewChat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAssistViewChat"/> class.
        /// </summary>
        /// <param name="aIAssistView">The owning <see cref="Syncfusion.Maui.AIAssistView.SfAIAssistView"/> instance.</param>
        public CustomAssistViewChat(Syncfusion.Maui.AIAssistView.SfAIAssistView aIAssistView) : base(aIAssistView)
        {
        }
    }

    /// <summary>
    /// Custom wrapper over <see cref="Syncfusion.Maui.AIAssistView.SfAIAssistView"/> to expose the created
    /// AssistChatView instance for binding in the control template.
    /// </summary>
    public partial class CustomAssistView : Syncfusion.Maui.AIAssistView.SfAIAssistView
    {
        #region Properties

        /// <summary>
        /// Bindable property backing store for <see cref="AssistChatView"/>.
        /// </summary>
        public static readonly BindableProperty AssistChatViewProperty =
            BindableProperty.Create(nameof(AssistChatView), typeof(CustomAssistViewChat), typeof(CustomAssistView));

        /// <summary>
        /// Gets or sets the created chat view instance for use in the right panel.
        /// </summary>
        public CustomAssistViewChat AssistChatView
        {
            get { return (CustomAssistViewChat)this.GetValue(AssistChatViewProperty); }
            set { this.SetValue(AssistChatViewProperty, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the chat instance and stores it so it can be displayed in the template's right column.
        /// </summary>
        /// <returns>The created <see cref="AssistViewChat"/> instance.</returns>
        protected override AssistViewChat CreateAssistChat()
        {
            AssistChatView = new CustomAssistViewChat(this);
            return AssistChatView;
        }

        #endregion
    }
}
