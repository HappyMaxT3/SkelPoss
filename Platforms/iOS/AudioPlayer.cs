using AVFoundation;
using Foundation;
using TechnoPoss.Services;
using System;

namespace TechnoPoss.Platforms.iOS
{
    public class AudioPlayer : IAudioPlayer
    {
        private AVAudioPlayer? _audioPlayer;

        public void Play(string filePath)
        {
            var url = NSUrl.FromFilename(filePath);
            _audioPlayer = AVAudioPlayer.FromUrl(url, out NSError? error);

            if (_audioPlayer == null || error != null)
            {
                Console.WriteLine($"Ошибка при загрузке аудиофайла: {error?.LocalizedDescription ?? "Неизвестная ошибка"}");
                return;
            }

            if (_audioPlayer.PrepareToPlay())
            {
                _audioPlayer.Play();
            }
            else
            {
                Console.WriteLine("Не удалось подготовить аудиофайл к воспроизведению.");
            }
        }
    }
}