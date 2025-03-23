namespace TechnoPoss.ViewModels
{
    public class Message
    {
        public string Text { get; set; } = string.Empty;
        public bool IsUserMessage { get; set; }
        public bool IsAudio { get; set; } 
        public string AudioFilePath { get; set; } = string.Empty; 
    }
}