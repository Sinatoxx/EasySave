namespace EasyLog
{
    public class Logger
    {
        private readonly string _logDirectory;
        private ILogExporter _exporter;

        public Logger()
        {
            _exporter = new JsonLogExporter();
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            EnsureDirectory(_logDirectory);
        }

        public void SetExporter(ILogExporter exporter) => _exporter = exporter;

        public void WriteEntry(LogEntry entry)
        {
            string extension = _exporter is JsonLogExporter ? "json" : "xml";
            string path = GetDailyLogPath(extension);

            List<LogEntry> entries = new();

            if (File.Exists(path))
                entries = LoadExisting(path);

            entries.Add(entry);
            _exporter.Export(path, entries);
        }

        private List<LogEntry> LoadExisting(string path)
        {
            try
            {
                if (_exporter is JsonLogExporter)
                {
                    string json = File.ReadAllText(path);
                    return System.Text.Json.JsonSerializer.Deserialize<List<LogEntry>>(json) ?? new();
                }
                else
                {
                    System.Xml.Serialization.XmlSerializer serializer = new(typeof(List<LogEntry>));
                    using FileStream fs = new(path, FileMode.Open);
                    return (List<LogEntry>?)serializer.Deserialize(fs) ?? new();
                }
            }
            catch
            {
                return new();
            }
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
