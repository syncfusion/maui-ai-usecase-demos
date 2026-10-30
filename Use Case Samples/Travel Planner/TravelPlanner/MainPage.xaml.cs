using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Syncfusion.Maui.AIAssistView;
using Syncfusion.Maui.Calendar;
using Syncfusion.Maui.Maps;
using Syncfusion.Maui.Toolkit.BottomSheet;
using Syncfusion.Maui.Toolkit.Chips;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using TravelPlanner.Models;
using TravelPlanner.ViewModels;

namespace TravelPlanner;

public partial class MainPage : ContentPage
{
    private readonly TravelPlannerViewModel _vm;
    private TravelPlanner.Services.OpenAIServices? _openAI;
    private bool _drawerOpen = false;
    private double AssistViewHeight;
    private readonly IKeyboardService? _keyboardService;

    private bool _isDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet;

    // Tracks whether we've disabled the custom composer after the first send
    private bool _composerDisabledAfterFirstSend;
    // Helpers to resolve device-specific elements (Phone vs Desktop)
    private SfMaps? MainMapControl => _isDesktop ? MainMapDesktop : MainMap;
    private Grid? RightDrawerControl => _isDesktop ? RightDrawerDesktop : RightDrawer;
    private Border? HamburgerFrameControl => _isDesktop ? HamburgerFrameDesktop : HamburgerFrame;
    private SfMaps? SummaryMapControl => _isDesktop ? SummaryMapDesktop : SummaryMap;
    private Grid? DatePopupOverlayControl => _isDesktop ? DatePopupOverlayDesktop : DatePopupOverlay;
    private SfCalendar? TripCalendarControl => _isDesktop ? TripCalendarDesktop : TripCalendar;

    // Helpers to resolve active MapTileLayer for main and summary maps
    private MapTileLayer? ActiveMainTileLayer
    {
        get
        {
            return _isDesktop ? MainMapDesktopLayer : MainMapLayer;
        }
    }

    private MapTileLayer? ActiveSummaryTileLayer
    {
        get
        {
            return _isDesktop ? SummaryMapDesktopLayer : SummaryMapLayer;
        }
    }

    private static string GetMarkerLabelText(object? source, string fallback, string? description = null, string? category = null)
    {
        if (source is MapMarkerLabel label)
        {
            if (!string.IsNullOrWhiteSpace(label.DisplayName))
                return label.DisplayName;
            if (!string.IsNullOrWhiteSpace(label.Name))
                return label.Name;
        }

        if (source is string s && !string.IsNullOrWhiteSpace(s))
            return s;

        var fromModel = MapMarkerLabel.From(source, fallback, description, category);
        return !string.IsNullOrWhiteSpace(fromModel.DisplayName) ? fromModel.DisplayName : fallback;
    }

    private MapMarkerLabel BuildMarkerLabel(object? source, string fallback, string? description = null, string? category = null)
    {
        var label = new MapMarkerLabel
        {
            Name = GetMarkerLabelText(source, fallback, description, category),
            Description = description ?? string.Empty,
            Category = category ?? fallback
        };

        if (source is not null)
        {
            var nameProp = source.GetType().GetProperty("Name");
            if (nameProp?.GetValue(source) is string name && !string.IsNullOrWhiteSpace(name))
                label.Name = name;

            var categoryProp = source.GetType().GetProperty("Category");
            if (categoryProp?.GetValue(source) is string cat && !string.IsNullOrWhiteSpace(cat))
                label.Category = cat;

            var addressProp = source.GetType().GetProperty("Address");
            if (addressProp?.GetValue(source) is string addr && !string.IsNullOrWhiteSpace(addr))
                label.Description = addr;
        }

        return label;
    }

    public double PixelWidth { get; private set; }
    public double pixelHeight;

    // Track the currently-added pin for a selected place so we can remove/update it
    private MapMarker? _selectedPlacePin;
    // Track the pin used for the device current location
    private MapMarker? _currentLocationMarker;

    public ICommand AssistRequestCommand { get; }

    // Parameterless constructor to support XAML instantiation (Shell DataTemplates)
    public MainPage() : this(ResolveViewModel()) { }

