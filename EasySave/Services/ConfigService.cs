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
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            EnsureFileExists();
        }

        public List<BackupJob> LoadJobs()
        {
            string json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
        }

        public void SaveJobs(List<BackupJob> jobs)
        {
            string json = JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configFilePath, json);
        }

        public AppSettings LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                AppSettings defaultSettings = new AppSettings();
                SaveSettings(defaultSettings);
                return defaultSettings;
            }

            string json = File.ReadAllText(_settingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(_configFilePath))
                File.WriteAllText(_configFilePath, "[]");
        }
    }
}