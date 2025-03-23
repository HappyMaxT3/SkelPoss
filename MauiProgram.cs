using Microsoft.Extensions.Logging;
using TechnoPoss.Services;
using TechnoPoss.ViewModels;

namespace TechnoPoss
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                    fonts.AddFont("LuckiestGuy-Regular.ttf", "TitleOposs");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if ANDROID
            builder.Services.AddSingleton<IAudioRecorder, TechnoPoss.Platforms.Android.AudioRecorder>();
            builder.Services.AddSingleton<IAudioPlayer, TechnoPoss.Platforms.Android.AudioPlayer>();
#elif MACCATALYST
            builder.Services.AddSingleton<IAudioRecorder, TechnoPoss.Platforms.MacCatalyst.AudioRecorder>();
#elif WINDOWS
            builder.Services.AddSingleton<IAudioRecorder, TechnoPoss.Platforms.Windows.AudioRecorder>();
            builder.Services.AddSingleton<IAudioPlayer, TechnoPoss.Platforms.Windows.AudioPlayer>();
#else
            builder.Services.AddSingleton<IAudioRecorder, UnsupportedPlatformAudioRecorder>();
#endif

            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<NewsPage>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<AboutPage>();

            return builder.Build();
        }
    }

    public class UnsupportedPlatformAudioRecorder : IAudioRecorder
    {
        public bool IsRecording => false;
        public Task StartRecordingAsync() => throw new NotSupportedException("Recording not supported on this platform.");
        public Task<string> StopRecordingAsync() => Task.FromResult(string.Empty);
    }
}