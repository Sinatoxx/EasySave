using System.Text.Json;

namespace EasyLog
{
    public class JsonLogExporter : ILogExporter
    {
        public void Export(string path, List<LogEntry> entries)
        {
            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