    private static TravelPlannerViewModel ResolveViewModel()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        if (services == null)
            throw new InvalidOperationException("Service provider is not available.");
        var vm = services.GetService<TravelPlannerViewModel>();
        if (vm == null)
            throw new InvalidOperationException("TravelPlannerViewModel is not registered.");
        return vm;
    }

    public MainPage(TravelPlannerViewModel vm)
    {
        // Initialize command BEFORE InitializeComponent so XAML binding has a valid command instance
        AssistRequestCommand = new Command<object>(async param => await ExecuteAssistRequestAsync(param));

        // Set BindingContext BEFORE InitializeComponent so bindings resolve during XAML load
        _vm = vm;
        BindingContext = _vm;

        // Compute pixel-based width BEFORE InitializeComponent so XAML binding
        // to the page instance can read the value during load.
        var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;
        PixelWidth = mainDisplayInfo.Width / mainDisplayInfo.Density;
        pixelHeight = mainDisplayInfo.Height / mainDisplayInfo.Density;

        InitializeComponent();
#if ANDROID
        if (Platform.CurrentActivity != null)
        {
            _keyboardService = new KeyboardService_Android(Platform.CurrentActivity);
            _keyboardService.KeyboardVisibilityChanged += OnKeyboardVisibilityChanged;
        }
#elif IOS
        _keyboardService = new KeyboardService_iOS();
        _keyboardService.KeyboardVisibilityChanged += OnKeyboardVisibilityChanged;
#endif
        // React to Destination changes to update the main map
        _vm.PropertyChanged += Vm_PropertyChanged;

        // React to TopPlaces changes to keep summary map in sync
        _vm.TopPlaces.CollectionChanged += OnTopPlacesChanged;

        // When the ViewModel resets trip state, clear view-only state (calendar selection,
        // map pins, interests picker selection).
        _vm.TripRestarted += OnVmTripRestarted;

        // Resolve OpenAI service from DI (fallback to direct instance if DI not ready)
        _openAI = Application.Current?.Handler?.MauiContext?.Services?.GetService<TravelPlanner.Services.OpenAIServices>()
                  ?? new TravelPlanner.Services.OpenAIServices();

        // Ensure drawer is initially closed and off-screen
        _drawerOpen = false;
        this.Loaded += async (_, __) =>
        {
            try
            {
                var isDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet;
                var width = isDesktop ? 600 : (this.Width > 0 ? this.Width : (this.Window?.Width ?? 320));
                if (_drawerOpen && RightDrawerControl != null)
                    RightDrawerControl.TranslationX = width; // place off-screen initially
                if (HamburgerFrameControl != null)
                    HamburgerFrameControl.IsVisible = true;

                // On desktop, start the conversation immediately in the desktop panel
                if (isDesktop)
                {
                    TryStartConversation();
                }
                // Try to obtain and show device current location on the main map
                try { await TryShowCurrentLocationAsync(); } catch { }
            }
            catch { }
        };

        // Animate floating button subtly using Dispatcher timer
        this.Dispatcher.StartTimer(TimeSpan.FromSeconds(2), () =>
        {
            if (AiButton != null)
            {
                _ = AiButton.ScaleToAsync(1.05, 250, Easing.CubicInOut)
                    .ContinueWith(_ => AiButton.ScaleToAsync(1.00, 250, Easing.CubicInOut));
            }
            return true;
        });

        // Ensure the conversation starts early
        TryStartConversation();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Enforce future-only dates (no today)
        try
        {
            if (TripCalendarControl != null)
                TripCalendarControl.MinimumDate = DateTime.Today.AddDays(1);
        }
        catch { }

        // Fallback: start the conversation if it hasn't yet
        TryStartConversation();

        // Ensure map and permissions are ready and show current location
        try { await TryShowCurrentLocationAsync(); } catch { }
    }

    private void OnTopPlacesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        try
        {
            // Ensure map updates happen on UI thread
            _ = Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(UpdateSummaryMapPinsAsync);
        }
        catch
        {
            // Fallback fire-and-forget
            _ = UpdateSummaryMapPinsAsync();
        }
    }

    private void OnVmTripRestarted(object? sender, EventArgs e)
    {
        try
        {
            TripCalendarControl?.SelectedDateRange = null;
            try { InterestsChipsDesktop.SelectedItem = null; } catch { }
            try { InterestsChips.SelectedItem = null; } catch { }

            // Clear maps
            try { ClearLayerMarkers(ActiveMainTileLayer); } catch { }
            try { ClearLayerMarkers(ActiveSummaryTileLayer); } catch { }

            // Restore the device pin
            _ = TryShowCurrentLocationAsync();
        }
        catch { }
    }

    private DataTemplate CreateDisabledEditorTemplate()
    {
        return new DataTemplate(() =>
        {
            // Editor
            var editor = new Editor
            {
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                FontSize = 16,
                Placeholder = "Type Message...",
                PlaceholderColor = Microsoft.Maui.Graphics.Color.FromArgb("#b7a27a"),
                TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#73623c"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(0),
                MaximumHeightRequest = 100
            };

            // Send button (wired but the whole template will be input-transparent)
            var sendButton = new Button
            {
                BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#73623c"),
                Padding = 8,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                FontSize = 18,
                FontFamily = "MauiSampleFontIcon",
                Text = "",
                TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#fff7e7"),
                CornerRadius = 18,
                WidthRequest = 40,
                HeightRequest = 40
            };
            try { sendButton.Command = AssistRequestCommand; sendButton.CommandParameter = editor; } catch { }

            var innerGrid = new Grid { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Fill };
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            innerGrid.Children.Add(editor);
            Grid.SetColumn(editor, 0);
            innerGrid.Children.Add(sendButton);
            Grid.SetColumn(sendButton, 1);

            var frame = new Frame
            {
                HasShadow = false,
                Padding = 8,
                CornerRadius = 20,
                BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#fff7e7"),
                BorderColor = Microsoft.Maui.Graphics.Color.FromArgb("#e0d2b8"),
                Content = innerGrid
            };

            // Disable interaction at the template level
            frame.InputTransparent = true;

            var root = new Grid { Padding = 8, InputTransparent = true };
            root.Children.Add(frame);
            return root;
        });
    }

    private async Task UpdateSummaryMapPinsAsync()
    {
        try
        {
            var layer = ActiveSummaryTileLayer;
            if (layer == null || _vm == null) return;

            // Clear existing markers
            ClearLayerMarkers(layer);

            var locs = new List<MapLatLng>();
            MapMarkerCollection mapMarkers = new MapMarkerCollection();
            foreach (var place in _vm.TopPlaces)
            {
                if (place == null) continue;
                Location? loc = null;
                if (place.Latitude.HasValue && place.Longitude.HasValue)
                {
                    loc = new Location(place.Latitude.Value, place.Longitude.Value);
                }
                else if (!string.IsNullOrWhiteSpace(place.Address))
                {
                    try
                    {
                        var results = await Geocoding.GetLocationsAsync(place.Address);
                        var first = results?.FirstOrDefault();
                        if (first != null)
                        {
                            loc = new Location(first.Latitude, first.Longitude);
                        }
                    }
                    catch { }
                }

                if (loc != null)
                {
                    locs.Add(new MapLatLng { Latitude = loc.Latitude, Longitude = loc.Longitude });
                    var markerLabel = BuildMarkerLabel(place, "Attraction", place?.Address, "Attraction");
                    var placedetails = new CustomMarker
                    {
                        Name = place!.Name,
                        Area = place.Address,
                        Latitude = loc.Latitude,
                        Longitude = loc.Longitude,
                    };
                    var marker = new MapMarker
                    {
                        Latitude = loc.Latitude,
                        Longitude = loc.Longitude,
                        BindingContext = placedetails
                    };
                    mapMarkers.Add(placedetails);
                }
            }
            layer.Markers = mapMarkers;

            // Adjust viewport
              if (locs.Count > 0)
            {
                var minLat = locs.Min(l => l.Latitude);
                var maxLat = locs.Max(l => l.Latitude);
                var minLon = locs.Min(l => l.Longitude);
                var maxLon = locs.Max(l => l.Longitude);

                layer.LatLngBounds = new MapLatLngBounds
                {
                    Northeast = new MapLatLng { Latitude = maxLat, Longitude = maxLon },
                    Southwest = new MapLatLng { Latitude = minLat, Longitude = minLon }
                };
                layer.ZoomPanBehavior.ZoomLevel = 12;
            }
        }
        catch { }
    }

    private void TryStartConversation()
    {
        if (_vm != null && _vm.AssistItems.Count == 0)
        {
            if (_vm.StartConversationCommand.CanExecute(null))
                _vm.StartConversationCommand.Execute(null);
        }
    }

    private async void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(TravelPlannerViewModel.Destination))
            {
                await UpdateMainMapForDestinationAsync();
            }
            else if (e.PropertyName == nameof(TravelPlannerViewModel.Step))
            {
                if (BindingContext is not TravelPlannerViewModel vm)
                    return;

                bool isDone = vm.Step == ConversationStep.Done;

                // Disable both mobile and desktop AssistViews once the plan is generated.
                if (AssistView != null)
                    AssistView.IsEnabled = !isDone;
                if (DesktopAssistView != null)
                    DesktopAssistView.IsEnabled = !isDone;

                if (isDone)
                {
                    // Open the right drawer to show the summary automatically
                    await EnsureDrawerOpenAsync();

                    // Update the summary map pins now that data is finalized
                    try { await UpdateSummaryMapPinsAsync(); } catch { }
                }
            }
        }
        catch { }
    }

    private void OnStateChanged(object sender, StateChangedEventArgs args)
    {
        Console.WriteLine($"Dynamic height while dragging: {args.NewState}");
        if (args.NewState == BottomSheetState.HalfExpanded)
        {
            // Hide the floating AI button while the sheet is half-expanded
            if (AiButton != null)
            {
                AiButton.TranslateToAsync(0, -400, 250, Easing.CubicOut);
            }
        }
        if (args.NewState == BottomSheetState.Hidden)
        {
            // Show the floating AI button again when the sheet is hidden
            if (AiButton != null)
            {
                AiButton.TranslateToAsync(0, -10, 250, Easing.CubicOut);
            }
        }
        if (args.NewState == BottomSheetState.Collapsed)
        {
            // Keep the AI button hidden when the sheet is collapsed (visible but not fully hidden)
            if (AiButton != null)
            {
                AiButton.TranslateToAsync(0, -100, 250, Easing.CubicOut);
            }
        }
        AssistViewHeight = BottomSheet.Bounds.Height;
        _ = UpdateSummaryMapPinsAsync();
    }

    private void OnAiButtonTapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        // Toggle bottom sheet
        BottomSheet.IsOpen = !BottomSheet.IsOpen;
        if (BottomSheet.IsOpen)
        {
            // Ensure conversation has started (mobile only)
            if (BindingContext is TravelPlannerViewModel vm && vm.AssistItems.Count == 0)
            {
                if (vm.StartConversationCommand.CanExecute(null))
                    vm.StartConversationCommand.Execute(null);
            }
            if (AiButton != null)
                {
                    _ = AiButton.TranslateToAsync(0, -400, 250, Easing.CubicOut);
                }
        }
        else
        {
            if (AiButton != null)
            {
                _ = AiButton.TranslateToAsync(0, -10, 250, Easing.CubicOut);
            }
        }
    }

    private async void OnAssistRequest(object sender, RequestEventArgs e)
    {
        await ExecuteAssistRequestAsync(null);
    }

    private void OnKeyboardVisibilityChanged(object? sender, bool isVisible)
    {
        if (!isVisible)
            return;
        // Expand the chat surface so the composer stays above the keyboard.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (BottomSheet != null)
                BottomSheet.State = BottomSheetState.FullExpanded;
        });
    }

    private async Task ExecuteAssistRequestAsync(object? parameter)
    {
        try
        {
            // Ensure conversation started before handling first request
            TryStartConversation();

            // Determine which AssistView to use based on platform
            var isDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet;
            var activeAssistView = isDesktop ? DesktopAssistView : AssistView;

            // If caller passed the Editor element as parameter, read text directly from it (reliable
            // even when template bindings haven't propagated); otherwise, fall back to AssistView.InputText.
            string? text = null;
            if (parameter is Microsoft.Maui.Controls.Editor paramEditor)
            {
                text = paramEditor.Text;
                _vm?.AssistItems.Add(new AssistItem { Text = text, IsRequested = true });
                paramEditor.Unfocus();
                // Clear the editor immediately (mirrors default send behaviour)
                paramEditor.Text = string.Empty;

                // Replace the active editor template with a disabled variant
                if (!_composerDisabledAfterFirstSend)
                {
                    _composerDisabledAfterFirstSend = true;

                    try
                    {
                        var disabledTemplate = CreateDisabledEditorTemplate();
                        if (AssistView != null)
                            AssistView.EditorViewTemplate = disabledTemplate;
                        if (DesktopAssistView != null)
                            DesktopAssistView.EditorViewTemplate = disabledTemplate;
                    }
                    catch { }
                }
            }
            else
            {
                text = activeAssistView?.InputText;
                if (activeAssistView != null)
                    activeAssistView.InputText = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(text))
                return;

            // If we are in the guided conversation flow, route the input to the ViewModel, not to the LLM.
            if (BindingContext is TravelPlannerViewModel vm)
            {
                if (vm.Step is ConversationStep.Destination or ConversationStep.Dates or ConversationStep.TripType or ConversationStep.Interests)
                {
                    // Important: yield to let SfAIAssistView append the user's message first,
                    // so our predefined follow-up appears AFTER the user's message.
                    await Task.Yield();
                    await Task.Delay(10);

                    if (vm.HandleUserInputCommand.CanExecute(text))
                        vm.HandleUserInputCommand.Execute(text);
                    return; // Stop here; follow-up questions are asked by the ViewModel
                }
            }

            // Otherwise, fall back to generic AI chat for ad-hoc questions
            var systemPrompt = "You are a helpful travel assistant. Reply concisely.";

            _openAI ??= Application.Current?.Handler?.MauiContext?.Services?.GetService<TravelPlanner.Services.OpenAIServices>()
                       ?? new TravelPlanner.Services.OpenAIServices();

            var reply = await _openAI.GetRawResponseAsync(systemPrompt, text);
            if (string.IsNullOrWhiteSpace(reply)) reply = "Sorry, I couldn't find an answer.";

            _vm?.AssistItems.Add(new AssistItem { Text = reply, IsRequested = false });
        }
        catch (Exception ex)
        {
            _vm?.AssistItems.Add(new AssistItem { Text = $"Error: {ex.Message}", IsRequested = false });
        }
    }

    private async void OnDrawerToggle(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        await ToggleDrawerAsync();
    }

    private async void OnDrawerButtonClicked(object sender, EventArgs e)
    {
        await ToggleDrawerAsync();
    }

    private void OnRestartTripClicked(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is not TravelPlannerViewModel vm)
                return;

            // All conversation/trip state is reset by the ViewModel; the page only
            // clears view-owned state in response to the TripRestarted notification.
            if (vm.RestartTripCommand.CanExecute(null))
                vm.RestartTripCommand.Execute(null);

            // Re-enable both composers explicitly
            if (AssistView != null)
                AssistView.IsEnabled = true;
            if (DesktopAssistView != null)
                DesktopAssistView.IsEnabled = true;

            ResetComposerTemplates();
        }
        catch { }
    }

    /// <summary>
    /// Restores the original (enabled) editor template after a restart.
    /// </summary>
    private void ResetComposerTemplates()
    {
        try
        {
            _composerDisabledAfterFirstSend = false;
            var template = this.Resources.ContainsKey("CustomEditorTemplate") ? this.Resources["CustomEditorTemplate"] as DataTemplate : null;
            if (template != null)
            {
                if (AssistView != null)
                    AssistView.EditorViewTemplate = template;
                if (DesktopAssistView != null)
                    DesktopAssistView.EditorViewTemplate = template;
            }
            else
            {
                // Fallback: reassign current templates to force a refresh
                if (AssistView != null)
                    AssistView.EditorViewTemplate = AssistView.EditorViewTemplate;
                if (DesktopAssistView != null)
                    DesktopAssistView.EditorViewTemplate = DesktopAssistView.EditorViewTemplate;
            }
        }
        catch { }
    }

    private async Task ToggleDrawerAsync()
    {
        var isDesktop = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet;
        var drawerWidth = isDesktop ? 600 : (this.Width > 0 ? this.Width : (this.Window?.Width ?? 320));

        if (_drawerOpen)
        {
            _drawerOpen = false;
            if (RightDrawerControl != null)
                await RightDrawerControl.TranslateToAsync(drawerWidth, 0, 250, Easing.CubicOut);
            if (HamburgerFrameControl != null)
                HamburgerFrameControl.IsVisible = true;
        }
        else
        {
            _drawerOpen = true;
            if (HamburgerFrameControl != null)
                HamburgerFrameControl.IsVisible = false;

            if (RightDrawerControl != null)
                await RightDrawerControl.TranslateToAsync(0, 0, 250, Easing.CubicOut);
        }
    }

    private async Task EnsureDrawerOpenAsync()
    {
        try
        {
            if (_drawerOpen) return;
            _drawerOpen = true;
            if (HamburgerFrameControl != null) HamburgerFrameControl.IsVisible = false;
            if (RightDrawerControl != null)
                await RightDrawerControl.TranslateToAsync(0, 0, 250, Easing.CubicOut);
        }
        catch { }
    }

    private void OnCalendarSelectionChanged(object? sender, CalendarSelectionChangedEventArgs e)
    {
        try
        {
            if (BindingContext is not TravelPlannerViewModel vm)
                return;

            var range = TripCalendarControl?.SelectedDateRange;
            vm.StageDateRange(range?.StartDate, range?.EndDate);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnCalendarSelectionChanged error: {ex}");
        }
    }

