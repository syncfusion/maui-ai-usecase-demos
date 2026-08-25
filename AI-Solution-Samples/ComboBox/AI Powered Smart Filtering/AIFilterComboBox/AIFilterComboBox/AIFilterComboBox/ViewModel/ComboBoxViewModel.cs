using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AIFilterComboBox.AIFilterComboBox
{
    public class ComboBoxViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ComboBoxModel> foods;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public ObservableCollection<ComboBoxModel> Foods
        {
            get { return foods; }
            set { foods = value; OnPropertyChanged(nameof(Foods)); }
        }
        public ComboBoxViewModel()
        {
            foods = new ObservableCollection<ComboBoxModel>
            {
                new ComboBoxModel { Name = "BBQ Brisket" },
                new ComboBoxModel { Name = "BBQ Pulled Pork" },
                new ComboBoxModel { Name = "BBQ Ribs" },
                new ComboBoxModel { Name = "Bacon Cheeseburger" },
                new ComboBoxModel { Name = "Baked Meatloaf" },
                new ComboBoxModel { Name = "Beef Tacos" },
                new ComboBoxModel { Name = "Cheeseburger" },
                new ComboBoxModel { Name = "Chicken Pot Pie" },
                new ComboBoxModel { Name = "Chicken Tenders" },
                new ComboBoxModel { Name = "Chili Con Carne" },
                new ComboBoxModel { Name = "Country Fried Steak" },
                new ComboBoxModel { Name = "Fried Chicken" },
                new ComboBoxModel { Name = "Grilled Steak" },
                new ComboBoxModel { Name = "Hot Dog" },
                new ComboBoxModel { Name = "Meatball Sub" },
                new ComboBoxModel { Name = "Philly Cheesesteak" },
                new ComboBoxModel { Name = "Rack of Ribs" },
                new ComboBoxModel { Name = "Turkey Club Sandwich" },

                new ComboBoxModel { Name = "Avocado Toast" },
                new ComboBoxModel { Name = "Baked Potato" },
                new ComboBoxModel { Name = "Caesar Salad" },
                new ComboBoxModel { Name = "Cheese Pizza" },
                new ComboBoxModel { Name = "Chips and Guacamole" },
                new ComboBoxModel { Name = "Coleslaw" },
                new ComboBoxModel { Name = "French Fries" },
                new ComboBoxModel { Name = "Garden Salad" },
                new ComboBoxModel { Name = "Grilled Cheese Sandwich" },
                new ComboBoxModel { Name = "Mac and Cheese" },
                new ComboBoxModel { Name = "Onion Rings" },
                new ComboBoxModel { Name = "Veggie Burger" },

                new ComboBoxModel { Name = "Bagel with Cream Cheese" },
                new ComboBoxModel { Name = "Biscuits and Gravy" },
                new ComboBoxModel { Name = "Blueberry Muffin" },
                new ComboBoxModel { Name = "Breakfast Burrito" },
                new ComboBoxModel { Name = "Buttermilk Pancakes" },
                new ComboBoxModel { Name = "Cinnamon Roll" },
                new ComboBoxModel { Name = "Classic Oatmeal" },
                new ComboBoxModel { Name = "Eggs Benedict" },
                new ComboBoxModel { Name = "French Toast" },
                new ComboBoxModel { Name = "Granola Bowl" },
                new ComboBoxModel { Name = "Hash Browns" },
                new ComboBoxModel { Name = "Huevos Rancheros" },
                new ComboBoxModel { Name = "Maple Syrup Waffles" },
                new ComboBoxModel { Name = "Scrambled Eggs" },
                new ComboBoxModel { Name = "Western Omelette" },

                new ComboBoxModel { Name = "Apple Pie" },
                new ComboBoxModel { Name = "Banana Split" },
                new ComboBoxModel { Name = "Brownies" },
                new ComboBoxModel { Name = "Carrot Cake" },
                new ComboBoxModel { Name = "Chocolate Chip Cookies" },
                new ComboBoxModel { Name = "Cinnamon Donuts" },
                new ComboBoxModel { Name = "Classic Cheesecake" },
                new ComboBoxModel { Name = "Funnel Cake" },
                new ComboBoxModel { Name = "Ice Cream Sundae" },
                new ComboBoxModel { Name = "Key Lime Pie" },
                new ComboBoxModel { Name = "New York Cheesecake" },
                new ComboBoxModel { Name = "Peach Cobbler" },
                new ComboBoxModel { Name = "Pecan Pie" },
                new ComboBoxModel { Name = "Red Velvet Cake" },
                new ComboBoxModel { Name = "Strawberry Shortcake" },

                new ComboBoxModel { Name = "Buffalo Chicken Sandwich" },
                new ComboBoxModel { Name = "Buffalo Wings" },
                new ComboBoxModel { Name = "Cajun Shrimp" },
                new ComboBoxModel { Name = "Ghost Pepper Burger" },
                new ComboBoxModel { Name = "Jalapeno Poppers" },
                new ComboBoxModel { Name = "Nashville Hot Chicken" },
                new ComboBoxModel { Name = "Pepper Jack Grilled Cheese" },
                new ComboBoxModel { Name = "Spicy BBQ Ribs" },
                new ComboBoxModel { Name = "Spicy Chicken Wings" },
                new ComboBoxModel { Name = "Spicy Chili" },
                new ComboBoxModel { Name = "Sriracha Burger" },
                new ComboBoxModel { Name = "Texas Chili" },

                new ComboBoxModel { Name = "Chicken McNuggets" },
                new ComboBoxModel { Name = "Corn Dog" },
                new ComboBoxModel { Name = "Crispy Chicken Sandwich" },
                new ComboBoxModel { Name = "Double Burger" },
                new ComboBoxModel { Name = "Fish Fillet Sandwich" },
                new ComboBoxModel { Name = "Large Fries" },
                new ComboBoxModel { Name = "Loaded Fries" },
                new ComboBoxModel { Name = "Mozzarella Sticks" },
                new ComboBoxModel { Name = "Nacho Fries" },
                new ComboBoxModel { Name = "Smash Burger" },
                new ComboBoxModel { Name = "Waffle Fries" },
                new ComboBoxModel { Name = "Whopper Burger" },

                new ComboBoxModel { Name = "Acai Bowl" },
                new ComboBoxModel { Name = "Avocado Salad" },
                new ComboBoxModel { Name = "Berry Smoothie Bowl" },
                new ComboBoxModel { Name = "Grilled Asparagus" },
                new ComboBoxModel { Name = "Grilled Chicken Bowl" },
                new ComboBoxModel { Name = "Kale Salad" },
                new ComboBoxModel { Name = "Overnight Oats" },
                new ComboBoxModel { Name = "Quinoa Salad" },
                new ComboBoxModel { Name = "Roasted Sweet Potato" },
                new ComboBoxModel { Name = "Spinach Salad" },
                new ComboBoxModel { Name = "Turkey Lettuce Wrap" },
                new ComboBoxModel { Name = "Zucchini Noodles" },

                new ComboBoxModel { Name = "Clam Chowder" },
                new ComboBoxModel { Name = "Crab Bisque" },
                new ComboBoxModel { Name = "Crab Cakes" },
                new ComboBoxModel { Name = "Fish and Chips" },
                new ComboBoxModel { Name = "Fish Tacos" },
                new ComboBoxModel { Name = "Fried Shrimp" },
                new ComboBoxModel { Name = "Lobster Bisque" },
                new ComboBoxModel { Name = "Lobster Roll" },
                new ComboBoxModel { Name = "New England Clam Chowder" },
                new ComboBoxModel { Name = "Seafood Gumbo" },
                new ComboBoxModel { Name = "Shrimp and Grits" },
                new ComboBoxModel { Name = "Shrimp Cocktail" },

                new ComboBoxModel { Name = "Arnold Palmer" },
                new ComboBoxModel { Name = "Chocolate Milkshake" },
                new ComboBoxModel { Name = "Classic Lemonade" },
                new ComboBoxModel { Name = "Craft Root Beer" },
                new ComboBoxModel { Name = "Fresh Orange Juice" },
                new ComboBoxModel { Name = "Fruit Punch" },
                new ComboBoxModel { Name = "Hot Chocolate" },
                new ComboBoxModel { Name = "Iced Coffee" },
                new ComboBoxModel { Name = "Iced Tea" },
                new ComboBoxModel { Name = "Mango Smoothie" },
                new ComboBoxModel { Name = "Milkshake" },
                new ComboBoxModel { Name = "Sparkling Water" },
                new ComboBoxModel { Name = "Strawberry Lemonade" },
                new ComboBoxModel { Name = "Sweet Tea" },
                new ComboBoxModel { Name = "Vanilla Milkshake" },

                new ComboBoxModel { Name = "Beef Stew" },
                new ComboBoxModel { Name = "Chicken Alfredo" },
                new ComboBoxModel { Name = "Grilled Salmon" },
                new ComboBoxModel { Name = "Jambalaya" },
                new ComboBoxModel { Name = "Lasagna" },
                new ComboBoxModel { Name = "Macaroni and Cheese" },
                new ComboBoxModel { Name = "Pot Roast" },
                new ComboBoxModel { Name = "Roast Turkey" },
                new ComboBoxModel { Name = "Shepherd's Pie" },
                new ComboBoxModel { Name = "Spaghetti and Meatballs" },
                new ComboBoxModel { Name = "Stuffed Bell Peppers" },
                new ComboBoxModel { Name = "T-Bone Steak" },

                new ComboBoxModel { Name = "Cheese Quesadilla" },
                new ComboBoxModel { Name = "Chicken Quesadilla" },
                new ComboBoxModel { Name = "Chips and Salsa" },
                new ComboBoxModel { Name = "Deviled Eggs" },
                new ComboBoxModel { Name = "Garlic Bread" },
                new ComboBoxModel { Name = "Loaded Nachos" },
                new ComboBoxModel { Name = "Pigs in a Blanket" },
                new ComboBoxModel { Name = "Popcorn" },
                new ComboBoxModel { Name = "Potato Skins" },
                new ComboBoxModel { Name = "Pretzel Bites" },
                new ComboBoxModel { Name = "Spinach Artichoke Dip" },
                new ComboBoxModel { Name = "Stuffed Mushrooms" },
                new ComboBoxModel { Name = "Sweet Potato Fries" },
                new ComboBoxModel { Name = "Tomato Soup" },
                new ComboBoxModel { Name = "Tortilla Chips" },
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
