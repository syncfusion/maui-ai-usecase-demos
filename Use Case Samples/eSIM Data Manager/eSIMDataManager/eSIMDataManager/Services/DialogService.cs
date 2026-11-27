namespace eSIMDataManager.Services;

public class DialogService : IDialogService
{
    private static Page? CurrentPage
    {
        get
        {
            var window = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            return window?.Page;
        }
    }

    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        var page = CurrentPage;
        if (page is not null)
        {
            await page.DisplayAlertAsync(title, message, cancel);
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
    {
        var page = CurrentPage;
        if (page is not null)
        {
            return await page.DisplayAlertAsync(title, message, accept, cancel);
        }
        return false;
    }

    public async Task<string?> ShowActionSheetAsync(string title, string cancel, string? destruction, params string[] buttons)
    {
        var page = CurrentPage;
        if (page is not null)
        {
            return await page.DisplayActionSheetAsync(title, cancel, destruction, buttons);
        }
        return null;
    }
}
