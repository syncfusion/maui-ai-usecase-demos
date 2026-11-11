using System.Text.Json.Nodes;
using Microsoft.Maui.Controls.Shapes;
using SkyBook.Core;
using Syncfusion.A2UI.Core.Schema;
using Syncfusion.A2UI.Core.State;
using Syncfusion.Maui.Chat;
using Syncfusion.Maui.A2UI.Hosting;

namespace SkyBook.Maui;

/// <summary>
/// Port of <c>App.tsx</c>. Non-visual logic lives in <c>SkyBook.Core</c>; this page owns the views, the
/// <see cref="SurfaceHost"/> (React: <c>MessageProcessor</c>) and the welcome / chat view state.
/// </summary>
public partial class MainPage : ContentPage
{
  private enum View { Welcome, Chat }
  private enum Tab { Chat, Booking }

  private readonly SurfaceHost _host;
  private readonly IAgentClient _agent;
  private readonly BookingSession _session;
  private readonly ChatViewModel _chat = new();
  private readonly List<Border> _stageCards = [];
  private readonly List<(Border Chip, Label Dot, Label Label)> _stepper = [];
  private readonly List<Button> _placeholderButtons = [];

  private View _view = View.Welcome;
  private Tab _tab = Tab.Chat;
  private bool _busy;
  private bool _compact;
  private Stage? _activeStage;
  private string? _silentPayload;
  private SurfaceModel? _surface;
  private bool _firstPageShown;

  public MainPage(SurfaceHost host, IAgentClient agent, BookingSession session)
  {
    InitializeComponent();
    _host = host; _agent = agent; _session = session;

    Chat.BindingContext = _chat;
    // SfChat: we add the user's message ourselves (so prompts from buttons and typed text look the same).
    // SfChat clears its editor after a send.
    // Let SfChat add the message and clear its input box (setting e.Handled would keep the text in the box).
    Chat.SendMessage += (_, e) =>
    {
      var text = e.Message?.Text?.Trim();
      if (string.IsNullOrEmpty(text) || _busy) { e.Handled = true; return; }
      if (e.Message is TextMessage tm) { tm.Author = _chat.CurrentUser; tm.DateTime = DateTime.Now; }
      _ = SubmitPromptAsync(text, silentPayload: null, alreadyShown: true);
    };

    SurfaceView.Host = _host;
    _host.SurfaceCreated += (_, s) => MainThread.BeginInvokeOnMainThread(() => SetSurface(s));
    _host.SurfaceUpdated += (_, s) => MainThread.BeginInvokeOnMainThread(() => { if (_surface?.Id == s.Id) { UpdateStep(); Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(250), NormalizeInputs); } });
    _host.SurfaceDeleted += (_, id) => MainThread.BeginInvokeOnMainThread(() => { if (_surface?.Id == id) SetSurface(null); });
    _host.ActionDispatched += (_, a) => MainThread.BeginInvokeOnMainThread(() => OnSurfaceAction(a));
    Ui.SkyHost.BookRequested += row => MainThread.BeginInvokeOnMainThread(() => OnBookRow(row));

