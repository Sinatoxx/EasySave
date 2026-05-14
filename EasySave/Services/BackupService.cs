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
        private List<BackupJob> _jobs = new();

        // Nouveaux services V2.0
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

        public void Execute(BackupJob job)
        {
            // V2.0 : Bloquer le lancement si le logiciel métier est déjà ouvert
            if (_businessAppService.IsBusinessAppRunning())
            {
                NotifyJobError(job.Name, "Backup aborted: Business software is currently running.");
                return;
            }

            try
            {
                IBackupStrategy strategy = ResolveStrategy(job.Type);

                strategy.Execute(
                    job, _logger,
                    state => { foreach (var obs in _observers) obs.OnFileProcessed(state); },
                    jobName => { foreach (var obs in _observers) obs.OnJobCompleted(jobName); },
                    _businessAppService, _cryptoService);
            }
            catch (OperationCanceledException)
            {
                NotifyJobError(job.Name, "Backup stopped: Business software detected during execution.");
            }
            catch (Exception ex)
            {
                NotifyJobError(job.Name, ex.Message);
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

        private void NotifyJobCompleted(string jobName)
        {
            foreach (IBackupObserver observer in _observers)
                observer.OnJobCompleted(jobName);
        }

        private void NotifyJobError(string jobName, string error)
        {
            foreach (IBackupObserver observer in _observers)
                observer.OnJobError(jobName, error);
        }
    }
}