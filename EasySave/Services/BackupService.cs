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

        public BackupService(Logger logger)
        {
            _logger = logger;
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
            try
            {
                IBackupStrategy strategy = ResolveStrategy(job.Type);
                StateService stateService = (StateService)_observers.First(o => o is StateService);
                strategy.Execute(job, _logger, stateService);
            }
            catch (Exception ex)
            {
                NotifyJobError(job.Name, ex.Message);
            }
        }

        public void ExecuteRange(List<int> ids)
        {
            foreach (int id in ids)
            {
                BackupJob? job = _jobs.FirstOrDefault(j => j.Id == id);
                if (job != null) Execute(job);
            }
        }

        public void ExecuteAll()
        {
            foreach (BackupJob job in _jobs)
                Execute(job);
        }





        private IBackupStrategy ResolveStrategy(BackupType type)
        {
            if (_strategyMap.TryGetValue(type, out IBackupStrategy? strategy))
                return strategy;
            throw new NotSupportedException($"Backup type {type} is not supported.");
        }

        private void NotifyJobError(string jobName, string error)
        {
            foreach (IBackupObserver observer in _observers)
                observer.OnJobError(jobName, error);
        }
#modification
    }
}