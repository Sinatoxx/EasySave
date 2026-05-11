using System.IO;
using EasyLog;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        public override void Execute(BackupJob job, Logger logger, StateService? stateService, BusinessAppService businessAppService, CryptoService cryptoService)
        {
            if (!Directory.Exists(job.SourcePath))
                throw new DirectoryNotFoundException($"Source not found: {job.SourcePath}");

            Directory.CreateDirectory(job.TargetPath);

            List<FileInfo> files = GetFilesToCopy(job.SourcePath, job.TargetPath);
            int total = files.Count;
            long totalSize = files.Sum(f => f.Length);
            int remaining = total;

            foreach (FileInfo file in files)
            {
                if (businessAppService.IsBusinessAppRunning())
                    throw new OperationCanceledException("Business software detected during execution.");

                string relativePath = Path.GetRelativePath(job.SourcePath, file.FullName);
                string targetFile = Path.Combine(job.TargetPath, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                stateService?.OnFileProcessed(BuildState(job, remaining, total, totalSize, file.FullName, targetFile));
                CopyFile(file.FullName, targetFile, job.Name, logger, cryptoService);
                remaining--;
            }

            stateService?.OnJobCompleted(job.Name);
        }

        protected override List<FileInfo> GetFilesToCopy(string source, string target)
        {
            return new DirectoryInfo(source)
                .GetFiles("*", SearchOption.AllDirectories)
                .ToList();
        }
    }
}
