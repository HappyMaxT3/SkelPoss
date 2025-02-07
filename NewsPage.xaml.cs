namespace SkelAppliences;

public partial class NewsPage : ContentPage
{
    public NewsPage()
    {
        InitializeComponent();
    }

    private void OnSwiped(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Right)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}
