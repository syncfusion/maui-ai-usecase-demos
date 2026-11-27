namespace eSIMDataManager.Services;

public class NavigationService : INavigationService
{
    public async Task GoToAsync(string route)
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(route);
        }
    }

    public Task NavigateToDashboardAsync() => GoToAsync("//dashboard");
    public Task NavigateToProfilesAsync() => GoToAsync("//profiles");
    public Task NavigateToStoreAsync() => GoToAsync("//store");
}
