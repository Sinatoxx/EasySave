namespace EasyLog
{
    public interface ILogExporter
    {
        void Export(string path, List<LogEntry> entries);
    }
}
