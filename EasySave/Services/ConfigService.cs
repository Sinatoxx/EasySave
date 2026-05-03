using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services
{
    public class ConfigService
    {
        private readonly string _configFilePath;

        public ConfigService()
        {
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
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

        private void EnsureFileExists()
        {
            if (!File.Exists(_configFilePath))
                File.WriteAllText(_configFilePath, "[]");
        }
    }
}