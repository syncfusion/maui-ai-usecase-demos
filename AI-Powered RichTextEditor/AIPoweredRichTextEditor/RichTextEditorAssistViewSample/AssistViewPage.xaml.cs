namespace RichTextEditorAssistViewSample;

/// <summary>
/// Class representing the assist view page, which serves as a dedicated interface for displaying and interacting with the assist view features in the application.
/// </summary>
[QueryProperty(nameof(ViewModel), "vm")]
public partial class AssistViewPage : ContentPage
{
    #region Field

    /// <summary>
    /// Field to hold the ViewModel instance.
    /// </summary>
    private AssistViewViewModel? viewModel;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the AssistViewPage class.
    /// </summary>
    public AssistViewPage()
    {
        InitializeComponent();
        ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel();
        comboBox.ItemsSource = comboBoxViewModel.Features;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the view model associated with the assist view.
    /// </summary>
    public AssistViewViewModel? ViewModel
    {
        get => viewModel;
        set
        {
            viewModel = value;
            BindingContext = viewModel;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Method to handle selection changes in the combo box, updating the view model accordingly.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void comboBox_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        if (this.viewModel != null)
        {
            this.viewModel?.GetComboBoxSelection(e);
        }
    }

    #endregion
}