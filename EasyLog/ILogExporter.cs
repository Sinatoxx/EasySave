public interface ILogExporter
{
    void Export(string path, List<object> entries);
}