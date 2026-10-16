using Microsoft.Maui.Controls;

namespace PatientCompanion.Controls;

public partial class AppPageLayout : ContentView
{
    public static readonly BindableProperty PageContentProperty =
        BindableProperty.Create(
            nameof(PageContent),
            typeof(object),
            typeof(AppPageLayout),
            propertyChanged: (bindable, _, value) =>
            {
                if (bindable is AppPageLayout layout && value is View view)
                    layout.Content = view;
            });

    public object? PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    public AppPageLayout()
    {
        InitializeComponent();
    }
}
