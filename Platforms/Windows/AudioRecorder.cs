using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using TechnoPoss.Services;

namespace TechnoPoss.Platforms.Windows
{
    public class AudioRecorder : IAudioRecorder
    {
        private MediaCapture? _mediaCapture; // Сделано nullable
        private StorageFile? _file; // Сделано nullable
        public bool IsRecording => _mediaCapture != null;

        public async Task StartRecordingAsync()
        {
            if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                throw new PermissionException("Microphone permission denied.");

            _mediaCapture = new MediaCapture();
            try
            {
                await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации MediaCapture: {ex.Message}");
                throw;
            }

            _file = await KnownFolders.MusicLibrary.CreateFileAsync($"recording_{Guid.NewGuid()}.mp3", CreationCollisionOption.GenerateUniqueName);
            await _mediaCapture.StartRecordToStorageFileAsync(
                MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto),
                _file);
        }

        public async Task<string> StopRecordingAsync()
        {
            if (_mediaCapture == null) return string.Empty; // Возвращаем string.Empty вместо null

            await _mediaCapture.StopRecordAsync();
            _mediaCapture.Dispose();
            _mediaCapture = null;
            var result = _file?.Path ?? string.Empty; // Безопасно получаем путь
            _file = null; // Сбрасываем поле
            return result;
        }
    }
}