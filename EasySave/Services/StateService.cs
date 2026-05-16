using System.Text.Json;
using EasySave.Models;
using EasySave.Observers;

namespace EasySave.Services
{
    public class StateService : IBackupObserver
    {
        private readonly string _stateFilePath;
        private readonly object _lock = new();
        private Dictionary<string, BackupState> _states = new();

        public StateService()
        {
            _stateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");
        }

        public override void OnJobStarted(string jobName) { }

        public override void OnJobStopped(string jobName)
        {
            lock (_lock)
            {
                if (_states.ContainsKey(jobName))
                {
                    _states[jobName].Status = BackupStatus.Inactive;
                    PersistToFile();
                }
            }
        }

        public override void OnFileProcessed(BackupState state)
        {
            lock (_lock)
            {
                _states[state.JobName] = state;
                PersistToFile();
            }
        }

        public override void OnJobCompleted(string jobName)
        {
            lock (_lock)
            {
                if (_states.ContainsKey(jobName))
                {
                    _states[jobName].Status = BackupStatus.Inactive;
                    _states[jobName].Progress = 100;
                    _states[jobName].RemainingFiles = 0;
                    PersistToFile();
                }
            }
        }

        public override void OnJobError(string jobName, string error)
        {
            lock (_lock)
            {
                if (_states.ContainsKey(jobName))
                {
                    _states[jobName].Status = BackupStatus.Error;
                    PersistToFile();
                }
            }
        }

        public List<BackupState> GetAllStates()
        {
            lock (_lock)
                return _states.Values.ToList();
        }

        private void PersistToFile()
        {
            string json = JsonSerializer.Serialize(
                _states.Values.ToList(),
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(_stateFilePath, json);
        }
    }
}