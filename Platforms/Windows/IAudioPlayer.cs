using Windows.Media.Playback;
using Windows.Media.Core;
using TechnoPoss.Services;

namespace TechnoPoss.Platforms.Windows
{
    public class AudioPlayer : IAudioPlayer
    {
        public void Play(string filePath)
        {
            var player = new MediaPlayer();
            player.Source = MediaSource.CreateFromUri(new Uri(filePath));
            player.Play();
        }
    }
}