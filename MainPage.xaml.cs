using System.Collections.ObjectModel;

namespace SkelAppliences;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Message> Messages { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void SendMessage_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(MessageEntry.Text))
        {
            Messages.Add(new Message { Text = MessageEntry.Text });
            MessageEntry.Text = "";
        }
    }

    private void RecordVoice_Clicked(object sender, EventArgs e)
    {
        Messages.Add(new Message { Text = "🎤 Голосовое сообщение записано!" });
    }

    private void OnSwiped(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Right)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}

public class Message
{
    public string Text { get; set; } = string.Empty;
}
