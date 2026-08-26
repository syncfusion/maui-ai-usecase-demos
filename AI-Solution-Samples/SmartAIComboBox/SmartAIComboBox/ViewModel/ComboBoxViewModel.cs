using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace SmartAIComboBox.SmartAIComboBox
{
    public class FoodViewModel : INotifyPropertyChanged
    {
        private readonly ComboBoxAzureAIService _aiService;

        private ObservableCollection<FoodModel> _foods;
        private ObservableCollection<FoodModel> _selectedItems;
        private string? _userQuery;
        private bool _isBusy;
        private bool _hasSearched;

        public ObservableCollection<FoodModel> Foods
        {
            get => _foods;
            set { _foods = value; OnPropertyChanged(nameof(Foods)); }
        }

        public ObservableCollection<FoodModel> SelectedItems
        {
            get => _selectedItems;
            set { _selectedItems = value; OnPropertyChanged(nameof(SelectedItems)); }
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

        public bool IsIdle { get; private set; } = true;
        public bool CanClear { get; private set; }
        public string ComboHint { get; private set; } = "Select fruits";

        public ICommand AskAICommand { get; }
        public ICommand ClearCommand { get; }

        public event EventHandler<IEnumerable<string>>? AISelectionReceived;

        public FoodViewModel()
        {
            _aiService = new ComboBoxAzureAIService();
            _foods = new ObservableCollection<FoodModel>(BuildFoodList());
            _selectedItems = new ObservableCollection<FoodModel>();
            AskAICommand = new Command(async () => await AskAIAsync());
            ClearCommand = new Command(Clear);
            UpdateState();
        }

        private void UpdateState()
        {
            IsIdle = !IsBusy && !HasSearched;
            CanClear = HasSearched && !IsBusy;
            ComboHint = IsBusy ? "Loading..." : "Select fruits";

            OnPropertyChanged(nameof(IsIdle));
            OnPropertyChanged(nameof(CanClear));
            OnPropertyChanged(nameof(ComboHint));
        }

        public void Clear()
        {
            UserQuery = null;
            foreach (var f in Foods) f.IsSelected = false;
            SelectedItems.Clear();
            HasSearched = false;
        }

        /// <summary>
        /// 50 fruits. Each carries a rich nutrition/health tag string so the AI
        /// can map ANY natural-language request (including "healthy fruit",
        /// "fruits for immunity", "fruits for glowing skin", etc.) to real fruits.
        /// </summary>
        private static List<FoodModel> BuildFoodList()
        {
            var fruits = new[]
            {
                ("Apple",                120, "Fiber Kids Sweet Healthy Heart Immunity Digestive Skin"),
                ("Banana",                60, "Smoothie Kids Sweet Energy Fiber Healthy Heart Potassium Bone Digestive"),
                ("Mango",                150, "Smoothie Kids Sweet Tropical Healthy Immunity Skin Eye"),
                ("Orange",                80, "VitaminC Water Citrus Kids Healthy Immunity Heart Skin"),
                ("Pomegranate",          200, "Fiber Antioxidant VitaminC Healthy Heart Immunity Skin Iron"),
                ("Papaya",               100, "Water Digestive Tropical Smoothie Healthy Immunity Skin Eye Heart"),
                ("Pineapple",            120, "VitaminC Water Tropical Smoothie Healthy Immunity Digestive"),
                ("Watermelon",            60, "Water Kids Sweet Tropical Healthy Hydrating LowCalorie Heart"),
                ("Muskmelon",             80, "Water Sweet Kids Healthy Hydrating Immunity VitaminC"),
                ("Strawberry",           250, "VitaminC Berry Smoothie Antioxidant Healthy Skin Immunity Heart"),
                ("Blueberry",            400, "Berry Antioxidant Fiber VitaminC Healthy Brain Skin Heart Immunity"),
                ("Raspberry",            350, "Berry Antioxidant Fiber VitaminC Healthy Skin Heart WeightLoss"),
                ("Blackberry",           300, "Berry Antioxidant Fiber Healthy Skin Immunity Brain"),
                ("Grapes",               120, "Kids Sweet Water Healthy Heart Immunity Antioxidant"),
                ("Guava",                 80, "VitaminC Fiber Kids Sweet Healthy Immunity WeightLoss Eye Skin"),
                ("Kiwi",                 250, "VitaminC Fiber Citrus Antioxidant Healthy Immunity Skin Heart Digestive"),
                ("Pear",                 150, "Fiber Sweet Kids Healthy Digestive Heart WeightLoss"),
                ("Peach",                200, "Sweet Fiber Kids Healthy Skin Immunity LowCalorie"),
                ("Plum",                 180, "Sweet Fiber Kids Antioxidant Healthy Digestive Immunity"),
                ("Cherry",               300, "Antioxidant Sweet Kids Healthy Heart Immunity Sleep Brain"),
                ("Lemon",                100, "VitaminC Citrus LowSugar Healthy Immunity Detox Digestive"),
                ("Lime",                 100, "VitaminC Citrus LowSugar Healthy Immunity Detox Digestive"),
                ("Cranberry",            350, "Antioxidant Berry VitaminC Healthy Immunity Digestive Heart"),
                ("Apricot",              250, "Fiber Sweet Antioxidant Healthy Eye Skin Heart Iron"),
                ("Fig",                  300, "Fiber Sweet Antioxidant Digestive Healthy Bone Iron"),
                ("Dates",                200, "Energy Fiber Sweet Kids Healthy Iron Bone Heart"),
                ("Dragon Fruit",         250, "Fiber Antioxidant Water Tropical Healthy Immunity Heart Digestive Skin"),
                ("Passion Fruit",        300, "VitaminC Fiber Citrus Tropical Healthy Immunity Heart Sleep"),
                ("Avocado",              200, "Fiber HealthyFat LowSugar Healthy Heart Brain Skin Eye Potassium"),
                ("Coconut",              100, "Energy Water Tropical Healthy Heart Immunity Skin Hydrating"),
                ("Sweet Lime (Mosambi)",  120, "VitaminC Water Citrus Healthy Immunity Detox Digestive"),
                ("Lychee",               200, "VitaminC Sweet Tropical Kids Healthy Immunity Heart Antioxidant"),
                ("Jamun (Black Plum)",   150, "Antioxidant LowSugar Fiber Healthy Diabetic WeightLoss Immunity"),
                ("Custard Apple (Sitaphal)", 200, "Sweet Kids Energy Fiber Healthy Bone Eye Immunity Heart"),
                ("Jackfruit",            150, "Energy Fiber Tropical Sweet Healthy Immunity Heart Iron"),
                ("Sapodilla (Chikoo)",    120, "Energy Sweet Kids Fiber Healthy Bone Eye Immunity"),
                ("Star Fruit",           200, "VitaminC Water LowSugar Tropical Healthy WeightLoss Heart Immunity"),
                ("Mulberry",             250, "Antioxidant Berry Fiber Healthy Iron Immunity Heart Skin"),
                ("Amla (Indian Gooseberry)", 80, "VitaminC Antioxidant LowSugar Fiber Healthy Immunity Skin Hair Detox Diabetic"),
                ("Banana (Ripe) (Smoothie)", 60, "Smoothie Kids Energy Sweet Healthy Heart Potassium Bone"),
                ("Mango (Ripe) (Smoothie)", 150, "Smoothie Sweet Tropical Kids Healthy Immunity Skin Eye"),
                ("Papaya (Smoothie)",    100, "Smoothie Water Digestive Tropical Healthy Skin Eye Immunity"),
                ("Pineapple (Smoothie)", 120, "Smoothie VitaminC Water Tropical Healthy Immunity Digestive"),
                ("Strawberry (Smoothie)", 250, "Smoothie VitaminC Berry Antioxidant Healthy Skin Heart Immunity"),
                ("Orange (Smoothie)",     80, "Smoothie VitaminC Water Citrus Healthy Immunity Heart Skin"),
                ("Pear (Ripe)",          150, "Fiber Sweet Kids Healthy Digestive Heart WeightLoss"),
                ("Cantaloupe",           100, "Water Sweet Kids VitaminC Healthy Hydrating Immunity Eye Skin"),
                ("Honeydew Melon",       120, "Water Sweet Kids Healthy Hydrating Immunity LowCalorie"),
                ("Tangerine",            150, "VitaminC Citrus Water Sweet Healthy Immunity Skin Heart"),
                ("Nectarine",            200, "Fiber Sweet Kids Antioxidant Healthy Skin Heart Immunity"),
            };

            var list = new List<FoodModel>();
            foreach (var (name, price, tags) in fruits)
            {
                list.Add(new FoodModel
                {
                    Name = name.Trim(),
                    Price = price,
                    Category = "Fruit",
                    Diet = tags,
                    IsSelected = false
                });
            }
            return list;
        }

        private CancellationTokenSource? _cts;

        private async Task AskAIAsync() => await AskAIAsync(UserQuery);

        /// <summary>
        /// Mirrors SmartAIDatePicker.MainPage.ResolveDateRequestAsync.
        /// Sets IsBusy (toggles loading indicator), calls AI, parses, raises event.
        /// </summary>
        public async Task AskAIAsync(string? query)
        {
            if (string.IsNullOrWhiteSpace(query) || IsBusy) return;

            if (!string.Equals(UserQuery, query, StringComparison.Ordinal))
                UserQuery = query;

            // Equivalent to: SearchButton.IsVisible = false; LoadingIndicator.IsVisible = true;
            IsBusy = true;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                string menu = string.Join("\n",
                    Foods.Select(f => $"{f.Name} | {f.Diet}"));

                string prompt =
                    "From the menu below, choose the fruits that best match the user's natural-language request.\n\n" +
                    "Rules:\n" +
                    "- Return ONLY the exact fruit Names from the menu, one per line.\n" +
                    "- Do NOT include tags, numbers, dashes, bullets, explanations, or any other text.\n" +
                    "- Do NOT include headings like 'Here are the items'.\n" +
                    "- The tags after each fruit describe its nutrition/health profile. Use BOTH the tags AND your general knowledge of fruits to interpret ANY natural-language request — including vague ones like 'healthy fruit', 'fruits good for health', 'nutritious fruits', 'fruits for weight loss', 'fruits for glowing skin', 'fruits for immunity', 'fruits for energy', 'fruits to eat during fever', etc.\n" +
                    "- Tag meanings:\n" +
                    "    Healthy, Immunity, Heart, Skin, Eye, Brain, Bone, Hair, Sleep, Detox, Digestive, Diabetic, WeightLoss, LowCalorie, Hydrating, Iron, Potassium, Energy, Fiber, VitaminC, Antioxidant, Water, Smoothie, Kids, Sweet, Citrus, Tropical, Berry, LowSugar, HealthyFat\n" +
                    "- Intent examples (NOT exhaustive — generalize to similar requests):\n" +
                    "    'healthy fruits' / 'fruits good for health' → fruits tagged Healthy\n" +
                    "    'water-rich fruits' / 'hydrating fruits'   → fruits tagged Water or Hydrating\n" +
                    "    'vitamin c rich fruits'                     → fruits tagged VitaminC\n" +
                    "    'high-fiber fruits'                         → fruits tagged Fiber\n" +
                    "    'fruits for smoothies'                       → fruits tagged Smoothie\n" +
                    "    'kid-friendly fruits'                        → fruits tagged Kids\n" +
                    "    'citrus fruits'                              → fruits tagged Citrus\n" +
                    "    'tropical fruits'                            → fruits tagged Tropical\n" +
                    "    'berry fruits'                               → fruits tagged Berry\n" +
                    "    'low sugar fruits' / 'diabetic fruits'       → fruits tagged LowSugar or Diabetic\n" +
                    "    'sweet fruits'                               → fruits tagged Sweet\n" +
                    "    'antioxidant rich fruits'                    → fruits tagged Antioxidant\n" +
                    "    'energy giving fruits'                       → fruits tagged Energy\n" +
                    "    'fruits for digestion'                       → fruits tagged Digestive\n" +
                    "    'fruits for immunity'                       → fruits tagged Immunity or VitaminC\n" +
                    "    'fruits for glowing skin'                   → fruits tagged Skin or VitaminC\n" +
                    "    'fruits for heart health'                  → fruits tagged Heart\n" +
                    "    'fruits for eyes / vision'                  → fruits tagged Eye\n" +
                    "    'fruits for weight loss'                    → fruits tagged WeightLoss or LowCalorie or Fiber\n" +
                    "    'fruits rich in iron'                       → fruits tagged Iron\n" +
                    "    'fruits rich in potassium'                  → fruits tagged Potassium\n" +
                    "    'fruits for brain / memory'                → fruits tagged Brain\n" +
                    "    'fruits for bones'                          → fruits tagged Bone\n" +
                    "    'fruits to eat at night' / 'fruits for sleep' → fruits tagged Sleep\n" +
                    "- NEVER return 'Empty' for a reasonable natural-language request about fruits. Always pick the closest matching fruits from the menu.\n" +
                    "- If multiple constraints are given, return fruits satisfying ALL of them.\n" +
                    "- Return at most 10 fruits.\n" +
                    "- If ABSOLUTELY no fruit matches (e.g. user asks for a non-fruit), return exactly: Empty\n\n" +
                    "Menu (Name | Tags):\n" + menu + "\n\n" +
                    "User request: " + UserQuery + "\n\n" +
                    "Matching fruits (one per line, exact names from the menu):";

                // Direct AI call — NO WaitForValidationAsync here (matches DatePicker pattern).
                string completion = await _aiService.GetCompletion(prompt, token);
                token.ThrowIfCancellationRequested();

                // The service returns an error string on failure (never throws).
                // Detect it the way the DatePicker's GetDateFromAIAsync does.
                if (string.IsNullOrWhiteSpace(completion) ||
                    completion.StartsWith("HTTP ", StringComparison.OrdinalIgnoreCase) ||
                    completion.StartsWith("Exception:", StringComparison.OrdinalIgnoreCase) ||
                    completion.StartsWith("No 'choices'", StringComparison.OrdinalIgnoreCase) ||
                    completion.StartsWith("Azure returned empty", StringComparison.OrdinalIgnoreCase))
                {
                    string err = !string.IsNullOrWhiteSpace(completion)
                        ? completion
                        : (_aiService.LastError ?? "AI returned an empty response.");
                    Debug.WriteLine($"AI selection failed: {err}");
                    MainThread.BeginInvokeOnMainThread(() =>
                        Application.Current?.Windows?[0]?.Page?.DisplayAlert(
                            "Error", $"AI request failed:\n{err}", "OK"));
                    return;
                }

                // Parse — tolerant to bullets/dashes/headers. NO Take(3).
                var matches = completion
                    .Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s) &&
                                !s.Equals("Empty", StringComparison.OrdinalIgnoreCase) &&
                                !s.StartsWith("Here are", StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Trim('•', '-', '*', '.', ')'))
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(10)
                    .ToList();

                AISelectionReceived?.Invoke(this, matches);
            }
            catch (OperationCanceledException) { /* ignored */ }
            catch (Exception ex)
            {
                Debug.WriteLine($"AI selection failed: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                    Application.Current?.Windows?[0]?.Page?.DisplayAlert(
                        "Error", $"AI request failed: {ex.Message}", "OK")); 
            }
            finally
            {
                // Equivalent to: LoadingIndicator.IsVisible = false; SearchButton.IsVisible = true;
                IsBusy = false;
                HasSearched = true;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}