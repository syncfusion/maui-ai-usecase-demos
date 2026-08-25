using Syncfusion.Maui.Core.Carousel;
using Syncfusion.Maui.Inputs;
using System.Collections.ObjectModel;


namespace AIFilterComboBox.AIFilterComboBox
{
    public class ComboBoxCustomFilter : IComboBoxFilterBehavior
    {
        private readonly ComboBoxAzureAIService _azureAIService;

        private readonly ComboBoxViewModel _viewModel;
        public ObservableCollection<ComboBoxModel> Items { get; set; }
        public ObservableCollection<ComboBoxModel> FilteredItems { get; set; } = new ObservableCollection<ComboBoxModel>();
        private CancellationTokenSource? _cancellationTokenSource;

        public ComboBoxCustomFilter(ComboBoxViewModel viewModel)
        {
            _viewModel = viewModel;
            _azureAIService = new ComboBoxAzureAIService();
            Items = new ObservableCollection<ComboBoxModel>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<object?> GetMatchingIndexes(SfComboBox source, ComboBoxFilterInfo filterInfo)
        {
            _viewModel.IsLoading = true;
            Items = (ObservableCollection<ComboBoxModel>)source.ItemsSource;

            //If crendential is not valid the filtering data shows as empty
            if (!_azureAIService.IsCredentialValid || string.IsNullOrEmpty(filterInfo.Text))
            {
                _cancellationTokenSource?.Cancel();
                FilteredItems.Clear();
                return await Task.FromResult(FilteredItems);
            }

            string listItems = string.Join(", ", Items!.Select(c => c.Name));

            // Join the first five items with newline characters for demo output template for AI.           
            string outputTemplate = string.Join("\n", Items.Take(5).Select(c => c.Name));

            //The cancellationToken was used for cancelling the API request if user types continuously.       
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            //Passing the User Input, ItemsSource, Reference output and CancellationToken
            var filteredItems = await FilterItemsUsingAzureAI(filterInfo.Text, listItems, outputTemplate, cancellationToken);

            return await Task.FromResult(filteredItems);
        }

        public async Task<ObservableCollection<ComboBoxModel>> FilterItemsUsingAzureAI(string userInput, string itemsList, string outputTemplate, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                var prompt =
                $"You are a strict food item filtering engine." +
                $"Your job is to filter ONLY relevant items from the provided list." +
                $"STEP 1 — CLASSIFY USER INTENT" +
                $"Extract filters from the user query." +
                $"Possible filters:" +

                $"Diet- veg, non veg, vegan, egg, halal" +
                $"Ingredients:- chicken- beef- mutton- fish- seafood- pork- paneer- cheese- rice- noodles- bread" +
                $"Other:- spicy- sweet- fried- grilled- healthy- breakfast- lunch- dinner- snack- drink- dessert" +

                $"STEP 2 — CLASSIFY EACH ITEM" +
                $"For EACH item, determine:" +
                $"- Is it veg?- Is it non veg ?-Does it contain egg?-Main ingredients- Food category" +
                $"Use the ITEM NAME ONLY." +
                $"Examples:" +
                $"Veg:- Veg Burger- Paneer Burger- Margherita Pizza- Cheese Sandwich" +
                $"Non Veg:- Chicken Burger- Beef Burger- Fish Fry- Mutton Biryani- Pepper Chicken" +

                $"IMPORTANT:- Cheeseburger is NOT automatically non veg-Only treat an item as non veg if the name explicitly contains: chicken, beef, mutton, fish, seafood, prawn, shrimp, pork, meat, turkey, lamb" +
                $"If meat is NOT explicitly mentioned, DO NOT assume non veg." +
                $"STEP 3 — HARD FILTERING (MANDATORY)" +
                $"Apply strict exclusions BEFORE ranking." +
                $"RULES:" +
                $"1. If user asks 'veg': EXCLUDE any item containing: chicken, beef, mutton, fish, seafood, prawn, shrimp, pork, meat, egg" +
                $"2. If user asks 'non veg':INCLUDE ONLY items explicitly containing:chicken, beef, mutton, fish, seafood, prawn, shrimp, pork, meat, egg, lamb, turkey. EXCLUDE:paneer,cheese,veg,vegetarian,vegan,margherita" +

                $" 3. NEVER infer meat from category names like: burger, pizza, sandwich, noodles, rice" +
                $" 4. If multiple filters exist, ALL must match." +

                $" STEP 4 — MATCHING PRIORITY" +
                $" Priority order:" +
                $" 1. Exact item name match" +
                $" 2.Fuzzy / spelling similarity" +
                $" 3.Full filter match" +
                $" 4.Partial match" +

                $"Partial match is allowed ONLY if diet rules are satisfied." +
                $"STEP 5 — OUTPUT RULES" +
                $"- Return ONLY item names from the list, One per line, No numbering, No explanation, No extra text, NEVER generate new items, If no items match, return exactly: Empty" +

             $" User Input: {userInput} " +
             $" List of Items: {itemsList} " +
             $" Expected Output Format: {outputTemplate}";

                var completion = await _azureAIService.GetCompletion(prompt, cancellationToken);

                var filteredItems = completion.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();

                if (FilteredItems.Count > 0)
                    FilteredItems.Clear();
                FilteredItems.AddRange(
                        Items
                        .Where(i => filteredItems.Any(item => i.Name!.StartsWith(item))));

                cancellationToken.ThrowIfCancellationRequested();
            }
            return FilteredItems;
        }
    }
}
