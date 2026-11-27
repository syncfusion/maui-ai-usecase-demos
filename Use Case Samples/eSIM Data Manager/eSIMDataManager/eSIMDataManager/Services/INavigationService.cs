namespace eSIMDataManager.Services;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task NavigateToDashboardAsync();
    Task NavigateToProfilesAsync();
    Task NavigateToStoreAsync();
}
