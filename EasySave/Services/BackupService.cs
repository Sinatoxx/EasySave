using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Models;
using EasySave.Observers;
using EasySave.Strategies;

namespace EasySave.Services
{
    public class BackupService
    {
        private readonly Logger _logger;
        private readonly List<IBackupObserver> _observers = new();
        private readonly Dictionary<BackupType, IBackupStrategy> _strategyMap;
        private readonly Dictionary<int, JobController> _controllers = new();
        private List<BackupJob> _jobs = new();

        private readonly BusinessAppService _businessAppService;
        private readonly CryptoService _cryptoService;

        public BackupService(Logger logger, BusinessAppService businessAppService, CryptoService cryptoService)
        {
            _logger = logger;
            _businessAppService = businessAppService;
            _cryptoService = cryptoService;

            _strategyMap = new Dictionary<BackupType, IBackupStrategy>
            {
                { BackupType.Full, new FullBackupStrategy() },
                { BackupType.Differential, new DifferentialBackupStrategy() }
            };
        }

        public void AddObserver(IBackupObserver observer) => _observers.Add(observer);
        public void RemoveObserver(IBackupObserver observer) => _observers.Remove(observer);
        public void SetJobs(List<BackupJob> jobs) => _jobs = jobs;

        public void PauseJob(int id) { if (_controllers.TryGetValue(id, out var ctrl)) ctrl.Pause(); }
        public void ResumeJob(int id) { if (_controllers.TryGetValue(id, out var ctrl)) ctrl.Resume(); }
        public void StopJob(int id) { if (_controllers.TryGetValue(id, out var ctrl)) ctrl.Stop(); }

        public void PauseAll()
        {
            foreach (var ctrl in _controllers.Values) ctrl.Pause();
        }

        public void ResumeAll()
        {
            foreach (var ctrl in _controllers.Values) ctrl.Resume();
        }

        public bool HasActiveJobs() => _controllers.Count > 0;

        public void Execute(BackupJob job)
        {
            var controller = new JobController();
            _controllers[job.Id] = controller;

            NotifyJobStarted(job.Name);

            try
            {
                IBackupStrategy strategy = ResolveStrategy(job.Type);

                strategy.Execute(
                    job, _logger,
                    state => { foreach (var obs in _observers) obs.OnFileProcessed(state); },
                    jobName => { foreach (var obs in _observers) obs.OnJobCompleted(jobName); },
                    _businessAppService, _cryptoService, controller);
            }
            catch (OperationCanceledException)
            {
                if (controller.CancelToken.IsCancellationRequested)
                    NotifyJobStopped(job.Name);
                else
                    NotifyJobError(job.Name, "Backup paused: Business software detected.");
            }
            catch (Exception ex)
            {
                NotifyJobError(job.Name, ex.Message);
            }
            finally
            {
                _controllers.Remove(job.Id);
            }
        }

        public async Task ExecuteRange(List<int> ids)
        {
            List<BackupJob> jobs = ids
                .Select(id => _jobs.FirstOrDefault(j => j.Id == id))
                .Where(j => j != null)
                .ToList()!;

            await Task.WhenAll(jobs.Select(job => Task.Run(() => Execute(job))));
        }

        public async Task ExecuteAll()
        {
            await Task.WhenAll(_jobs.Select(job => Task.Run(() => Execute(job))));
        }

        private IBackupStrategy ResolveStrategy(BackupType type)
        {
            if (_strategyMap.TryGetValue(type, out IBackupStrategy? strategy))
                return strategy;
            throw new NotSupportedException($"Backup type {type} is not supported.");
        }

        private void NotifyJobStarted(string jobName) { foreach (var obs in _observers) obs.OnJobStarted(jobName); }
        private void NotifyJobCompleted(string jobName) { foreach (var obs in _observers) obs.OnJobCompleted(jobName); }
        private void NotifyJobStopped(string jobName) { foreach (var obs in _observers) obs.OnJobStopped(jobName); }
        private void NotifyJobError(string jobName, string error) { foreach (var obs in _observers) obs.OnJobError(jobName, error); }
    }
}