using System.Globalization;
using TechnoPoss.ViewModels;

namespace TechnoPoss;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnSwiped(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Right)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}

public class BoolToMicTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value as bool?) == true ? "■" : "🎙";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}