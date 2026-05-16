using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    public class BackupJob : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public BackupType Type { get; set; }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        private string _statusText = "Idle";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private JobPhase _phase = JobPhase.Idle;
        [JsonIgnore]
        public JobPhase Phase
        {
            get => _phase;
            set
            {
                _phase = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanPause));
                OnPropertyChanged(nameof(CanResume));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        [JsonIgnore] public bool CanStart => Phase == JobPhase.Idle || Phase == JobPhase.Completed || Phase == JobPhase.Stopped || Phase == JobPhase.Error;
        [JsonIgnore] public bool CanPause => Phase == JobPhase.Running;
        [JsonIgnore] public bool CanResume => Phase == JobPhase.Paused;
        [JsonIgnore] public bool CanStop => Phase == JobPhase.Running || Phase == JobPhase.Paused;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}