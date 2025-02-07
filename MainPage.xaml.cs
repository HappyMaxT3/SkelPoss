using SkelAppliences.ViewModels;

namespace SkelAppliences;

public partial class MainPage : ContentPage
{
    public MainViewModel ViewModel { get; set; }

    public MainPage()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        BindingContext = ViewModel;
    }

    private void SendMessage_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(MessageEntry.Text))
        {
            ViewModel.SendMessage();
            MessageEntry.Text = "";
        }
    }

    private void RecordVoice_Clicked(object sender, EventArgs e)
    {
        ViewModel.RecordVoice();
    }

    private void OnSwiped(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Right)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}
