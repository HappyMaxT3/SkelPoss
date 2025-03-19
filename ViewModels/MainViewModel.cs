using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TechnoPoss.Services;

namespace TechnoPoss.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IAudioRecorder _audioRecorder;
        private readonly IAudioPlayer _audioPlayer;
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

        public bool IsRecording => _audioRecorder.IsRecording;

        public ICommand SendMessageCommand { get; }
        public ICommand RecordVoiceCommand { get; }
        public ICommand PlayAudioCommand { get; } 

        public MainViewModel(IAudioRecorder audioRecorder, IAudioPlayer audioPlayer)
        {
            _audioRecorder = audioRecorder ?? throw new ArgumentNullException(nameof(audioRecorder));
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
            SendMessageCommand = new Command(SendMessage);
            RecordVoiceCommand = new Command(async () => await RecordVoiceAsync());
            PlayAudioCommand = new Command<string>(PlayAudio);
        }

        private void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(MessageText))
            {
                Messages.Add(new Message { Text = MessageText, IsUserMessage = true });
                Messages.Add(new Message { Text = "✅ Сообщение принято! Это ответ.", IsUserMessage = false });
                MessageText = "";
                OnPropertyChanged(nameof(MessageText));
            }
        }

        private async Task RecordVoiceAsync()
        {
            try
            {
                if (!IsRecording)
                {
                    await _audioRecorder.StartRecordingAsync();
                    OnPropertyChanged(nameof(IsRecording));
                }
                else
                {
                    var filePath = await _audioRecorder.StopRecordingAsync();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        Messages.Add(new Message
                        {
                            IsUserMessage = true,
                            IsAudio = true,
                            AudioFilePath = filePath // Сохраняем путь для воспроизведения
                        });
                        Messages.Add(new Message { Text = "✅ Голосовое сообщение принято! Это ответ.", IsUserMessage = false });
                    }
                    OnPropertyChanged(nameof(IsRecording));
                }
            }
            catch (PermissionException ex)
            {
                Messages.Add(new Message { Text = $"Ошибка: {ex.Message}", IsUserMessage = false });
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { Text = $"Ошибка записи: {ex.Message}", IsUserMessage = false });
            }
        }

        private void PlayAudio(string filePath)
        {
            try
            {
                _audioPlayer.Play(filePath);
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { Text = $"Ошибка воспроизведения: {ex.Message}", IsUserMessage = false });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}