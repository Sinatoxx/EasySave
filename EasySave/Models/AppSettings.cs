namespace EasySave.Models
{
    public class AppSettings
    {
        public string Language { get; set; } = "en";
        public LogFormat LogFormat { get; set; } = LogFormat.Json;
    }
}