using LLM_Routing.ViewModels;
using Syncfusion.Maui.AIAssistView;

namespace LLM_Routing
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _vm;

        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
            assistView.Request += OnAssistViewRequest;
            Unloaded += (_, _) => assistView.Request -= OnAssistViewRequest;
        }

        // Bound to the SfAIAssistView's Request event so we can clear the editor
        // *after* the view-model finishes the chat turn, but only on a real send.
        // SfAIAssistView raises this event both when the user taps the send button
        // and when the user picks a file from the attachment picker (the request
        // item is an AssistAttachmentItem in that case and has no text). We must
        // NOT clear attachments on a bare attachment pick — the file should stay
        // in the composer until the user actually types a message and sends.
        private async void OnAssistViewRequest(object? sender, RequestEventArgs e)
        {
            if (e.RequestItem is null)
                return;

            // Stop SfAIAssistView's default behavior; the view-model adds the request
            // item to the chat explicitly and we clear the editor ourselves.
            e.Handled = true;

            // A bare attachment selection (AssistAttachmentItem with no text) is not
            // a send — keep the attachment in the composer for the next real send.
            bool isAttachmentOnlyPick =
                e.RequestItem is AssistAttachmentItem &&
                string.IsNullOrWhiteSpace(e.RequestItem.Text);

            try
            {
                await _vm.HandleSendAsync(e.RequestItem);
            }
            finally
            {
                // Only clear pending attachments on a real send, regardless of
                // whether the API call succeeded. The request item itself is kept
                // in the chat as the user bubble. Clear the bound VM collection
                // (not assistView.Attachments) so the binding stays intact and
                // the chip area updates through the same source the picker writes to.
                if (!isAttachmentOnlyPick)
                    _vm.Attachments.Clear();
            }
        }
    }
}