    StyleChat();
    BuildJourney();
    BuildStageCards();
    BuildStepper();
    BuildPlaceholderSteps();
    ApplyState();
    SizeChanged += (_, _) => ApplyLayout();
  }

  // ------------------------------------------------------------------ view building

  private void BuildJourney()
  {
    for (var i = 0; i < Stage.All.Count; i++)
    {
      var s = Stage.All[i];
      JourneyRow.Add(new HorizontalStackLayout
      {
        Spacing = 8,
        VerticalOptions = LayoutOptions.Center,
        Children =
                {
                    new Border
                    {
                        WidthRequest = 26, HeightRequest = 26, StrokeThickness = 0, BackgroundColor = Color.FromArgb("#2563eb"),
                        StrokeShape = new RoundRectangle { CornerRadius = 13 },
                        Content = new Label { Text = s.Step.ToString(), FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
                    },
                    new Label { Text = s.ShortLabel, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1e293b"), VerticalOptions = LayoutOptions.Center },
                },
      });
      if (i < Stage.All.Count - 1)
        JourneyRow.Add(new BoxView
        {
          WidthRequest = 56,
          HeightRequest = 2,
          VerticalOptions = LayoutOptions.Center,
          CornerRadius = 1,
          Background = new LinearGradientBrush([new GradientStop(Color.FromArgb("#2563eb"), 0), new GradientStop(Color.FromArgb("#22ad78"), 1)], new Point(0, 0), new Point(1, 0)),
        });
    }
  }

  private void BuildStageCards()
  {
    foreach (var s in Stage.All)
    {
      var card = new StageCardView(s, OpenStage)
      {
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Fill,
      };
      _stageCards.Add(card);
      card.SizeChanged += (_, _) => FitCardText(card);
    }
    ArrangeStageCards(3);
  }

  /// <summary>
  /// Keeps every line of text inside its stage card: labels wrap instead of running past the border.
  /// Labels inside horizontal rows (e.g. "✓ Review traveler details…") get a max width so they can wrap too.
  /// </summary>
  private static void FitCardText(Border card)
  {
    if (card.Width <= 0) return;
    var inner = card.Width - card.Padding.HorizontalThickness - card.StrokeThickness * 2;
    foreach (var label in ((IVisualTreeElement)card).GetVisualTreeDescendants().OfType<Label>())
    {
      label.LineBreakMode = LineBreakMode.WordWrap;
      label.MaxLines = -1;
      if (label.Parent is HorizontalStackLayout row)
      {
        // room left after the other items in the row (icon, spacing)
        var others = row.Children.OfType<VisualElement>().Where(c => c != label).Sum(c => c.Width + row.Spacing);
        var max = Math.Max(60, inner - others - 8);
        if (Math.Abs(label.MaximumWidthRequest - max) > 1) label.MaximumWidthRequest = max;
      }
    }
  }

  private int _cardColumns;

  /// <summary>Lays the stage cards out in an equal-width grid: 3 columns (desktop), 2 (tablet), 1 (phone).</summary>
  private void ArrangeStageCards(int columns)
  {
    if (columns == _cardColumns) return;
    _cardColumns = columns;

    StageGrid.Children.Clear();
    StageGrid.ColumnDefinitions.Clear();
    StageGrid.RowDefinitions.Clear();
    for (var c = 0; c < columns; c++) StageGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
    var rows = (int)Math.Ceiling(_stageCards.Count / (double)columns);
    for (var r = 0; r < rows; r++) StageGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

    for (var i = 0; i < _stageCards.Count; i++)
    {
      var card = _stageCards[i];
      card.WidthRequest = -1; // let the star column size it
      var row = i / columns; var col = i % columns;
      // Centre a lone last card when there are 2 columns (e.g. card 3 of 3)
      var lone = columns == 2 && i == _stageCards.Count - 1 && _stageCards.Count % 2 == 1;
      StageGrid.Add(card, col, row);
      if (lone) { Grid.SetColumnSpan(card, 2); card.MaximumWidthRequest = 520; card.HorizontalOptions = LayoutOptions.Center; }
      else { card.MaximumWidthRequest = double.PositiveInfinity; card.HorizontalOptions = LayoutOptions.Fill; }
    }
  }

  private readonly List<BoxView> _stepLines = [];

  /// <summary>SfChat look: blue outgoing bubbles (right), white incoming bubbles with the bot avatar (left).</summary>
  private void StyleChat()
  {
    Chat.BackgroundColor = Colors.White;
    // Our own bubbles (same look as the web design): avatar + white bordered bubble on the left,
    // blue gradient bubble on the right, time under each message.
    Chat.MessageTemplate = new BubbleSelector(_chat);
  }

  private sealed class BubbleSelector(ChatViewModel vm) : DataTemplateSelector
  {
    private readonly DataTemplate _out = new(() => Bubble(user: true));
    private readonly DataTemplate _in = new(() => Bubble(user: false));
    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        item is IMessage m && ReferenceEquals(m.Author, vm.CurrentUser) ? _out : _in;
  }

  private static Microsoft.Maui.Controls.View Bubble(bool user)
  {
    var text = new Label { FontSize = 14, LineBreakMode = LineBreakMode.WordWrap, TextColor = user ? Colors.White : Color.FromArgb("#0f172a") };
    text.SetBinding(Label.TextProperty, "Text");
    var time = new Label { FontSize = 11, TextColor = Color.FromArgb("#94a3b8"), HorizontalOptions = user ? LayoutOptions.End : LayoutOptions.Start };
    time.SetBinding(Label.TextProperty, new Binding("DateTime", stringFormat: "{0:hh:mm tt}"));

    var bubble = new Border
    {
      Padding = new Thickness(14, 10),
      StrokeThickness = user ? 0 : 1,
      Stroke = Color.FromArgb("#e2e8f0"),
      Background = user
            ? new LinearGradientBrush([new GradientStop(Color.FromArgb("#2563eb"), 0), new GradientStop(Color.FromArgb("#1d4ed8"), 1)], new Point(0, 0), new Point(1, 1))
            : new SolidColorBrush(Colors.White),
      StrokeShape = new RoundRectangle { CornerRadius = user ? new CornerRadius(16, 16, 4, 16) : new CornerRadius(16, 16, 16, 4) },
      Content = text,
    };
    if (!user) bubble.Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.06f, Radius = 6, Offset = new Point(0, 2) };
    var column = new VerticalStackLayout { Spacing = 3, Children = { bubble, time }, MaximumWidthRequest = 300 };

    if (user)
    {
      column.HorizontalOptions = LayoutOptions.End;
      return new Grid { Padding = new Thickness(48, 6, 12, 6), Children = { column } };
    }

    var avatar = new Border
    {
      WidthRequest = 30,
      HeightRequest = 30,
      StrokeThickness = 0,
      VerticalOptions = LayoutOptions.End,
      Margin = new Thickness(0, 0, 0, 18),
      StrokeShape = new RoundRectangle { CornerRadius = 15 },
      Background = new LinearGradientBrush([new GradientStop(Color.FromArgb("#2563eb"), 0), new GradientStop(Color.FromArgb("#22ad78"), 1)], new Point(0, 0), new Point(1, 1)),
      Content = new Label { Text = "✦", FontSize = 13, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
    };
    return new HorizontalStackLayout { Spacing = 8, Padding = new Thickness(10, 6, 48, 6), HorizontalOptions = LayoutOptions.Start, Children = { avatar, column } };
  }

  private void BuildStepper()
  {
    // React: dot + label, joined by a 2px line that turns green once the step is done.
    var n = Stage.All.Count;
    for (var i = 0; i < n; i++)
    {
      var s = Stage.All[i];
      var dot = new Label { Text = s.Step.ToString(), FontSize = 12, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, WidthRequest = 26, HeightRequest = 26 };
      var label = new Label { Text = s.StepperLabel, FontSize = 13, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
      var chip = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 13 }, Content = dot, VerticalOptions = LayoutOptions.Center };
      var item = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center, Children = { chip, label } };

      Stepper.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
      Stepper.Add(item, Stepper.ColumnDefinitions.Count - 1, 0);
      _stepper.Add((chip, dot, label));

      if (i < n - 1)
      {
        var line = new BoxView { HeightRequest = 2, Margin = new Thickness(12, 0), VerticalOptions = LayoutOptions.Center, Color = Color.FromArgb("#e2e8f0"), CornerRadius = 1 };
        Stepper.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        Stepper.Add(line, Stepper.ColumnDefinitions.Count - 1, 0);
        _stepLines.Add(line);
      }
    }
  }

  private void BuildPlaceholderSteps()
  {
    foreach (var s in Stage.All)
    {
      var b = new Button
      {
        Text = $"Step {s.Step} · {s.Title}   →",
        FontSize = 13,
        FontAttributes = FontAttributes.Bold,
        TextColor = Color.FromArgb("#0f172a"),
        BackgroundColor = Colors.White,
        BorderColor = Color.FromArgb("#e2e8f0"),
        BorderWidth = 1,
        CornerRadius = 12,
        HorizontalOptions = LayoutOptions.Fill,
        Padding = new Thickness(14, 11),
        AutomationId = $"placeholder-{s.Id.ToString().ToLowerInvariant()}",
      };
      SemanticProperties.SetDescription(b, $"{s.Title}: {s.Tagline}");
      b.Clicked += (_, _) => OpenStage(s);
      _placeholderButtons.Add(b);
      PlaceholderSteps.Add(b);
    }
  }

  // ------------------------------------------------------------------ state → UI

  private void ApplyState()
  {
    var welcome = _view == View.Welcome;
    WelcomeScroll.IsVisible = welcome;
    Chat.IsVisible = !welcome;
    ChatHeader.IsVisible = !welcome;
    HomeButton.IsVisible = !welcome;
    Stepper.IsVisible = !welcome;
    SurfacePane.IsVisible = !welcome && (!_compact || _tab == Tab.Booking);
    ChatPane.IsVisible = welcome || !_compact || _tab == Tab.Chat;
    MobileTabs.IsVisible = !welcome && _compact;

    // Columns: welcome = chat only; wide = chat 420 + booking; phone = whichever tab is selected, full width.
    var showBooking = !welcome && _compact && _tab == Tab.Booking;
    ChatColumn.Width = welcome ? GridLength.Star
        : _compact ? (showBooking ? new GridLength(0) : GridLength.Star)
        : new GridLength(420);
    SurfaceColumn.Width = welcome ? new GridLength(0)
        : _compact ? (showBooking ? GridLength.Star : new GridLength(0))
        : GridLength.Star;
    Body.ColumnSpacing = _compact || welcome ? 0 : 20;
    ChatPane.MinimumWidthRequest = welcome || _compact ? 0 : 420;

    foreach (var b in _placeholderButtons) b.IsEnabled = !_busy;
    WelcomeSendButton.IsEnabled = !_busy && !string.IsNullOrWhiteSpace(WelcomeEditor.Text);
    ClearButton.IsEnabled = _chat.Messages.Count > 0;
    SurfaceLoading.IsVisible = SurfaceLoading.IsRunning = _busy;
    Chat.Margin = new Thickness(0);
    // Welcome uses the whole pane (header row is hidden); chat view keeps rows 1-2.
    Grid.SetRow(WelcomeScroll, welcome ? 0 : 1);
    Grid.SetRowSpan(WelcomeScroll, welcome ? 3 : 2);

    var active = _tab == Tab.Chat;
    ChatTabButton.BackgroundColor = active ? Color.FromArgb("#2563eb") : Colors.White;
    ChatTabButton.TextColor = active ? Colors.White : Color.FromArgb("#64748b");
    ViewTabButton.BackgroundColor = !active ? Color.FromArgb("#2563eb") : Colors.White;
    ViewTabButton.TextColor = !active ? Colors.White : Color.FromArgb("#64748b");

    UpdateStep();
    ApplyLayout();
  }

  private void ApplyLayout()
  {
    var compact = Width > 0 && Width < AppConfig.CompactWidth;
    if (compact != _compact) { _compact = compact; ApplyState(); return; }

    if (Width <= 0) return;

    // Welcome cards: 3 columns >= 1000px, 2 columns >= 640px, otherwise 1 (phones / narrow windows).
    var content = Math.Min(1100, Width - (_compact ? 24 : 48) - 48);
    ArrangeStageCards(content >= 900 ? 3 : content >= 560 ? 2 : 1);

    // Journey strip: shorter connectors on narrow widths so it never overflows.
    foreach (var line in JourneyRow.Children.OfType<BoxView>())
      line.WidthRequest = content >= 700 ? 56 : content >= 420 ? 24 : 10;
    JourneyRow.Spacing = content >= 420 ? 12 : 6;

    Body.Padding = _compact ? new Thickness(12) : new Thickness(24, 20, 24, 24);
    WelcomeStack.Padding = _compact ? new Thickness(12, 16) : new Thickness(24);
    SurfaceView.MinimumWidthRequest = 0;
  }

  private int CurrentStep() =>
      _surface is null ? 0 : Stage.InferStep(_surface.Id,
          _surface.ComponentsModel.Keys);

  private void UpdateStep()
  {
    var step = CurrentStep();
    for (var i = 0; i < _stepper.Count; i++)
    {
      var (chip, dot, label) = _stepper[i];
      var n = i + 1;
      var current = step == n; var done = step > n;
      var bg = current ? Color.FromArgb("#2563eb") : done ? Color.FromArgb("#14a37f") : Color.FromArgb("#e2e8f0");
      chip.BackgroundColor = bg;
      dot.Text = done ? "✓" : n.ToString();
      chip.BackgroundColor = current ? Color.FromArgb("#2563eb") : done ? Color.FromArgb("#14a37f") : Color.FromArgb("#e2e8f0");
      dot.TextColor = current || done ? Colors.White : Color.FromArgb("#64748b");
      label.TextColor = current ? Color.FromArgb("#0f172a") : Color.FromArgb("#64748b");
      label.FontAttributes = current ? FontAttributes.Bold : FontAttributes.None;
      label.IsVisible = !_compact || current; // React: .step-label hidden on narrow screens unless current
      if (i < _stepLines.Count) _stepLines[i].Color = step > n ? Color.FromArgb("#14a37f") : Color.FromArgb("#e2e8f0");
    }

    var shown = step > 0 ? Stage.All[step - 1] : _activeStage;
    StagePillRow.IsVisible = shown is not null;
    if (shown is not null) StagePillLabel.Text = $"{shown.Icon}  Step {shown.Step} · {shown.Title}";
  }

  /// <summary>
  /// Gives every form input on the rendered page the same height (48) and full column width.
  /// The stock A2UI input renderers ignore heightRequest in the JSON, so we fix it after rendering.
  /// </summary>
  private void NormalizeInputs()
  {
    SelectComboValues();
    static IEnumerable<Element> Walk(IVisualTreeElement e)
    {
      foreach (var c in e.GetVisualChildren())
      {
        if (c is Element el) yield return el;
        foreach (var d in Walk(c)) yield return d;
      }
    }
    string[] inputTypes = ["SfComboBox", "SfNumericEntry", "SfMaskedEntry", "SfDatePicker", "DatePicker", "Entry", "SfTextInputLayout"];
    foreach (var v in Walk(SurfaceView).OfType<Microsoft.Maui.Controls.View>())
    {
      var name = v.GetType().Name;
      if (!inputTypes.Contains(name)) continue;
      if (v.Parent is Microsoft.Maui.Controls.View p && inputTypes.Contains(p.GetType().Name)) continue; // inner part of another input
      v.HeightRequest = 48;
      v.MinimumHeightRequest = 48;
      v.HorizontalOptions = LayoutOptions.Fill;
      v.VerticalOptions = LayoutOptions.Start;
    }
  }

  /// <summary>
  /// The stock combo-box renderer doesn't pick its selection from the data model, so after a page renders we
  /// select Trip Type / Cabin Class / Nationality ourselves from the values the agent put in the model.
  /// Selecting also triggers the combo's own change logic, so Search Flights sends these values.
  /// </summary>
  private void SelectComboValues()
  {
    if (_surface is null) return;
    var wanted = new[] { "/search/tripType", "/search/cabinClass", "/passenger/nationality" }
        .Select(p => { try { return Convert.ToString(_surface.DataModel.Get(p))?.Trim('"') ?? ""; } catch { return ""; } })
        .Where(v => v.Length > 0).ToList();
    if (wanted.Count == 0) return;

    static IEnumerable<Element> Walk(IVisualTreeElement e)
    {
      foreach (var c in e.GetVisualChildren())
      {
        if (c is Element el) yield return el;
        foreach (var d in Walk(c)) yield return d;
      }
    }

    foreach (var combo in Walk(SurfaceView).Where(e => e.GetType().Name == "SfComboBox"))
    {
      var t = combo.GetType();
      if (t.GetProperty("ItemsSource")?.GetValue(combo) is not System.Collections.IEnumerable items) continue;
      var texts = items.Cast<object?>().Select(ItemText).ToList();
      foreach (var value in wanted)
      {
        var index = texts.FindIndex(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
        if (index < 0) continue;
        if (t.GetProperty("SelectedIndex") is { CanWrite: true } si && !Equals(si.GetValue(combo), index))
          si.SetValue(combo, index);
        break;
      }
    }
  }

  /// <summary>Display text of a combo item, whatever shape it has (dictionary, JSON, object with Text/text).</summary>
  private static string ItemText(object? item) => item switch
  {
    null => "",
    string s => s,
    System.Text.Json.JsonElement je when je.ValueKind == System.Text.Json.JsonValueKind.Object && je.TryGetProperty("text", out var tx) => tx.ToString(),
    System.Text.Json.Nodes.JsonObject jo => jo["text"]?.ToString() ?? "",
    IDictionary<string, object?> d => d.TryGetValue("text", out var v) ? Convert.ToString(v) ?? "" : "",
    System.Collections.IDictionary nd => nd.Contains("text") ? Convert.ToString(nd["text"]) ?? "" : "",
    _ => Convert.ToString(item.GetType().GetProperty("text")?.GetValue(item)
            ?? item.GetType().GetProperty("Text")?.GetValue(item) ?? item) ?? "",
  };

  private void SetSurface(SurfaceModel? surface)
  {
    if (surface is not null) Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(250), NormalizeInputs);
    _surface = surface;
    SurfaceView.Surface = surface;
    SurfaceScroll.IsVisible = surface is not null;
    Placeholder.IsVisible = surface is null;
    UpdateStep();
    if (surface is not null && _compact) { _tab = Tab.Booking; ApplyState(); }
  }

  // ------------------------------------------------------------------ user interaction

  private void OnHomeClicked(object? s, EventArgs e) => BackHome();
  private void OnChatTab(object? s, EventArgs e) { _tab = Tab.Chat; ApplyState(); }
  private void OnViewTab(object? s, EventArgs e) { _tab = Tab.Booking; ApplyState(); }
  private void OnClearClicked(object? s, EventArgs e) => ResetConversation();
  private void OnWelcomeTextChanged(object? s, TextChangedEventArgs e) => ApplyState();

  private void OnWelcomeSend(object? s, EventArgs e)
  {
    var text = WelcomeEditor.Text?.Trim();
    if (string.IsNullOrEmpty(text) || _busy) return;
    WelcomeEditor.Text = "";
    _ = SubmitPromptAsync(text, null, alreadyShown: false);
  }

  private void OpenStage(Stage stage)
  {
    if (_busy) return;
    _activeStage = stage;
    _ = SubmitPromptAsync(stage.StarterPrompt, null, alreadyShown: false);
  }

  private void ResetSurfaces()
  {
    if (_host.Surfaces.Count == 0) return;
    var del = new JsonArray(_host.Surfaces.Keys.Select(id =>
        (JsonNode)new JsonObject { ["version"] = "v0.9", ["deleteSurface"] = new JsonObject { ["surfaceId"] = id } }).ToArray());
    _host.ProcessJson(del.ToJsonString());
  }

  private void ResetConversation()
  {
    _session.Reset();
    _silentPayload = null;
    _chat.Clear();
    ApplyState();
  }

  private void BackHome()
  {
    _firstPageShown = false;
    ResetSurfaces();
    SetSurface(null);
    ResetConversation();
    _activeStage = null;
    _view = View.Welcome;
    _tab = Tab.Chat;
    ApplyState();
    _ = WelcomeScroll.ScrollToAsync(0, 0, false); // always land on the top of the home page
  }

  /// <summary>Port of the MessageProcessor action callback in App.tsx.</summary>
  private void OnSurfaceAction(A2uiClientAction action)
  {
    var surface = _host.Surfaces.GetValueOrDefault(action.SurfaceId);
    var ctx = ContextResolver.Resolve(action.Context, path => surface?.DataModel.Get(path));
    var prompt = _session.BuildActionPrompt(action.Name, ctx, _busy);
    if (prompt is null) return;
    // The summary is shown as the user's chat prompt; the agent receives the detailed payload.
    _ = SubmitPromptAsync(prompt.ChatText, prompt.AgentPayload, alreadyShown: false);
  }

  /// <summary>React: each flight row has its own "Book now" button; pressing it books exactly that row.</summary>
  private void OnBookRow(IDictionary<string, object> row)
  {
    var selected = new JsonObject();
    foreach (var (k, v) in row)
      if (v is null or string or bool or int or long or double or float)
        selected[k] = v is null ? null : JsonValue.Create(v);
    selected.Remove("action");
    var context = new JsonObject { ["selectedRow"] = selected, ["activeStep"] = 1, ["nextStep"] = 2 };
    var prompt = _session.BuildActionPrompt("selectFlightAndProceed", context, _busy);
    if (prompt is null) return;
    _ = SubmitPromptAsync(prompt.ChatText, prompt.AgentPayload, alreadyShown: false);
  }

  // ------------------------------------------------------------------ agent round-trip

  /// <summary>Port of <c>onPromptRequest</c> + <c>sendToAgent</c> (App.tsx).</summary>
  private async Task SubmitPromptAsync(string text, string? silentPayload, bool alreadyShown)
  {
    if (_busy) return;

    _silentPayload = silentPayload;

    if (!alreadyShown)
    {
      _chat.AddUserMessage(text);

      ApplyState();          // Force UI update
      await Task.Delay(100); // Give MAUI time to render
    }

    _session.RecordUserPrompt(text);

    _view = View.Chat;
    _tab = Tab.Chat;

    var payload = _silentPayload;
    _silentPayload = null;

    await SendToAgentAsync(payload ?? text, silent: payload is not null);
  }

  private async Task SendToAgentAsync(string text, bool silent)
  {
    if (string.IsNullOrWhiteSpace(text)) return;
    _busy = true;
    _chat.ShowTyping();
    ApplyState();
    try
    {
      var outgoing = _session.BuildOutgoingText(text, silent);
      var started = DateTime.UtcNow;
      var reply = await _agent.SendAsync(outgoing, _session.ContextId);
      if (reply.ContextId is not null) _session.ContextId = reply.ContextId;

      if (reply.A2uiMessages is null && reply.AgentText.Length > 0)
      {
        _chat.AddBotMessage(reply.AgentText); // plain answer, no surface change
        return;
      }
      if (reply.A2uiMessages is not { } messages)
      {
        System.Diagnostics.Debug.WriteLine("[A2UI] no a2uiEnvelope in response");
        _chat.AddBotMessage("I couldn't get a valid result from the booking service. Please try again.");
        return;
      }

      try
      {
        // Always render on a fresh surface: drop existing ones when the agent creates a new surface.
        if (EnvelopeSanitizer.HasCreateSurface(messages))
        {
          // Only the very first page shows the "Your booking workspace" intro for ~4 s
          ResetSurfaces();
          if (!_firstPageShown)
          {
            _firstPageShown = true;
            SetSurface(null);
            var remaining = TimeSpan.FromSeconds(4) - (DateTime.UtcNow - started);
            if (remaining > TimeSpan.Zero) await Task.Delay(remaining);
          }
        }
        _host.ProcessJson(EnvelopeSanitizer.Sanitize(messages).ToJsonString());
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"[A2UI] failed to render surface: {ex}");
        _chat.AddBotMessage($"I received the results but couldn't render them: {ex.Message}");
        return;
      }

      var explanation = EnvelopeSanitizer.ExtractExplanation(messages);
      var answer = explanation.Length > 0 ? explanation : reply.AgentText;
      _chat.AddBotMessage(answer.Length > 0 ? answer : "Updated the booking view.");
    }
    catch (HttpRequestException ex)
    {
      System.Diagnostics.Debug.WriteLine($"[A2UI] agent unreachable: {ex}");
      await ShowBundledStageAsync(text);
    }
    catch (TaskCanceledException ex)
    {
      System.Diagnostics.Debug.WriteLine($"[A2UI] agent timeout: {ex}");
      await ShowBundledStageAsync(text);
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"[A2UI] fetch error: {ex}");

      // Missing AI configuration -> switch to offline mode
      if (ex.Message.Contains("API key", StringComparison.OrdinalIgnoreCase) ||
          ex.Message.Contains("OpenRouter", StringComparison.OrdinalIgnoreCase) ||
          ex.Message.Contains("Azure OpenAI", StringComparison.OrdinalIgnoreCase))
      {
        await ShowBundledStageAsync(text);

        
        return;
      }

      _chat.AddBotMessage(
          "Unable to connect to the AI service. Please try again later.");
    }
    finally
    {
      _chat.HideTyping();
      _busy = false;
      ApplyState();
    }
  }

  /// <summary>
  /// Agent unreachable: open the matching stage page from the bundled design (same JSON the agent serves),
  /// so Search / Results / Confirmation still open. Free-text requests that need the AI get a hint instead.
  /// </summary>
  private async Task ShowBundledStageAsync(string text)
  {
    var lower = text.ToLowerInvariant();
    string? file = null; string say = "";
    if (lower.Contains("selectflightandproceed") || lower.Contains("booking and confirmation") || lower.Contains("confirmation page"))
    { file = "stage3-booking-confirmation.json"; say = "Here is the booking confirmation page."; }
    else if (lower.Contains("searchflights") || lower.Contains("flight results"))
    { file = "stage2-flight-results.json"; say = "Here are the flight results. Tap a flight row, then press Book now."; }
    else if (lower.Contains("flight search form"))
    { file = "stage1-flight-search.json"; say = "Here is the flight search form. Fill in your trip and press Search Flights."; }

    if (file is null)
    {
      _chat.AddBotMessage(
    "AI Assistant is currently unavailable. " +
    "You can continue using the flight booking workflow manually.");
      return;
    }

    try
    {
      using var stream = await FileSystem.OpenAppPackageFileAsync("designs/" + file);
      using var reader = new StreamReader(stream);
      var messages = System.Text.Json.Nodes.JsonNode.Parse(await reader.ReadToEndAsync())!.AsArray();
      ResetSurfaces();
      _host.ProcessJson(EnvelopeSanitizer.Sanitize(messages).ToJsonString());
      _chat.AddBotMessage(say + " (offline preview - start the agent for live AI replies)");
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"[A2UI] bundled stage failed: {ex}");
      _chat.AddBotMessage($"I couldn't open that page: {ex.Message}");
    }
  }
}
