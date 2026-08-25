namespace SmartDataFormSample
{
    using SampleBrowser.Maui.Base;

    public partial class AIDataForm : ContentPage
    {
        public AIDataForm()
        {
            InitializeComponent();
#if ANDROID || IOS
            this.Content = new DataFormMobileUI();
#else
            this.Content = new DataFormDesktopUI();
#endif
        }
    }
}
