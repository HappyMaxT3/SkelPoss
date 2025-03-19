using AudioToolbox;
using AVFoundation;
using Foundation;
using TechnoPoss.Services;

namespace TechnoPoss.Platforms.MacCatalyst
{
    public class AudioRecorder : IAudioRecorder
    {
        private AVAudioRecorder? _recorder; // Nullable
        private string _filePath = string.Empty; // Non-nullable с начальным значением
        public bool IsRecording => _recorder?.Recording ?? false;

        public async Task StartRecordingAsync()
        {
            if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                throw new PermissionException("Microphone permission denied.");

            var session = AVAudioSession.SharedInstance();
            try
            {
                session.SetCategory(AVAudioSessionCategory.Record);
                session.SetActive(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка настройки аудиосессии: {ex.Message}");
                throw;
            }

#pragma warning disable CS8600 // Подавляем предупреждение, так как Path.Combine не вернет null
            _filePath = Path.Combine(FileSystem.CacheDirectory, $"recording_{Guid.NewGuid()}.m4a");
#pragma warning restore CS8600
            var url = NSUrl.FromFilename(_filePath); // _filePath гарантированно не null

            _recorder = AVAudioRecorder.Create(url, settings: new AudioSettings
            {
                Format = AudioFormatType.MPEG4AAC,
                SampleRate = 44100,
                NumberChannels = 1
            }, out NSError error);

            if (_recorder == null || error != null)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания рекордера: {error?.LocalizedDescription ?? "Неизвестная ошибка"}");
                throw new Exception(error?.LocalizedDescription ?? "Failed to create AVAudioRecorder");
            }

            _recorder.PrepareToRecord();
            _recorder.Record();
        }

        public Task<string> StopRecordingAsync()
        {
            if (_recorder == null) return Task.FromResult(string.Empty);

            _recorder.Stop();
            _recorder.Dispose();
            _recorder = null;
            var result = _filePath;
            _filePath = string.Empty;
            return Task.FromResult(result);
        }
    }
}