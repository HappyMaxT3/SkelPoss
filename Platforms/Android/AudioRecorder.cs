using Android.Media;
using TechnoPoss.Services;

namespace TechnoPoss.Platforms.Android
{
    public class AudioRecorder : IAudioRecorder
    {
        private MediaRecorder? _recorder; 
        private string _filePath = string.Empty;
        public bool IsRecording => _recorder != null;

#pragma warning disable CA1422
        public async Task StartRecordingAsync()
        {
            if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                throw new PermissionException("Microphone permission denied.");

            _filePath = Path.Combine(FileSystem.CacheDirectory, $"recording_{Guid.NewGuid()}.mp3");
            _recorder = new MediaRecorder();

            try
            {
                _recorder.SetAudioSource(AudioSource.Mic);
                _recorder.SetOutputFormat(OutputFormat.Mpeg4);
                _recorder.SetAudioEncoder(AudioEncoder.Aac);
                _recorder.SetOutputFile(_filePath);
                _recorder.Prepare();
                _recorder.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка записи: {ex.Message}");
                _recorder?.Release();
                _recorder = null;
                _filePath = string.Empty;
                throw;
            }
        }
#pragma warning restore CA1422

        public Task<string> StopRecordingAsync()
        {
            if (_recorder == null) return Task.FromResult(string.Empty);

            _recorder.Stop();
            _recorder.Release();
            _recorder = null;
            var result = _filePath;
            _filePath = string.Empty;
            return Task.FromResult(result);
        }
    }
}