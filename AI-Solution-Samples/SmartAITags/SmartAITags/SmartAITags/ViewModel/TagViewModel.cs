using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using SmartAITags.AIService;
using SmartAITags.Model;

namespace SmartAITags.ViewModel
{
    /// <summary>
    /// Drives AI tag generation using AzureOpenAITagService (direct HttpClient
    /// against the Azure AI Foundry chat/completions endpoint). Mirrors the
    /// SmartAIDatePicker call pattern: VM builds the user prompt, calls
    /// GetCompletion, then parses + filters tags before raising TagsReceived.
    /// </summary>
    public class TagViewModel : INotifyPropertyChanged
    {
        // Service is constructed directly — same pattern as SmartAIDatePicker.MainPage.
        private readonly AzureOpenAITagService _aiService = new();

        private ObservableCollection<TagModel> _tags;
        private string? _userQuery;
        private bool _isBusy;
        private bool _hasSearched;
        private bool _hasTags;

        public ObservableCollection<TagModel> Tags
        {
            get => _tags;
            set { _tags = value; OnPropertyChanged(nameof(Tags)); }
        }

        public string? UserQuery
        {
            get => _userQuery;
            set { _userQuery = value; OnPropertyChanged(nameof(UserQuery)); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(IsLoading));
                UpdateState();
            }
        }

        public bool HasSearched
        {
            get => _hasSearched;
            set
            {
                if (_hasSearched == value) return;
                _hasSearched = value;
                OnPropertyChanged(nameof(HasSearched));
                UpdateState();
            }
        }

        // ── Tri-state flags used by the chips area in the XAML ──────────────
        public bool IsLoading => _isBusy;

        public bool HasTags
        {
            get => _hasTags;
            private set
            {
                if (_hasTags == value) return;
                _hasTags = value;
                OnPropertyChanged(nameof(HasTags));
                OnPropertyChanged(nameof(ShowEmptyState));
            }
        }

        public bool ShowEmptyState => !_isBusy && !_hasTags && !_hasSearched;

        public bool IsIdle { get; private set; } = true;
        public bool CanClear { get; private set; }
        public string InputHint { get; private set; } = "Describe your needs...";

        public ICommand GenerateTagsCommand { get; }
        public ICommand ClearCommand { get; }

        public event EventHandler<IEnumerable<string>>? TagsReceived;

        public TagViewModel()
        {
            _tags = new ObservableCollection<TagModel>();
            _tags.CollectionChanged += OnTagsCollectionChanged;
            GenerateTagsCommand = new Command(async () => await GenerateTagsAsync());
            ClearCommand = new Command(Clear);
            UpdateState();
        }

        private void OnTagsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            HasTags = _tags.Count > 0;
            OnPropertyChanged(nameof(ShowEmptyState));
        }

        private void UpdateState()
        {
            IsIdle = !IsBusy && !HasSearched;
            CanClear = HasSearched && !IsBusy;
            InputHint = IsBusy ? "Generating tags..." : "Describe your needs...";

            OnPropertyChanged(nameof(IsIdle));
            OnPropertyChanged(nameof(CanClear));
            OnPropertyChanged(nameof(InputHint));
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(ShowEmptyState));
        }

        public void Clear()
        {
            UserQuery = null;
            Tags.Clear();
            HasSearched = false;
        }

        private CancellationTokenSource? _cts;

        private async Task GenerateTagsAsync() => await GenerateTagsAsync(UserQuery);

        public async Task GenerateTagsAsync(string? query)
        {
            if (string.IsNullOrWhiteSpace(query) || IsBusy) return;

            if (!string.Equals(UserQuery, query, StringComparison.Ordinal))
                UserQuery = query;

            Tags.Clear();        // hide stale chips while loading

            IsBusy = true;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                // ── Call Azure OpenAI ── Same pattern as SmartAIDatePicker.
                string completion = await _aiService.GetCompletion(query, token);
                token.ThrowIfCancellationRequested();

                // Service returns error text in 'completion' AND sets LastError.
                if (!string.IsNullOrWhiteSpace(_aiService.LastError))
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
                        if (page != null)
                            await page.DisplayAlert("AI Error", _aiService.LastError, "OK");
                    });
                    return;
                }

                if (string.IsNullOrWhiteSpace(completion))
                    return;

                // Parse + post-filter to the vocabulary so chips are valid keywords only.
                var tags = ParseAndFilterTags(completion);
                TagsReceived?.Invoke(this, tags);
            }
            catch (OperationCanceledException) { /* ignored */ }
            catch (Exception ex)
            {
                Debug.WriteLine($"AI tag generation failed: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
                    if (page != null)
                        await page.DisplayAlert("Error",
                            $"AI request failed: {ex.Message}", "OK");
                });
            }
            finally
            {
                IsBusy = false;
                HasSearched = true;
            }
        }

        /// <summary>
        /// Parses the AI completion and KEEPS only tags that exist (case-insensitive)
        /// in <see cref="AzureOpenAITagService.TagVocabulary"/>. Returns the canonical
        /// TitleCase form. Single source of truth for both prompt and post-filter.
        /// </summary>
        private static List<string> ParseAndFilterTags(string completion)
        {
            if (string.IsNullOrWhiteSpace(completion))
                return new List<string>();

            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in completion
                .Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s) &&
                            !s.Equals("Empty", StringComparison.OrdinalIgnoreCase) &&
                            !s.StartsWith("Here are", StringComparison.OrdinalIgnoreCase)))
            {
                var cleaned = raw
                    .Trim('•', '-', '*', '.', ')', '[', ']', ',', ';', ':', '\t')
                    .Trim();

                if (string.IsNullOrWhiteSpace(cleaned) || cleaned.Length > 40)
                    continue;

                var canonical = AzureOpenAITagService.TagVocabulary
                    .FirstOrDefault(v => string.Equals(v, cleaned, StringComparison.OrdinalIgnoreCase));

                if (canonical != null && seen.Add(canonical))
                    result.Add(canonical);

                if (result.Count >= 12) break;
            }

            return result;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}