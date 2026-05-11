using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySave.Models;
using EasySave.Services;
using EasySave.Observers;

namespace EasySave.ViewModels
{
    public class BackupManagerViewModel : INotifyPropertyChanged, IBackupObserver
    {
        private readonly BackupService _backupService;
        private readonly ConfigService _configService;
        private readonly LanguageService _langService;

        // Liste illimitée de travaux (ObservableCollection met à jour l'UI automatiquement)
        public ObservableCollection<BackupJob> Jobs { get; set; }

        private double _currentProgress;
        public double CurrentProgress
        {
            get => _currentProgress;
            set { _currentProgress = value; OnPropertyChanged(); }
        }

        private string _currentStatus;
        public string CurrentStatus
        {
            get => _currentStatus;
            set { _currentStatus = value; OnPropertyChanged(); }
        }

        public BackupManagerViewModel(BackupService backupService, ConfigService configService, LanguageService langService)
        {
            _backupService = backupService;
            _configService = configService;
            _langService = langService;

            // Chargement des travaux depuis le JSON
            Jobs = new ObservableCollection<BackupJob>(_configService.LoadJobs());

            // On s'abonne aux notifications du moteur de sauvegarde
            _backupService.AddObserver(this);
        }

        public void AddJob(string name, string source, string target, BackupType type)
        {
            var newJob = new BackupJob { Id = Jobs.Count + 1, Name = name, SourcePath = source, TargetPath = target, Type = type };
            Jobs.Add(newJob);
            _configService.SaveJobs(new List<BackupJob>(Jobs));
        }

        public void ExecuteJob(BackupJob job)
        {
            // Lancer en arrière-plan pour ne pas freezer l'interface WPF
            Task.Run(() => _backupService.Execute(job));
        }

        public void ExecuteAll()
        {
            Task.Run(() => _backupService.ExecuteAll());
        }

        // --- Implémentation de IBackupObserver ---
        public void OnFileProcessed(BackupState state)
        {
            CurrentProgress = state.Progress;
            CurrentStatus = $"Copying: {state.CurrentSourceFile}";
        }

        public void OnJobCompleted(string jobName)
        {
            CurrentStatus = $"{jobName} Completed!";
            CurrentProgress = 100;
        }

        public void OnJobError(string jobName, string error)
        {
            CurrentStatus = $"Error on {jobName}: {error}";
        }

        // --- Logique de mise à jour WPF ---
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}