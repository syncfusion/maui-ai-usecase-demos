using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class ScanIngredientsPage : ContentPage
{
    private CancellationTokenSource? animationCancellation;
    private ScanIngredientsViewModel? viewModel;

    public ScanIngredientsPage()
    {
        InitializeComponent();
        BindingContext = new ScanIngredientsViewModel();
    }

    public ScanIngredientsPage(ScanIngredientsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ScanIngredientsViewModel vm)
        {
            viewModel = vm;
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }

        if (viewModel is null || !viewModel.IsAnalyzing)
        {
            StartScanAnimation();
        }
    }

    protected override void OnDisappearing()
    {
        StopScanAnimation();
        //if (viewModel is not null)
        //{
        //    viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        //    viewModel = null;
        //}

        base.OnDisappearing();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScanIngredientsViewModel.IsAnalyzing))
        {
            if (viewModel?.IsAnalyzing == true)
            {
                StopScanAnimation();
            }
            else
            {
                StartScanAnimation();
            }
        }
    }

    private void StartScanAnimation()
    {
        if (animationCancellation is not null)
            return;

        animationCancellation = new CancellationTokenSource();
        _ = AnimateScanLineAsync(animationCancellation.Token);
    }

    private void StopScanAnimation()
    {
        animationCancellation?.Cancel();
        animationCancellation?.Dispose();
        animationCancellation = null;
    }

    private async Task AnimateScanLineAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (viewModel?.IsAnalyzing == true)
                {
                    await Task.Delay(150, cancellationToken);
                    continue;
                }

                ScanLine.TranslationY = -58;
                ScanLine.Opacity = 0;

                await ScanLine.FadeTo(0.7, 180, Easing.CubicIn);

                await ScanLine.TranslateTo(0, 58, 1500, Easing.SinInOut);

                await ScanLine.FadeTo(0, 180, Easing.CubicOut);

                await Task.Delay(250, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}