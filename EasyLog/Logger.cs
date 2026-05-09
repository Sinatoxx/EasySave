using System.Text.Json;
using System.Xml.Serialization;

namespace EasyLog
{
    public enum LogFormat { JSON, XML }

    public class Logger
    {
        private readonly string _logDirectory;
        private LogFormat _format;

        public Logger(LogFormat format = LogFormat.JSON)
        {
            _format = format;
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            EnsureDirectory(_logDirectory);
        }

        public void SetFormat(LogFormat format) => _format = format;

        public void WriteEntry(LogEntry entry)
        {
            if (_format == LogFormat.XML)
                WriteXml(entry);
            else
                WriteJson(entry);
        }

        private void WriteJson(LogEntry entry)
        {
            string path = GetDailyLogPath("json");
            List<LogEntry> entries = new();

            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path);
                entries = JsonSerializer.Deserialize<List<LogEntry>>(existing) ?? new();
            }

            entries.Add(entry);
            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private void WriteXml(LogEntry entry)
        {
            string path = GetDailyLogPath("xml");
            List<LogEntry> entries = new();

            if (File.Exists(path))
            {
                XmlSerializer serializer = new(typeof(List<LogEntry>));
                using FileStream fs = new(path, FileMode.Open);
                entries = (List<LogEntry>?)serializer.Deserialize(fs) ?? new();
            }

            entries.Add(entry);

            XmlSerializer writer = new(typeof(List<LogEntry>));
            using FileStream output = new(path, FileMode.Create);
            writer.Serialize(output, entries);
        }

        private string GetDailyLogPath(string extension)
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "." + extension;
            return Path.Combine(_logDirectory, fileName);
        }

        private void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