//#if !WINDOWS
    private async void OnPlaceTapped(object? sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        try
        {
            if (sender is not VisualElement ve) return;
            if (ve.BindingContext is not PlaceCard place) return;

            Location? loc = null;
            if (place.Latitude.HasValue && place.Longitude.HasValue)
            {
                loc = new Location(place.Latitude.Value, place.Longitude.Value);
            }
            else if (!string.IsNullOrWhiteSpace(place.Address))
            {
                try
                {
                    var results = await Geocoding.GetLocationsAsync(place.Address);
                    var first = results?.FirstOrDefault();
                    if (first != null)
                        loc = new Location(first.Latitude, first.Longitude);
                }
                catch { }
            }

            if (loc == null || MainMapControl == null) return;

                // Remove previous selected marker
                try { if (_selectedPlacePin != null) RemoveLayerMarker(ActiveMainTileLayer, _selectedPlacePin); } catch { }

            // Add new marker for selected place
            var mainLayer = ActiveMainTileLayer;
                if (mainLayer != null)
                {
                    var placeLabel = BuildMarkerLabel(place, "Selected Place", place?.Address, "Selected Place");

                    var placedetails = new CustomMarker
                    {
                        Name = place!.Name,
                        Area = place.Address,
                        Latitude = loc.Latitude,
                        Longitude = loc.Longitude,
                    };

                    _selectedPlacePin = new MapMarker
                    {
                        Latitude = loc.Latitude,
                        Longitude = loc.Longitude,
                        BindingContext = placedetails
                    };
                    MapMarkerCollection mapMarkers = new MapMarkerCollection();
                    mapMarkers.Add(placedetails);
                    mainLayer.Markers = mapMarkers;
                    System.Diagnostics.Debug.WriteLine($"Created place marker with tooltip: '{placeLabel.DisplayName}'");

                // Center and zoom in
                mainLayer.Center = new MapLatLng { Latitude = loc.Latitude, Longitude = loc.Longitude };
                    if (mainLayer.ZoomPanBehavior == null)
                        mainLayer.ZoomPanBehavior = new MapZoomPanBehavior { ZoomLevel = 14 };
                    else
                        mainLayer.ZoomPanBehavior.ZoomLevel = 14;
                }
        }
        catch { }
    }

    private void OnTripTypeTapped(object? sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        try
        {
            if (BindingContext is not TravelPlannerViewModel vm)
                return;
            var text = e?.Parameter as string;
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Add the selected trip type as a user request before the follow-up bot prompt
            vm.AssistItems.Add(new AssistItem { Text = text, IsRequested = true });

            if (vm.HandleUserInputCommand.CanExecute(text))
                vm.HandleUserInputCommand.Execute(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnTripTypeTapped error: {ex}");
        }
    }

    private void OnSuggestionTapped(object? sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        try
        {
            if (BindingContext is not TravelPlannerViewModel vm)
                return;
            var text = e?.Parameter as string;
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Add the tapped suggestion as a user request and forward to the ViewModel
            vm.AssistItems.Add(new AssistItem { Text = text, IsRequested = true });

            if (vm.HandleUserInputCommand.CanExecute(text))
                vm.HandleUserInputCommand.Execute(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnSuggestionTapped error: {ex}");
        }
    }

    private void OnCalendarOkClicked(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is not TravelPlannerViewModel vm)
                return;

            if (vm.CommitDateRangeCommand.CanExecute(null))
                vm.CommitDateRangeCommand.Execute(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnCalendarOkClicked error: {ex}");
        }
    }

    private void OnCalendarCancelClicked(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is TravelPlannerViewModel vm && vm.CancelDateRangeCommand.CanExecute(null))
                vm.CancelDateRangeCommand.Execute(null);

            // Clear the calendar selection so the visual state matches the cancelled range.
            try { TripCalendarControl?.SelectedDateRange = null; }
            catch { }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnCalendarCancelClicked error: {ex}");
        }
    }

    private async Task UpdateMainMapForDestinationAsync()
    {
        try
        {
            if (_vm == null || MainMapControl == null)
                return;

            var query = _vm.Destination?.Trim();
            if (string.IsNullOrWhiteSpace(query))
                return;

            // iOS CLGeocoder may throw NSError for invalid/unresolvable inputs; guard it.
            IEnumerable<Location>? results = null;
            try
            {
                results = await Geocoding.GetLocationsAsync(query);
            }
            catch
            {
                // Other transient/network errors
                return;
            }

            var first = results?.FirstOrDefault();
            if (first == null)
                return;

            var destLocation = new Location(first.Latitude, first.Longitude);

            // Update markers on active main layer
            var mainLayer = ActiveMainTileLayer;
            if (mainLayer != null)
            {
                ClearLayerMarkers(mainLayer);
                var markerLabel = BuildMarkerLabel(query, "Destination", "", "Destination");
                
                var marker = new MapMarker
                {
                    Latitude = destLocation.Latitude,
                    Longitude = destLocation.Longitude,
                    BindingContext = markerLabel
                };
                CustomMarker temp = new CustomMarker();
                temp.Latitude = destLocation.Latitude;
                temp.Longitude = destLocation.Longitude;
                temp.Name = query;
                temp.Area = "Local Area";

                MapMarkerCollection mapMarkers = new MapMarkerCollection();
                mapMarkers.Add(temp);
                mainLayer.Markers = mapMarkers;

                mainLayer.Center = new MapLatLng { Latitude = destLocation.Latitude, Longitude = destLocation.Longitude };
                if (mainLayer.ZoomPanBehavior == null)
                    mainLayer.ZoomPanBehavior = new MapZoomPanBehavior { ZoomLevel = 13 };
                else
                    mainLayer.ZoomPanBehavior.ZoomLevel = 13;
            }
        }
        catch { }
    }

    // Try to obtain device current location and show it on the active main map layer
    private async Task TryShowCurrentLocationAsync()
    {
        try
        {
            // Check and request runtime permission
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            if (status != PermissionStatus.Granted)
                return;

            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);
            if (location == null) return;

            var mainLayer = ActiveMainTileLayer;
            if (mainLayer == null) return;

            // Remove previous device marker if present
            try { if (_currentLocationMarker != null) RemoveLayerMarker(mainLayer, _currentLocationMarker); } catch { }

            var currentLocationLabel = BuildMarkerLabel("Your Location", "Your Location", "Current device location", "Device Location");

            CustomMarker temp = new CustomMarker();
            temp.Latitude = location.Latitude;
            temp.Longitude = location.Longitude;
            temp.Name = "Your Location";
            temp.Area = "Local Area";
            _currentLocationMarker = new MapMarker
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                BindingContext = temp
            };

            MapMarkerCollection mapMarkers = new MapMarkerCollection();
            mapMarkers.Add(temp);
            mainLayer.Markers = mapMarkers;

            System.Diagnostics.Debug.WriteLine($"Created marker with tooltip: 'Your Location'");

            // Ensure UI-thread map updates
            await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    // Center and zoom
                    mainLayer.Center = new MapLatLng { Latitude = location.Latitude, Longitude = location.Longitude };
                    if (mainLayer.ZoomPanBehavior == null)
                        mainLayer.ZoomPanBehavior = new MapZoomPanBehavior { ZoomLevel = 13 };
                    else
                        mainLayer.ZoomPanBehavior.ZoomLevel = 13;
                }
                catch { }
            });
        }
        catch { }
    }

    // Helpers to manipulate MapTileLayer.Markers safely across Syncfusion versions
    private void ClearLayerMarkers(Syncfusion.Maui.Maps.MapTileLayer? layer)
    {
        if (layer == null) return;
        try
        {
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var markers = layer.Markers;
                    if (markers == null)
                    {
                        var propInit = layer.GetType().GetProperty("Markers");
                        if (propInit != null && propInit.CanWrite)
                        {
                            propInit.SetValue(layer, new System.Collections.Generic.List<Syncfusion.Maui.Maps.MapMarker>());
                        }
                        return;
                    }

                    if (markers is System.Collections.IList list)
                    {
                        list.Clear();
                        return;
                    }

                    var prop = layer.GetType().GetProperty("Markers");
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(layer, new System.Collections.Generic.List<Syncfusion.Maui.Maps.MapMarker>());
                        return;
                    }

                    var clearMethod = markers.GetType().GetMethod("Clear");
                    clearMethod?.Invoke(markers, null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ClearLayerMarkers error: {ex}");
                }
            });
        }
        catch (Exception exOuter)
        {
            System.Diagnostics.Debug.WriteLine($"ClearLayerMarkers outer error: {exOuter}");
        }
    }

    private void RemoveLayerMarker(Syncfusion.Maui.Maps.MapTileLayer? layer, Syncfusion.Maui.Maps.MapMarker marker)
    {
        if (layer == null || marker == null) return;
        try
        {
            var markers = layer.Markers;
            if (markers is System.Collections.IList list)
            {
                list.Remove(marker);
                return;
            }

            var prop = layer.GetType().GetProperty("Markers");
            if (prop != null && prop.CanRead && prop.CanWrite)
            {
                var existing = prop.GetValue(layer) as System.Collections.Generic.IEnumerable<Syncfusion.Maui.Maps.MapMarker>;
                if (existing != null)
                {
                    var newList = new System.Collections.Generic.List<Syncfusion.Maui.Maps.MapMarker>(existing);
                    newList.Remove(marker);
                    prop.SetValue(layer, newList);
                }
                return;
            }

            var removeMethod = markers?.GetType().GetMethod("Remove");
            removeMethod?.Invoke(markers, new object[] { marker });
        }
        catch { }
    }

    private void OnSelectInterestsClicked(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is not TravelPlannerViewModel vm) return;
            vm.IsInterestsPopupVisible = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnSelectInterestsClicked error: {ex}");
        }
    }



    private void OnInterestsCancelClicked(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is TravelPlannerViewModel vm && vm.CancelInterestsCommand.CanExecute(null))
                vm.CancelInterestsCommand.Execute(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnInterestsCancelClicked error: {ex}");
        }
    }

    private void OnChipsSelectionChanged(object? sender, Syncfusion.Maui.Toolkit.Chips.SelectionChangedEventArgs e)
    {
        try
        {
            if (BindingContext is not TravelPlannerViewModel vm) return;

            // Clear and rebuild selected interests from the chip group

            string selectedItem =  (string)e.AddedItem!;
            string removedItem = (string)e.RemovedItem!;
            if (selectedItem != null)
            {
                vm.SelectedInterests.Add(selectedItem);
            }
            if(removedItem != null) {
            vm.SelectedInterests.Remove(removedItem);
            }
            // Clear any previous validation error when the user changes selection
            vm.InterestsErrorMessage = string.Empty;
            ErrorText.HeightRequest = 0;
            ErrorTextDesktop.HeightRequest = 0;
            System.Diagnostics.Debug.WriteLine($"Selected interests updated: {string.Join(", ", vm.SelectedInterests)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnChipsSelectionChanged error: {ex}");
        }
    }

    private async void OnInterestsConfirmClicked(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is not TravelPlannerViewModel vm)
                return;

            if (vm.ConfirmInterestsCommand.CanExecute(null))
                vm.ConfirmInterestsCommand.Execute(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnInterestsConfirmClicked error: {ex}");
        }
    }

    private void MapTooltipSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {

    }
}

public class CustomMarker : MapMarker
{
    public string? Name { get; set; }
    public string? Area { get; set; }
}