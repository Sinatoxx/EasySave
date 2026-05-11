using System.IO;
using EasyLog;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public abstract class IBackupStrategy
    {
        public abstract void Execute(
            BackupJob job,
            Logger logger,
            Action<BackupState>? onFileProcessed,
            Action<string>? onJobCompleted,
            BusinessAppService businessAppService,
            CryptoService cryptoService);

        protected abstract List<FileInfo> GetFilesToCopy(string source, string target);

        protected void CopyFile(string src, string dst, string jobName, Logger logger, CryptoService cryptoService)
        {
            long fileSize = new FileInfo(src).Length;
            long transferTime;
            long cryptoTimeMs = 0;

            try
            {
                DateTime start = DateTime.Now;
                File.Copy(src, dst, true);
                transferTime = (long)(DateTime.Now - start).TotalMilliseconds;

                if (cryptoService.MustEncrypt(src))
                    cryptoTimeMs = cryptoService.Encrypt(src, dst);
            }
            catch
            {
                transferTime = -1;
            }

            logger.WriteEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = jobName,
                SourceFile = src,
                TargetFile = dst,
                FileSize = fileSize,
                TransferTimeMs = transferTime,
                CryptoTimeMs = cryptoTimeMs
            });
        }

        protected BackupState BuildState(BackupJob job, int remaining, int total, long totalSize, string src, string dst)
        {
            return new BackupState
            {
                JobName = job.Name,
                Timestamp = DateTime.Now,
                Status = BackupStatus.Active,
                TotalFiles = total,
                TotalSize = totalSize,
                RemainingFiles = remaining,
                RemainingSize = 0,
                Progress = total > 0 ? Math.Round((double)(total - remaining) / total * 100, 2) : 0,
                CurrentSourceFile = src,
                CurrentTargetFile = dst
            };
        }
    }
}
