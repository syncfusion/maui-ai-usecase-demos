using System.Globalization;
using Syncfusion.Maui.Inputs;

namespace SmartAIComboBox.SmartAIComboBox;

/// <summary>Inverts a bool for IsEnabled bindings during AI calls.</summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}

public partial class SmartAIComboBoxPage : ContentPage
{
    public SmartAIComboBoxPage()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is FoodViewModel vm)
        {
            vm.AISelectionReceived -= OnAISelectionReceived;
            vm.AISelectionReceived += OnAISelectionReceived;
        }
    }

    /// <summary>Hides the query-hint once the user starts typing (Entry has TextChanged).</summary>
    private void OnQueryTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue ?? string.Empty;
        queryInputLayout.ShowHint = string.IsNullOrEmpty(text);
    }

    /// <summary>Enter key while editing the query Entry → fire search.</summary>
    private void OnQueryCompleted(object? sender, EventArgs e) => RunAISearch();

    /// <summary>Search 🔍 button tapped → fire search.</summary>
    private void OnSearchClicked(object? sender, EventArgs e) => RunAISearch();

    /// <summary>Clear ✕ button tapped → reset input + combo selections.</summary>
    private void OnClearClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not FoodViewModel vm)
            return;

        vm.Clear(); // clears UserQuery, unchecks all, empties SelectedItems, HasSearched=false

        queryInputLayout.ShowHint = true;
        queryEntry.Text = string.Empty;
        comboBox.IsDropDownOpen = false;
    }

    private void RunAISearch()
    {
        if (BindingContext is not FoodViewModel vm || vm.IsBusy)
            return;

        string query = queryEntry.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query)) return;

        _ = vm.AskAIAsync(query);
    }

    private void OnAISelectionReceived(object? sender, IEnumerable<string> matchedNames)
    {
        if (BindingContext is not FoodViewModel vm)
            return;

        var nameSet = matchedNames
            .Select(n => n.Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toSelect = vm.Foods
            .Where(f => f.Name != null && nameSet.Contains(f.Name))
            .ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Reset every checkbox first so a previous run doesn't carry over.
            foreach (var f in vm.Foods) f.IsSelected = false;
            vm.SelectedItems.Clear();

            // Check + add only the AI matches.
            foreach (var item in toSelect)
            {
                item.IsSelected = true;
                vm.SelectedItems.Add(item);
            }

            // Open the dropdown so the (checkbox) ItemTemplate is visible.
            comboBox.IsDropDownOpen = true;
        });
    }

    /// <summary>
    /// Fires when the user manually toggles a CheckBox rendered inside a dropdown row.
    /// Keeps <see cref="SfComboBox.SelectedItems"/> in sync with the checkbox state so
    /// the token display updates in real time.
    /// </summary>
    private void OnDropDownItemCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (BindingContext is not FoodViewModel vm)
            return;

        if (sender is CheckBox cb && cb.BindingContext is FoodModel item)
        {
            if (e.Value)
            {
                if (!vm.SelectedItems.Contains(item))
                    vm.SelectedItems.Add(item);
            }
            else
            {
                if (vm.SelectedItems.Contains(item))
                    vm.SelectedItems.Remove(item);
            }
        }
    }
}