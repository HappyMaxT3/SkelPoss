namespace TechnoPoss.ViewModels
{
    public class Message
    {
        public string Text { get; set; } = string.Empty;
        public bool IsUserMessage { get; set; }
        public bool IsProduct { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductDetails { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public bool IsAudio { get; set; } 
        public string AudioFilePath { get; set; } = string.Empty; 
    }
}