namespace AIChatSample;

public partial class AgentLogoPage : ContentPage
{
    private AgentLogoPopupViewModel _vm;

    public AgentLogoPage() : this(null) { }

    public AgentLogoPage(AgentLogoPopupViewModel? viewModel)
    {
        InitializeComponent();
        _vm = viewModel ?? new AgentLogoPopupViewModel();
        if (BindingContext is null) BindingContext = _vm;
        _vm.RequestClose += OnRequestClose;
        _vm.ResetForOpen();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is AgentLogoPopupViewModel incoming && incoming != _vm)
        {
            _vm.RequestClose -= OnRequestClose;
            _vm = incoming;
            _vm.RequestClose += OnRequestClose;
            _vm.ResetForOpen();
        }
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        LogoPopup.IsOpen = false;
        if (Navigation?.ModalStack.Contains(this) == true)
        {
            Navigation.PopModalAsync(animated: true);
        }
    }
}
