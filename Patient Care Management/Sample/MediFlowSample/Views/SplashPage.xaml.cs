using MediFlowSample.ViewModels;
using Syncfusion.Maui.ProgressBar;

namespace MediFlowSample.Views;

public partial class SplashPage : ContentPage
{
	private SplashPageViewModel? _viewModel;
	private SfLinearProgressBar? _loadingProgress;

	public SplashPage()
	{
		InitializeComponent();
		_viewModel = new SplashPageViewModel();
		BindingContext = _viewModel;
		
		// Find the progress bar control
		_loadingProgress = this.FindByName<SfLinearProgressBar>("LoadingProgress");
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		// Animate the progress bar with stepped updates
		await RunSplashSequenceAsync();
	}

	private async Task RunSplashSequenceAsync()
	{
		try
		{
			if (_loadingProgress != null)
			{
				// Step 1: 30% progress
				_loadingProgress.Progress = 30;
				await Task.Delay(900);

				// Step 2: 60% progress
				_loadingProgress.Progress = 60;
				await Task.Delay(600);

				// Step 3: 80% progress
				_loadingProgress.Progress = 80;
				await Task.Delay(600);

				// Step 4: 100% progress (complete)
				_loadingProgress.Progress = 100;
				await Task.Delay(900);

				// Navigate to dashboard
				await Shell.Current.GoToAsync("//dashboard");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Animation error: {ex.Message}");
		}
	}
}
