using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySave.Models;
using EasySave.Services;
using EasySave.Observers;

namespace EasySave.ViewModels
{
    public class BackupManagerViewModel : IBackupObserver, INotifyPropertyChanged
    {
        private readonly BackupService _backupService;
        private readonly ConfigService _configService;
        private readonly LanguageService _langService;
        private readonly StateService _stateService;

        public ObservableCollection<BackupJob> Jobs { get; set; }

        private double _currentProgress;
        public double CurrentProgress
        {
            get => _currentProgress;
            set { _currentProgress = value; OnPropertyChanged(); }
        }

        private string? _currentStatus;
        public string? CurrentStatus
        {
            get => _currentStatus;
            set { _currentStatus = value; OnPropertyChanged(); }
        }

        public BackupManagerViewModel(BackupService backupService, ConfigService configService, LanguageService langService, StateService stateService)
        {
            _backupService = backupService;
            _configService = configService;
            _langService = langService;
            _stateService = stateService;

            Jobs = new ObservableCollection<BackupJob>(_configService.LoadJobs());
            _backupService.AddObserver(this);
            _backupService.AddObserver(_stateService);
            _backupService.SetJobs(new List<BackupJob>(Jobs));
        }

        public bool AddJob(string name, string source, string target, BackupType type)
        {
            var newJob = new BackupJob { Id = Jobs.Count + 1, Name = name, SourcePath = source, TargetPath = target, Type = type };
            Jobs.Add(newJob);
            _configService.SaveJobs(new List<BackupJob>(Jobs));
            _backupService.SetJobs(new List<BackupJob>(Jobs));
            return true;
        }

        public void RemoveJob(int id)
        {
            var job = Jobs.FirstOrDefault(j => j.Id == id);
            if (job == null) return;
            Jobs.Remove(job);
            _configService.SaveJobs(new List<BackupJob>(Jobs));
            _backupService.SetJobs(new List<BackupJob>(Jobs));
        }

        public void ExecuteJob(int id)
        {
            var job = Jobs.FirstOrDefault(j => j.Id == id);
            if (job != null) _ = Task.Run(() => _backupService.Execute(job));
        }

        public void PauseJob(int id)
        {
            _backupService.PauseJob(id);
            var job = Jobs.FirstOrDefault(j => j.Id == id);
            if (job != null) System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                job.Phase = JobPhase.Paused;
                job.StatusText = "Paused";
            });
        }

        public void ResumeJob(int id)
        {
            _backupService.ResumeJob(id);
            var job = Jobs.FirstOrDefault(j => j.Id == id);
            if (job != null) System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                job.Phase = JobPhase.Running;
                job.StatusText = "Running...";
            });
        }

        public void StopJob(int id)
        {
            _backupService.StopJob(id);
        }

        public void ExecuteJobs(List<int> ids) => _ = _backupService.ExecuteRange(ids);

        public void ExecuteAll() => _ = _backupService.ExecuteAll();

        public List<BackupState> GetStates() => _stateService.GetAllStates();

        public string Translate(string key) => _langService.Get(key);
        public void SetLanguage(string lang) => _langService.SetLanguage(lang);

        public string GetCurrentLogFormat()
        {
            AppSettings settings = _configService.LoadSettings();
            return settings.LogFormat.ToString();
        }

        public void SetLogFormat(LogFormat format)
        {
            AppSettings settings = _configService.LoadSettings();
            settings.LogFormat = format;
            _configService.SaveSettings(settings);
        }

        public override void OnJobStarted(string jobName)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var job = Jobs.FirstOrDefault(j => j.Name == jobName);
                if (job != null) { job.Phase = JobPhase.Running; job.Progress = 0; job.StatusText = "Running..."; }
            });
        }

        public override void OnJobStopped(string jobName)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var job = Jobs.FirstOrDefault(j => j.Name == jobName);
                if (job != null) { job.Phase = JobPhase.Stopped; job.StatusText = "Stopped"; }
            });
        }

        public override void OnFileProcessed(BackupState state)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var job = Jobs.FirstOrDefault(j => j.Name == state.JobName);
                if (job != null)
                {
                    job.Progress = state.Progress;
                    job.StatusText = $"Copying: {System.IO.Path.GetFileName(state.CurrentSourceFile)}";
                }
            });
        }

        public override void OnJobCompleted(string jobName)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var job = Jobs.FirstOrDefault(j => j.Name == jobName);
                if (job != null) { job.Phase = JobPhase.Completed; job.Progress = 100; job.StatusText = "Completed"; }
            });
        }

        public override void OnJobError(string jobName, string error)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var job = Jobs.FirstOrDefault(j => j.Name == jobName);
                if (job != null) { job.Phase = JobPhase.Error; job.StatusText = $"Error: {error}"; }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
