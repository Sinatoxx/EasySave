using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services
{
    public class ConfigService
    {
        private readonly string _configFilePath;
        private readonly string _settingsFilePath;

        public ConfigService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _configFilePath = Path.Combine(baseDir, "config.json");
            _settingsFilePath = Path.Combine(baseDir, "settings.json");
            EnsureFileExists(_configFilePath, "[]");
            EnsureFileExists(_settingsFilePath, "{\"LogFormat\":\"JSON\"}");
        }

        public List<BackupJob> LoadJobs()
        {
            string json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new();
        }

        public void SaveJobs(List<BackupJob> jobs)
        {
            string json = JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configFilePath, json);
        }

        public string GetLogFormat()
        {
            string json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return settings != null && settings.TryGetValue("LogFormat", out string? format) ? format : "JSON";
        }

        public void SetLogFormat(string format)
        {
            var settings = new Dictionary<string, string> { { "LogFormat", format } };
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }

        private void EnsureFileExists(string path, string defaultContent)
        {
            if (!File.Exists(path))
                File.WriteAllText(path, defaultContent);
        }
    }
}
