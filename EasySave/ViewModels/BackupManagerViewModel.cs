using EasyLog;
using EasySave.Factory;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.ViewModels
{
    public class BackupManagerViewModel
    {
        private readonly BackupService _backupService;
        private readonly ConfigService _configService;
        private readonly StateService _stateService;
        private readonly LanguageService _langService;
        private readonly Logger _logger;
        private AppSettings _settings;

        public List<BackupJob> Jobs { get; private set; } = new();
        public int MaxJobs => 5;

        public BackupManagerViewModel(
            BackupService backupService,
            ConfigService configService,
            StateService stateService,
            LanguageService langService,
            Logger logger)
        {
            _backupService = backupService;
            _configService = configService;
            _stateService = stateService;
            _langService = langService;
            _logger = logger;

            _settings = _configService.LoadSettings();
            _langService.SetLanguage(_settings.Language);
            ApplyLogFormat(_settings.LogFormat);

            Jobs = _configService.LoadJobs();
            _backupService.SetJobs(Jobs);
        }

        public bool CanAddJob() => Jobs.Count < MaxJobs;

        public bool AddJob(string name, string source, string target, BackupType type)
        {
            if (!CanAddJob()) return false;
            int id = Jobs.Count > 0 ? Jobs.Max(j => j.Id) + 1 : 1;
            BackupJob job = BackupJobFactory.CreateJob(id, name, source, target, type);
            Jobs.Add(job);
            _configService.SaveJobs(Jobs);
            _backupService.SetJobs(Jobs);
            return true;
        }

        public void RemoveJob(int id)
        {
            BackupJob? job = Jobs.FirstOrDefault(j => j.Id == id);
            if (job == null) return;
            Jobs.Remove(job);
            _configService.SaveJobs(Jobs);
            _backupService.SetJobs(Jobs);
        }

        public void ExecuteJob(int id)
        {
            BackupJob? job = Jobs.FirstOrDefault(j => j.Id == id);
            if (job != null) _backupService.Execute(job);
        }

        public void ExecuteJobs(List<int> ids) => _backupService.ExecuteRange(ids);
        public void ExecuteAll() => _backupService.ExecuteAll();
        public List<BackupState> GetStates() => _stateService.GetAllStates();

        public void SetLanguage(string lang)
        {
            _langService.SetLanguage(lang);
            _settings.Language = lang;
            _configService.SaveSettings(_settings);
        }

        public void SetLogFormat(LogFormat format)
        {
            _settings.LogFormat = format;
            ApplyLogFormat(format);
            _configService.SaveSettings(_settings);
        }

        public LogFormat GetCurrentLogFormat() => _settings.LogFormat;

        private void ApplyLogFormat(LogFormat format)
        {
            ILogExporter exporter = format == LogFormat.Xml
                ? new XmlLogExporter()
                : new JsonLogExporter();
            _logger.SetExporter(exporter);
        }

        public string Translate(string key) => _langService.Translate(key);
    }
}