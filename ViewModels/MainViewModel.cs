using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkelAppliences.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

        private string? _messageText;
        public string MessageText
        {
            get => _messageText ?? "";
            set
            {
                _messageText = value;
                OnPropertyChanged();
            }
        }

        public void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(MessageText))
            {
                Messages.Add(new Message { Text = MessageText });
                MessageText = "";
                OnPropertyChanged(nameof(MessageText));
            }
        }

        public void RecordVoice()
        {
            Messages.Add(new Message { Text = "🎤 Голосовое сообщение записано!" });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Message
    {
        public string Text { get; set; } = string.Empty;
    }
}
