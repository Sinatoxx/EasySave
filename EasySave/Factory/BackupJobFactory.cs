using EasySave.Models;

namespace EasySave.Factory
{
    public class BackupJobFactory
    {
        public static BackupJob CreateJob(int id, string name, string source, string target, BackupType type)
        {
            ValidatePaths(source, target);
            return new BackupJob
            {
                Id = id,
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = type
            };
        }

        private static void ValidatePaths(string source, string target)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Source path cannot be empty.");
            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentException("Target path cannot be empty.");
            if (!Directory.Exists(source))
                throw new DirectoryNotFoundException($"Source directory not found: {source}");
        }
    }
}