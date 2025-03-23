using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using TechnoPoss.Services;

namespace TechnoPoss.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IAudioRecorder _audioRecorder;
        private readonly IAudioPlayer _audioPlayer;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://193.233.48.232:8000"; // сервачок

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
            _httpClient = new HttpClient();
            SendMessageCommand = new Command(async () => await SendMessageAsync());
            RecordVoiceCommand = new Command(async () => await RecordVoiceAsync());
            PlayAudioCommand = new Command<string>(async (path) => await PlayAudioAsync(path));
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(MessageText)) return;

            var userMessage = new Message { Text = MessageText, IsUserMessage = true };
            Messages.Add(userMessage);
            MessageText = "";
            OnPropertyChanged(nameof(MessageText));

            try
            {
                var json = JsonSerializer.Serialize(new
                {
                    userId = "12345",
                    message = userMessage.Text
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/text", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var serverResponse = JsonSerializer.Deserialize<ServerResponse>(responseText);
                    if (serverResponse != null)
                    {
                        var serverMessage = new Message
                        {
                            IsUserMessage = false,
                            Text = serverResponse.Text,
                            IsAudio = !string.IsNullOrEmpty(serverResponse.Audio)
                        };

                        // Если сервер вернул URL аудио, сохраняем его как путь для последующей загрузки
                        if (serverMessage.IsAudio)
                        {
                            serverMessage.AudioFilePath = serverResponse.Audio;
                        }

                        Messages.Add(serverMessage);
                    }
                    else
                    {
                        Messages.Add(new Message { Text = "Ошибка: ответ сервера не распознан.", IsUserMessage = false });
                    }
                }
                else
                {
                    Messages.Add(new Message { Text = $"Ошибка сервера: {response.StatusCode}", IsUserMessage = false });
                }
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { Text = $"Ошибка отправки: {ex.Message}", IsUserMessage = false });
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
                        var userMessage = new Message { IsUserMessage = true, IsAudio = true, AudioFilePath = filePath };
                        Messages.Add(userMessage);
                        await SendVoiceMessageAsync(filePath);
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

        private async Task SendVoiceMessageAsync(string filePath)
        {
            try
            {
                var fileBytes = await File.ReadAllBytesAsync(filePath);

                string userId = "12345";
                content.Add(new StringContent(userId), "userId")
                
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(fileBytes), "file", Path.GetFileName(filePath));

                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/voice", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var serverResponse = JsonSerializer.Deserialize<ServerResponse>(responseText);
                    if (serverResponse != null)
                    {
                        var serverMessage = new Message
                        {
                            IsUserMessage = false,
                            Text = serverResponse.Text,
                            IsAudio = !string.IsNullOrEmpty(serverResponse.Audio)
                        };

                        // Если сервер вернул URL аудио, сохраняем его как путь для последующей загрузки
                        if (serverMessage.IsAudio)
                        {
                            serverMessage.AudioFilePath = serverResponse.Audio;
                        }

                        Messages.Add(serverMessage);
                    }
                    else
                    {
                        Messages.Add(new Message { Text = "Ошибка: ответ сервера не распознан.", IsUserMessage = false });
                    }
                }
                else
                {
                    Messages.Add(new Message { Text = $"Ошибка сервера: {response.StatusCode}", IsUserMessage = false });
                }
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { Text = $"Ошибка отправки голосового сообщения: {ex.Message}", IsUserMessage = false });
            }
        }

        private async Task<string> DownloadAudioAsync(string audioUrl)
        {
            try
            {
                var audioBytes = await _httpClient.GetByteArrayAsync(audioUrl);
                var localPath = Path.Combine(FileSystem.CacheDirectory, $"response_audio_{Guid.NewGuid()}.mp3");
                await File.WriteAllBytesAsync(localPath, audioBytes);
                return localPath;
            }
            catch (Exception ex)
            {
                Messages.Add(new Message { Text = $"Ошибка загрузки аудио: {ex.Message}", IsUserMessage = false });
                return string.Empty;
            }
        }

        private async Task PlayAudioAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    Messages.Add(new Message { Text = "Ошибка: нет аудиофайла для воспроизведения.", IsUserMessage = false });
                    return;
                }

                string localPath = filePath;
                // Если filePath — это URL (начинается с http), загружаем файл
                if (filePath.StartsWith("http"))
                {
                    localPath = await DownloadAudioAsync(filePath);
                    if (string.IsNullOrEmpty(localPath))
                    {
                        return;
                    }
                }

                _audioPlayer.Play(localPath);
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

    public class ServerResponse
    {
        public string Text { get; set; } = string.Empty;
        public string Audio { get; set; } = string.Empty;
    }
}