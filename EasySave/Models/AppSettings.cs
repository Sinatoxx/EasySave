namespace EasySave.Models
{
    public class AppSettings
    {
        public string Language { get; set; } = "en";
        public LogFormat LogFormat { get; set; } = LogFormat.Json;
        public string BusinessSoftwareName { get; set; } = "";
        public List<string> EncryptionExtensions { get; set; } = new();
        public string CryptoKey { get; set; } = "ABC";

        // v3.0
        public List<string> PriorityExtensions { get; set; } = new();
        public long MaxParallelFileSizeKB { get; set; } = 1024;
        public LogStorageMode LogStorageMode { get; set; } = LogStorageMode.Local;
        public string DockerServerUrl { get; set; } = "http://localhost:5000";
        public string UserIdentifier { get; set; } = Environment.MachineName;
    }
}