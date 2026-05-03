using System.Text.Json;

namespace EasyLog
{
    public class Logger
    {
        private readonly string _logDirectory;

        public Logger()
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            EnsureDirectory(_logDirectory);
        }

        public void WriteEntry(LogEntry entry)
        {
            string path = GetDailyLogPath();
            List<LogEntry> entries = new List<LogEntry>();

            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path);
                entries = JsonSerializer.Deserialize<List<LogEntry>>(existing) ?? new List<LogEntry>();
            }

            entries.Add(entry);
            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private string GetDailyLogPath()
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            return Path.Combine(_logDirectory, fileName);
        }

        private void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}