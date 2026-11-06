using System.ComponentModel;
using System.Linq;
using Syncfusion.Maui.Core;
using WardrobeAI.ViewModels;

namespace WardrobeAI.Views.OutfitPlanner;

/// <summary>
/// Outfit Planner page (SDS Modules 4 &amp; 5). Renders the
/// <c>/designs/outfit_planner.html</c> mockup sections: top app bar, header,
/// occasion chips (Work / Meeting / Casual / Party), live weather context,
/// AI-generated outfit hero with title + subtitle + Generate action, four
/// radial score gauges (Match / Style / Comfort / Trend), Alternative Looks
/// strip, and the "Why this works" glassmorphic explanation. Plus the FAB
/// that opens the stand-alone Plan popup.
///
/// <para>The page calls <c>LoadContextAsync</c> on every <c>OnAppearing</c>;
/// the VM coalesces concurrent calls via <c>IsGenerating</c> and reuses
/// the last AI plan across tab switches unless the user taps Generate
/// explicitly.</para>
/// </summary>
public partial class OutfitPlannerPage : ContentPage
{
    private readonly OutfitPlannerViewModel _viewModel;

    public OutfitPlannerPage(OutfitPlannerViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Subscribe to IsGenerating so the dot-wave animation stops the moment
        // the AI call completes; the XAML wrapper's IsVisible already hides
        // the indicator visually, but we still want to release the repeat-
        // forever timeline so it does not keep ticking on invisible views.
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        StartCuratingWave();
        // Refreshing is cheap when we already have an outfit (no AI re-call),
        // so we just ask for the latest state on every appearance.
        await _viewModel.LoadContextAsync(forceRegenerate: false);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        StopCuratingWave();
    }

    /// <summary>
    /// Triggered when the user picks an occasion chip from the <c>SfChipGroup</c>.
    /// We re-fire <c>SelectOccasionCommand</c> (CommunityToolkit.Mvvm's
    /// source generator strips the <c>Async</c> suffix from
    /// <c>SelectOccasionAsync(string)</c>) with the chip's <c>Label</c> so the
    /// AI plan regenerates for the new category exactly like the per-chip
    /// <c>Command</c> wiring used to. The bound <c>SelectedItem</c> has already
    /// updated <c>ActiveCategory</c> on the VM by the time we get here, so the
    /// gauges / hero / alternatives flip immediately; the AI call is the
    /// long-running part that fills in the new scores.
    /// </summary>
    private void OnOccasionChipSelectionChanged(object sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        if (sender is not SfChipGroup group || group.SelectedItem is not OutfitPlannerViewModel.OccasionChip chip)
        {
            return;
        }
        if (_viewModel.SelectOccasionCommand.CanExecute(chip.Label))
        {
            _viewModel.SelectOccasionCommand.Execute(chip.Label);
        }
    }

    // --- Curating-outfit wave loader ---------------------------------------
    // The XAML places three small Ellipses (Dot0..Dot2) next to the
    // "Curating outfit" label; StartCuratingWave translates each dot up and
    // back down on a shared 1s timeline, staggered by ~150ms per dot, to
    // produce the wave motion. The animation is started on OnAppearing and
    // reset/cleared whenever IsGenerating changes so we do not leak a
    // repeat-forever timeline across back-to-back generations.
    private Animation? _waveAnimation;
    private bool _isWaveRunning;

    /// <summary>
    /// Toggle the dot-wave animation in lock-step with the VM's
    /// <c>IsGenerating</c> property so it only ticks while the wrapper is
    /// actually visible.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(OutfitPlannerViewModel.IsGenerating))
        {
            return;
        }

        if (_viewModel.IsGenerating)
        {
            StartCuratingWave();
        }
        else
        {
            StopCuratingWave();
        }
    }

    /// <summary>
    /// Build and start the three-dot wave loop. No-ops if the wrapper has
    /// not been inflated yet (e.g. before the page's first measure pass).
    /// </summary>
    private void StartCuratingWave()
    {
        if (CuratingDots is null)
        {
            return;
        }

        // Always reset translation so a restarted wave starts from neutral.
        foreach (var child in CuratingDots.Children.OfType<View>())
        {
            child.TranslationY = 0d;
        }

        StopCuratingWave();

        var dots = CuratingDots.Children.OfType<View>().ToList();
        if (dots.Count == 0)
        {
            return;
        }

        const double waveAmplitude = 8d;   // translation in device-independent pixels
        const double step = 0.15;          // 150 ms stagger per dot on a 1s timeline
        const double pad = 0.15;           // initial delay so dot0 leads
        double beginAt = pad;

        _waveAnimation = new Animation();

        foreach (var dot in dots)
        {
            // Phase 1: dot bobs up to -waveAmplitude.
            var up = new Animation(v => dot.TranslationY = -waveAmplitude * v,
                                    start: 0d, end: 1d);

            // Phase 2: dot eases back down to neutral.
            var down = new Animation(v => dot.TranslationY = -waveAmplitude * (1d - v),
                                     start: 0d, end: 1d);

            _waveAnimation.Add(beginAt: beginAt, duration: 0.35, animation: up);
            _waveAnimation.Add(beginAt: beginAt + 0.35, duration: 0.65, animation: down);

            beginAt += step;
        }

        _isWaveRunning = true;
        this.Animate("CuratingWave", _waveAnimation, length: 1000, repeat: () => _isWaveRunning);
    }

    /// <summary>
    /// Cancel the wave loop and reset dot translations so the dots return
    /// to their resting position when the AI call completes.
    /// </summary>
    private void StopCuratingWave()
    {
        _isWaveRunning = false;
        this.AbortAnimation("CuratingWave");

        if (CuratingDots is not null)
        {
            foreach (var child in CuratingDots.Children.OfType<View>())
            {
                child.TranslationY = 0d;
            }
        }

        _waveAnimation = null;
    }
}
