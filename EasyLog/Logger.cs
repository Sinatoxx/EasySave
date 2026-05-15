using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace EasyLog
{
    public class Logger
    {
        private readonly string _logDirectory;
        private ILogExporter _exporter;
        private static readonly object _fileLock = new();
        private static readonly HttpClient _httpClient = new();

        public enum StorageMode { Local, Docker, Both }
        private StorageMode _storageMode = StorageMode.Local;
        private string _dockerUrl = "http://localhost:5000";
        private string _userId = Environment.MachineName;

        public Logger()
        {
            _exporter = new JsonLogExporter();
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            EnsureDirectory(_logDirectory);
        }

        public void SetExporter(ILogExporter exporter) => _exporter = exporter;

        public void ConfigureStorage(StorageMode mode, string dockerUrl, string userId)
        {
            _storageMode = mode;
            _dockerUrl = dockerUrl;
            _userId = userId;
        }

        public void WriteEntry(LogEntry entry)
        {
            if (_storageMode == StorageMode.Local || _storageMode == StorageMode.Both)
                WriteLocal(entry);

            if (_storageMode == StorageMode.Docker || _storageMode == StorageMode.Both)
                _ = Task.Run(() => SendToDocker(entry));
        }

        private void WriteLocal(LogEntry entry)
        {
            lock (_fileLock)
            {
                string extension = _exporter is JsonLogExporter ? "json" : "xml";
                string path = GetDailyLogPath(extension);

                List<LogEntry> entries = new();
                if (File.Exists(path)) entries = LoadExisting(path);

                entries.Add(entry);
                _exporter.Export(path, entries);
            }
        }

        private async Task SendToDocker(LogEntry entry)
        {
            try
            {
                var payload = new
                {
                    UserId = _userId,
                    Entry = entry
                };
                string json = JsonSerializer.Serialize(payload);
                using StringContent content = new(json, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync($"{_dockerUrl}/logs", content);
            }
            catch
            {
                // Échec silencieux si Docker non dispo
            }
        }

        private List<LogEntry> LoadExisting(string path)
        {
            try
            {
                if (_exporter is JsonLogExporter)
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<List<LogEntry>>(json) ?? new();
                }
                else
                {
                    System.Xml.Serialization.XmlSerializer serializer = new(typeof(List<LogEntry>));
                    using FileStream fs = new(path, FileMode.Open);
                    return (List<LogEntry>?)serializer.Deserialize(fs) ?? new();
                }
            }
            catch { return new(); }
        }

        private string GetDailyLogPath(string extension)
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "." + extension;
            return Path.Combine(_logDirectory, fileName);
        }

        private void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}