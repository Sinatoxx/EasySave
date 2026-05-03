using EasySave.Models;

namespace EasySave.Observers
{
    public abstract class IBackupObserver
    {
        public abstract void OnFileProcessed(BackupState state);
        public abstract void OnJobCompleted(string jobName);
        public abstract void OnJobError(string jobName, string error);
    }
}