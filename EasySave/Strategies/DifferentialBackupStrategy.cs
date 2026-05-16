using System.IO;
using EasyLog;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public override void Execute(
            BackupJob job,
            Logger logger,
            Action<BackupState>? onFileProcessed,
            Action<string>? onJobCompleted,
            BusinessAppService businessAppService,
            CryptoService cryptoService,
            JobController controller)
        {
            if (!Directory.Exists(job.SourcePath))
                throw new DirectoryNotFoundException($"Source not found: {job.SourcePath}");

            Directory.CreateDirectory(job.TargetPath);

            List<FileInfo> files = GetFilesToCopy(job.SourcePath, job.TargetPath);
            int total = files.Count;
            long totalSize = files.Sum(f => f.Length);
            int remaining = total;

            int priorityCount = files.Count(f => cryptoService.IsPriority(f.FullName));
            PriorityCoordinator.RegisterPriorityFiles(priorityCount);

            files = SortByPriority(files, cryptoService);

            foreach (FileInfo file in files)
            {
                controller.CheckPoint();

                bool isPriority = cryptoService.IsPriority(file.FullName);

                if (!isPriority)
                    PriorityCoordinator.WaitUntilNoPriorityPending(controller.CancelToken);

                string relativePath = Path.GetRelativePath(job.SourcePath, file.FullName);
                string targetFile = Path.Combine(job.TargetPath, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                CopyFile(file.FullName, targetFile, job.Name, logger, cryptoService, controller.CancelToken);

                if (isPriority) PriorityCoordinator.OnePriorityDone();

                remaining--;

                onFileProcessed?.Invoke(BuildState(job, remaining, total, totalSize, file.FullName, targetFile));
            }

            onJobCompleted?.Invoke(job.Name);
        }

        protected override List<FileInfo> GetFilesToCopy(string sourcePath, string targetPath)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);
            return sourceDir.GetFiles("*", SearchOption.AllDirectories)
                            .Where(f => {
                                string rel = Path.GetRelativePath(sourcePath, f.FullName);
                                string target = Path.Combine(targetPath, rel);
                                return !File.Exists(target) || f.LastWriteTime > File.GetLastWriteTime(target);
                            }).ToList();
        }
    }
}