namespace EasySave.Models
{
    public class AppSettings
    {
        public string Language { get; set; } = "en";
        public LogFormat LogFormat { get; set; } = LogFormat.Json;
        public string BusinessSoftwareName { get; set; } = "";
        public List<string> EncryptionExtensions { get; set; } = new();
    }
}