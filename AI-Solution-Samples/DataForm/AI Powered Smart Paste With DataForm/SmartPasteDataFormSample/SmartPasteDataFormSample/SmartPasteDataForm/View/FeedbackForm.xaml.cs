namespace SmartPasteDataFormSample
{
    using SampleBrowser.Maui.Base;

    public partial class FeedbackForm : ContentPage
    {
        public FeedbackForm()
        {
#if ANDROID || IOS
            this.Content = new FeedbackFormMobileView();
#else
            this.Content = new FeedbackFormDesktopView();
#endif
        }
    }
}
