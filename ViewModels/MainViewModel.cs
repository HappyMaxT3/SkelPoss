using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkelAppliences.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
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
                // Сообщение от пользователя
                Messages.Add(new Message { Text = MessageText, IsUserMessage = true });
                MessageText = "";
                OnPropertyChanged(nameof(MessageText));

                // Ответное сообщение от модели
                Messages.Add(new Message { Text = "✅ Сообщение принято! Это ответ.", IsUserMessage = false });
            }
        }

        public void RecordVoice()
        {
            // Сообщение от пользователя (голосовое)
            Messages.Add(new Message { Text = "🎤 Голосовое сообщение записано (расшифровка)!", IsUserMessage = true });

            MessageText = "";
            OnPropertyChanged(nameof(MessageText));

            // Ответное сообщение от модели
            Messages.Add(new Message { Text = "✅ Голосовое сообщение принято! Это ответ.", IsUserMessage = false });
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
        public bool IsUserMessage { get; set; } // true, если сообщение от пользователя
        public bool IsProduct { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductDetails { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
    }
}