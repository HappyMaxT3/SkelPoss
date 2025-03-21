using Microsoft.Maui.Controls;

namespace TechnoPoss;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        BindingContext = new AboutViewModel();
    }

    private void OnSwiped(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Right)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}