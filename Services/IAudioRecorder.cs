namespace TechnoPoss.Services
{
    public interface IAudioRecorder
    {
        Task StartRecordingAsync();
        Task<string> StopRecordingAsync();
        bool IsRecording { get; }
    }
}