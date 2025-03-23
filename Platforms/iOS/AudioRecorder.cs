using AVFoundation;
using Foundation;
using TechnoPoss.Services;
using System;
using System.Threading.Tasks;
using AudioToolbox;

namespace TechnoPoss.Platforms.iOS
{
    public class AudioRecorder : IAudioRecorder
    {
        private AVAudioRecorder? _recorder; 
        private string _filePath = string.Empty;
        public bool IsRecording => _recorder != null && _recorder.Recording;

        public async Task StartRecordingAsync()
        {
            if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                throw new PermissionException("Microphone permission denied.");

            var audioSession = AVAudioSession.SharedInstance();
            NSError? error;

            audioSession.SetCategory(
                AVAudioSessionCategory.Record,
                0,
                out error
            );

            if (error != null)
            {
                throw new Exception($"Ошибка настройки аудиосессии: {error.LocalizedDescription}");
            }

            audioSession.SetActive(true, out error);
            if (error != null)
            {
                throw new Exception($"Ошибка активации аудиосессии: {error.LocalizedDescription}");
            }

            _filePath = Path.Combine(FileSystem.CacheDirectory, $"recording_{Guid.NewGuid()}.m4a");

            var settings = new AudioSettings
            {
                Format = AudioFormatType.MPEG4AAC,
                SampleRate = 44100,
                NumberChannels = 1,
                AudioQuality = AVAudioQuality.Medium
            };

            var url = NSUrl.FromFilename(_filePath);
            _recorder = AVAudioRecorder.Create(url, settings, out error);

            if (_recorder == null || error != null)
            {
                throw new Exception($"Ошибка создания рекордера: {error?.LocalizedDescription ?? "Неизвестная ошибка"}");
            }

            if (!_recorder.PrepareToRecord())
            {
                throw new Exception("Не удалось подготовить рекордер к записи.");
            }

            if (!_recorder.Record())
            {
                throw new Exception("Не удалось начать запись.");
            }
        }

        public Task<string> StopRecordingAsync()
        {
            if (_recorder == null) return Task.FromResult(string.Empty);

            _recorder.Stop();

            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetActive(false, out NSError? error); 
            if (error != null)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка деактивации аудиосессии: {error.LocalizedDescription}");
            }

            _recorder.Dispose();
            _recorder = null;
            var result = _filePath;
            _filePath = string.Empty;
            return Task.FromResult(result);
        }
    }
}