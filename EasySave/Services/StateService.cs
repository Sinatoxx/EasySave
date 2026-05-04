using System.Text.Json;
using EasySave.Models;
using EasySave.Observers;

namespace EasySave.Services
{
    public class StateService : IBackupObserver
    {
        private readonly string _stateFilePath;
        private Dictionary<string, BackupState> _states = new();

        public StateService()
        {
            _stateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");
        }

        public override void OnFileProcessed(BackupState state)
        {
            _states[state.JobName] = state;
            PersistToFile();
        }

        public override void OnJobCompleted(string jobName)
        {
            if (_states.ContainsKey(jobName))
            {
                _states[jobName].Status = BackupStatus.Inactive;
                _states[jobName].Progress = 100;
                _states[jobName].RemainingFiles = 0;
                PersistToFile();
            }
        }

        public override void OnJobError(string jobName, string error)
        {
            if (_states.ContainsKey(jobName))
            {
                _states[jobName].Status = BackupStatus.Error;
                PersistToFile();
            }
        }

        public List<BackupState> GetAllStates()
        {
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
using System;

namespace EasySaveApp.Models
{
    public class BackupState
    {
        public string JobName { get; set; }
        public string LastActionTimestamp { get; set; }
        public string Status { get; set; } // Active, Inactive, etc.
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int Progress { get; set; }
        public int RemainingFiles { get; set; }
        public long RemainingSize { get; set; }
        public string CurrentSource { get; set; }
        public string CurrentDest { get; set; }
    }
}