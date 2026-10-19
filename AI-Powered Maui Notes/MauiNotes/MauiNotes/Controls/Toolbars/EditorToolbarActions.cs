using Syncfusion.Maui.RichTextEditor;
using CustomRte = CustomRichTextEditor.CustomRichTextEditor;

namespace MauiNotes.Controls.Toolbars;

public static class EditorToolbarActions
{
    public static async Task InsertHyperlinkAsync(ContentPage page, CustomRte editor)
    {
        string? displayText = await page.DisplayPromptAsync(
            "Insert Link",
            "Text to display",
            placeholder: "Link text");
        if (string.IsNullOrWhiteSpace(displayText))
            return;

        string? url = await page.DisplayPromptAsync(
            "Insert Link",
            "Web address",
            placeholder: "https://example.com");
        if (string.IsNullOrWhiteSpace(url))
            return;

        url = url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            url = $"https://{url}";
        else if (uri.Scheme is not "http" and not "https")
        {
            await page.DisplayAlertAsync("Invalid link", "Enter an http or https address.", "OK");
            return;
        }

        editor.InsertHyperlink(displayText.Trim(), url);
    }

    public static async Task InsertImageAttachmentAsync(ContentPage page, CustomRte editor)
    {
        try
        {
            FileResult? file = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Choose an image",
                FileTypes = FilePickerFileType.Images
            });
            if (file is null)
                return;

            byte[] imageBytes;
            await using (Stream input = await file.OpenReadAsync())
            using (var buffer = new MemoryStream())
            {
                await input.CopyToAsync(buffer);
                imageBytes = buffer.ToArray();
            }

            editor.InsertImage(new RichTextEditorImageSource
            {
                ImageFormat = RichTextEditorImageFormat.Base64,
                Source = ImageSource.FromStream(() => new MemoryStream(imageBytes)),
                Width = 600,
                Height = 400
            });
        }
        catch (Exception)
        {
            await page.DisplayAlertAsync("Unable to add image", "The selected image could not be inserted.", "OK");
        }
    }

    public static void InsertTable(CustomRte editor) => editor.InsertTable(2, 2);
}