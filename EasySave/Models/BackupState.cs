namespace EasySave.Models
{
    public class BackupState
    {
        public string JobName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public BackupStatus Status { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int RemainingFiles { get; set; }
        public long RemainingSize { get; set; }
        public double Progress { get; set; }
        public string CurrentSourceFile { get; set; } = string.Empty;
        public string CurrentTargetFile { get; set; } = string.Empty;
    }
}