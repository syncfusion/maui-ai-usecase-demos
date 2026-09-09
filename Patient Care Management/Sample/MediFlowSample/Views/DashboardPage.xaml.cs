using MediFlowSample.Services;
using MediFlowSample.ViewModels;

namespace MediFlowSample.Views;

public partial class DashboardPage : ContentPage, IQueryAttributable
{
	public DashboardPage()
	{
		InitializeComponent();
		var services = Application.Current?.Handler?.MauiContext?.Services;
		var dataService = services?.GetService<DashboardDataService>()
			?? CreateFallbackDataService();
		BindingContext = new DashboardViewModel(dataService);
	}

	private static DashboardDataService CreateFallbackDataService()
	{
		var patientDataService = new PatientDataService();
		return new DashboardDataService(
			new ScheduleDataService(patientDataService),
			new NotificationService(),
			patientDataService);
	}

	private void OnViewAllClicked(object? sender, EventArgs e)
	{
		if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone || DeviceInfo.Current.Idiom == DeviceIdiom.Tablet)
		{
			MobileAppointmentsSheet.IsVisible = true;
			return;
		}

		AppointmentsPopup.IsOpen = true;
	}

	private void OnNotificationsTapped(object? sender, TappedEventArgs e)
	{
		OpenNotifications();
	}

	private void OpenNotifications()
	{
		if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone || DeviceInfo.Current.Idiom == DeviceIdiom.Tablet)
		{
			MobileCareAlertsSheet.IsVisible = true;
			return;
		}

		NotificationsPopup.IsOpen = true;
	}

	private void OnCloseAppointmentsPopupTapped(object? sender, TappedEventArgs e)
	{
		AppointmentsPopup.IsOpen = false;
		MobileAppointmentsSheet.IsVisible = false;
	}

	private void OnCloseAppointmentsPopupClicked(object? sender, EventArgs e)
	{
		AppointmentsPopup.IsOpen = false;
		MobileAppointmentsSheet.IsVisible = false;
	}

	private void OnCloseNotificationsPopupTapped(object? sender, TappedEventArgs e)
	{
		NotificationsPopup.IsOpen = false;
		MobileCareAlertsSheet.IsVisible = false;
	}

	private void OnCloseNotificationsPopupClicked(object? sender, EventArgs e)
	{
		NotificationsPopup.IsOpen = false;
		MobileCareAlertsSheet.IsVisible = false;
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("showNotifications", out var value)
			&& string.Equals(value?.ToString(), "true", StringComparison.OrdinalIgnoreCase))
		{
			OpenNotifications();
		}
	}
}
