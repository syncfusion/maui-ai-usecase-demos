using AIFilterAutocomplete.AIFilterAutocomplete.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AIFilterAutocomplete.AIFilterAutocomplete.ViewModel
{
    public class AutocompleteViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<AutocompleteModel> foods;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public ObservableCollection<AutocompleteModel> Foods
        {
            get { return foods; }
            set { foods = value; OnPropertyChanged(nameof(Foods)); }
        }
        public AutocompleteViewModel()
        {
            foods = new ObservableCollection<AutocompleteModel>
            {
                new AutocompleteModel { Name = "BBQ Pulled Pork" },
                new AutocompleteModel { Name = "BBQ Brisket" },
                new AutocompleteModel { Name = "BBQ Ribs" },
                new AutocompleteModel { Name = "Bacon Cheeseburger" },
                new AutocompleteModel { Name = "Baked Meatloaf" },
                new AutocompleteModel { Name = "Beef Tacos" },
                new AutocompleteModel { Name = "Cheeseburger" },
                new AutocompleteModel { Name = "Chicken Pot Pie" },
                new AutocompleteModel { Name = "Chicken Tenders" },
                new AutocompleteModel { Name = "Chili Con Carne" },
                new AutocompleteModel { Name = "Country Fried Steak" },
                new AutocompleteModel { Name = "Fried Chicken" },
                new AutocompleteModel { Name = "Grilled Steak" },
                new AutocompleteModel { Name = "Hot Dog" },
                new AutocompleteModel { Name = "Meatball Sub" },
                new AutocompleteModel { Name = "Philly Cheesesteak" },
                new AutocompleteModel { Name = "Rack of Ribs" },
                new AutocompleteModel { Name = "Turkey Club Sandwich" },

                new AutocompleteModel { Name = "Avocado Toast" },
                new AutocompleteModel { Name = "Baked Potato" },
                new AutocompleteModel { Name = "Caesar Salad" },
                new AutocompleteModel { Name = "Cheese Pizza" },
                new AutocompleteModel { Name = "Chips and Guacamole" },
                new AutocompleteModel { Name = "Coleslaw" },
                new AutocompleteModel { Name = "French Fries" },
                new AutocompleteModel { Name = "Garden Salad" },
                new AutocompleteModel { Name = "Grilled Cheese Sandwich" },
                new AutocompleteModel { Name = "Mac and Cheese" },
                new AutocompleteModel { Name = "Onion Rings" },
                new AutocompleteModel { Name = "Veggie Burger" },

                new AutocompleteModel { Name = "Bagel with Cream Cheese" },
                new AutocompleteModel { Name = "Biscuits and Gravy" },
                new AutocompleteModel { Name = "Blueberry Muffin" },
                new AutocompleteModel { Name = "Breakfast Burrito" },
                new AutocompleteModel { Name = "Buttermilk Pancakes" },
                new AutocompleteModel { Name = "Cinnamon Roll" },
                new AutocompleteModel { Name = "Classic Oatmeal" },
                new AutocompleteModel { Name = "Eggs Benedict" },
                new AutocompleteModel { Name = "French Toast" },
                new AutocompleteModel { Name = "Granola Bowl" },
                new AutocompleteModel { Name = "Hash Browns" },
                new AutocompleteModel { Name = "Huevos Rancheros" },
                new AutocompleteModel { Name = "Maple Syrup Waffles" },
                new AutocompleteModel { Name = "Scrambled Eggs" },
                new AutocompleteModel { Name = "Western Omelette" },

                new AutocompleteModel { Name = "Apple Pie" },
                new AutocompleteModel { Name = "Banana Split" },
                new AutocompleteModel { Name = "Brownies" },
                new AutocompleteModel { Name = "Carrot Cake" },
                new AutocompleteModel { Name = "Chocolate Chip Cookies" },
                new AutocompleteModel { Name = "Cinnamon Donuts" },
                new AutocompleteModel { Name = "Classic Cheesecake" },
                new AutocompleteModel { Name = "Funnel Cake" },
                new AutocompleteModel { Name = "Ice Cream Sundae" },
                new AutocompleteModel { Name = "Key Lime Pie" },
                new AutocompleteModel { Name = "New York Cheesecake" },
                new AutocompleteModel { Name = "Peach Cobbler" },
                new AutocompleteModel { Name = "Pecan Pie" },
                new AutocompleteModel { Name = "Red Velvet Cake" },
                new AutocompleteModel { Name = "Strawberry Shortcake" },

                new AutocompleteModel { Name = "Buffalo Chicken Sandwich" },
                new AutocompleteModel { Name = "Buffalo Wings" },
                new AutocompleteModel { Name = "Cajun Shrimp" },
                new AutocompleteModel { Name = "Ghost Pepper Burger" },
                new AutocompleteModel { Name = "Jalapeno Poppers" },
                new AutocompleteModel { Name = "Nashville Hot Chicken" },
                new AutocompleteModel { Name = "Pepper Jack Grilled Cheese" },
                new AutocompleteModel { Name = "Spicy BBQ Ribs" },
                new AutocompleteModel { Name = "Spicy Chicken Wings" },
                new AutocompleteModel { Name = "Spicy Chili" },
                new AutocompleteModel { Name = "Sriracha Burger" },
                new AutocompleteModel { Name = "Texas Chili" },

                new AutocompleteModel { Name = "Chicken McNuggets" },
                new AutocompleteModel { Name = "Corn Dog" },
                new AutocompleteModel { Name = "Crispy Chicken Sandwich" },
                new AutocompleteModel { Name = "Double Burger" },
                new AutocompleteModel { Name = "Fish Fillet Sandwich" },
                new AutocompleteModel { Name = "Large Fries" },
                new AutocompleteModel { Name = "Loaded Fries" },
                new AutocompleteModel { Name = "Mozzarella Sticks" },
                new AutocompleteModel { Name = "Nacho Fries" },
                new AutocompleteModel { Name = "Smash Burger" },
                new AutocompleteModel { Name = "Waffle Fries" },
                new AutocompleteModel { Name = "Whopper Burger" },
                    
                new AutocompleteModel { Name = "Acai Bowl" },
                new AutocompleteModel { Name = "Avocado Salad" },
                new AutocompleteModel { Name = "Berry Smoothie Bowl" },
                new AutocompleteModel { Name = "Grilled Asparagus" },
                new AutocompleteModel { Name = "Grilled Chicken Bowl" },
                new AutocompleteModel { Name = "Kale Salad" },
                new AutocompleteModel { Name = "Overnight Oats" },
                new AutocompleteModel { Name = "Quinoa Salad" },
                new AutocompleteModel { Name = "Roasted Sweet Potato" },
                new AutocompleteModel { Name = "Spinach Salad" },
                new AutocompleteModel { Name = "Turkey Lettuce Wrap" },
                new AutocompleteModel { Name = "Zucchini Noodles" },
                    
                new AutocompleteModel { Name = "Clam Chowder" },
                new AutocompleteModel { Name = "Crab Bisque" },
                new AutocompleteModel { Name = "Crab Cakes" },
                new AutocompleteModel { Name = "Fish and Chips" },
                new AutocompleteModel { Name = "Fish Tacos" },
                new AutocompleteModel { Name = "Fried Shrimp" },
                new AutocompleteModel { Name = "Lobster Bisque" },
                new AutocompleteModel { Name = "Lobster Roll" },
                new AutocompleteModel { Name = "New England Clam Chowder" },
                new AutocompleteModel { Name = "Seafood Gumbo" },
                new AutocompleteModel { Name = "Shrimp and Grits" },
                new AutocompleteModel { Name = "Shrimp Cocktail" },
                    
                new AutocompleteModel { Name = "Arnold Palmer" },
                new AutocompleteModel { Name = "Chocolate Milkshake" },
                new AutocompleteModel { Name = "Classic Lemonade" },
                new AutocompleteModel { Name = "Craft Root Beer" },
                new AutocompleteModel { Name = "Fresh Orange Juice" },
                new AutocompleteModel { Name = "Fruit Punch" },
                new AutocompleteModel { Name = "Hot Chocolate" },
                new AutocompleteModel { Name = "Iced Coffee" },
                new AutocompleteModel { Name = "Iced Tea" },
                new AutocompleteModel { Name = "Mango Smoothie" },
                new AutocompleteModel { Name = "Milkshake" },
                new AutocompleteModel { Name = "Sparkling Water" },
                new AutocompleteModel { Name = "Strawberry Lemonade" },
                new AutocompleteModel { Name = "Sweet Tea" },
                new AutocompleteModel { Name = "Vanilla Milkshake" },
                    
                new AutocompleteModel { Name = "Beef Stew" },
                new AutocompleteModel { Name = "Chicken Alfredo" },
                new AutocompleteModel { Name = "Grilled Salmon" },
                new AutocompleteModel { Name = "Jambalaya" },
                new AutocompleteModel { Name = "Lasagna" },
                new AutocompleteModel { Name = "Macaroni and Cheese" },
                new AutocompleteModel { Name = "Pot Roast" },
                new AutocompleteModel { Name = "Roast Turkey" },
                new AutocompleteModel { Name = "Shepherd's Pie" },
                new AutocompleteModel { Name = "Spaghetti and Meatballs" },
                new AutocompleteModel { Name = "Stuffed Bell Peppers" },
                new AutocompleteModel { Name = "T-Bone Steak" },
                    
                new AutocompleteModel { Name = "Cheese Quesadilla" },
                new AutocompleteModel { Name = "Chicken Quesadilla" },
                new AutocompleteModel { Name = "Chips and Salsa" },
                new AutocompleteModel { Name = "Deviled Eggs" },
                new AutocompleteModel { Name = "Garlic Bread" },
                new AutocompleteModel { Name = "Loaded Nachos" },
                new AutocompleteModel { Name = "Pigs in a Blanket" },
                new AutocompleteModel { Name = "Popcorn" },
                new AutocompleteModel { Name = "Potato Skins" },
                new AutocompleteModel { Name = "Pretzel Bites" },
                new AutocompleteModel { Name = "Spinach Artichoke Dip" },
                new AutocompleteModel { Name = "Stuffed Mushrooms" },
                new AutocompleteModel { Name = "Sweet Potato Fries" },
                new AutocompleteModel { Name = "Tomato Soup" },
                new AutocompleteModel { Name = "Tortilla Chips" },
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
