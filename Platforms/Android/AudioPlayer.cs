using Android.Media;
using TechnoPoss.Services;

namespace TechnoPoss.Platforms.Android
{
    public class AudioPlayer : IAudioPlayer
    {
        public void Play(string filePath)
        {
            var player = new MediaPlayer();
            player.SetDataSource(filePath);
            player.Prepare();
            player.Start();
        }
    }
}