using OpenAI.Chat;
using System.Text;

namespace NutriLens;

public partial class MainPage : ContentPage
{
    private string _selectedFile = string.Empty;

    private const string Endpoint =
        "https://your-openai-resource.openai.azure.com/";

    private const string ApiKey =
        "your-azure-openai-api-key";

    private const string DeploymentName =
        "your-DeploymentName";

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnSelectImageClicked(object sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Image"
            });

            if (file == null)
                return;

            _selectedFile = file.FullPath;

            PreviewImage.Source =
                ImageSource.FromFile(_selectedFile);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnExtractContentClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_selectedFile))
            {
                await DisplayAlert(
                    "Image Required",
                    "Select image first.",
                    "OK");

                return;
            }

            ResultEditor.Text = "Processing...";

            byte[] imageBytes =
                await File.ReadAllBytesAsync(_selectedFile);

            string base64 =
                Convert.ToBase64String(imageBytes);

            string extension =
                Path.GetExtension(_selectedFile)
                .TrimStart('.')
                .ToLower();

            if (extension == "jpg")
                extension = "jpeg";

            string imageUrl =
                $"data:image/{extension};base64,{base64}";

            var options = new OpenAI.OpenAIClientOptions()
            {
                Endpoint = new Uri(Endpoint)
            };

            var client = new OpenAI.OpenAIClient(
                new System.ClientModel.ApiKeyCredential(ApiKey),
                options);

            ChatClient chatClient =
                client.GetChatClient(DeploymentName);

            var messages = new List<ChatMessage>
            {
                new UserChatMessage(
                [
                    ChatMessageContentPart.CreateTextPart(
                        """
                        Extract all text from this image.
                        Preserve formatting where possible.

                        If this is a document, table, invoice, receipt,
                        math formula, handwritten note, or screenshot,
                        extract everything accurately.
                        """
                    ),

                    ChatMessageContentPart.CreateImagePart(
                        new Uri(imageUrl)
                    )
                ])
            };

            ChatCompletion completion =
                await chatClient.CompleteChatAsync(messages);

            ResultEditor.Text =
                completion.Content[0].Text;
        }
        catch (Exception ex)
        {
            ResultEditor.Text = ex.ToString();
        }
    }
}