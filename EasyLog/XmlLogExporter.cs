using System.Xml.Serialization;

namespace EasyLog
{
    public class XmlLogExporter : ILogExporter
    {
        public void Export(string path, List<LogEntry> entries)
        {
            XmlSerializer serializer = new(typeof(List<LogEntry>));
            using FileStream output = new(path, FileMode.Create);
            serializer.Serialize(output, entries);
        }
    }
}
