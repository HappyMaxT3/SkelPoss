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
                .UseMauiApp<App>() // Используем App, а не AppShell
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
#elif MACCATALYST
            builder.Services.AddSingleton<IAudioRecorder, TechnoPoss.Platforms.MacCatalyst.AudioRecorder>();
#elif WINDOWS
            builder.Services.AddSingleton<IAudioRecorder, TechnoPoss.Platforms.Windows.AudioRecorder>();
#else
            builder.Services.AddSingleton<IAudioRecorder, UnsupportedPlatformAudioRecorder>();
#endif

            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<NewsPage>(); // Если NewsPage существует
            builder.Services.AddSingleton<AppShell>(); // Регистрация AppShell

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