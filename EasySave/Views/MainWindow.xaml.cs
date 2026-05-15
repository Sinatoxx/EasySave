using System.Windows;
using System.Windows.Controls;
using EasySave.ViewModels;
using EasySave.Services;
using EasySave.Models;
using EasyLog;

namespace EasySaveGUI
{
    public partial class MainWindow : Window
    {
        private readonly BackupManagerViewModel _viewModel;
        private BusinessAppWatcher? _businessWatcher;
        public MainWindow()
        {
            InitializeComponent();

            Logger logger = new Logger();
            BusinessAppService businessService = new BusinessAppService();
            CryptoService cryptoService = new CryptoService();
            ConfigService configService = new ConfigService();
            LanguageService langService = new LanguageService();
            StateService stateService = new StateService();

            AppSettings settings = configService.LoadSettings();
            businessService.Configure(settings.BusinessSoftwareName);
            cryptoService.Configure(settings.EncryptionExtensions);
            cryptoService.ConfigurePriorities(settings.PriorityExtensions);
            BandwidthLimiter.Configure(settings.MaxParallelFileSizeKB);

            BackupService backupService = new BackupService(logger, businessService, cryptoService);
            _viewModel = new BackupManagerViewModel(backupService, configService, langService, stateService);

            this.DataContext = _viewModel;

            _businessWatcher = new BusinessAppWatcher(businessService, isRunning =>
            {
                if (isRunning) backupService.PauseAll();
                else backupService.ResumeAll();
            });
            _businessWatcher.Start();

            this.Closing += (s, e) => _businessWatcher.Stop();
        }

        private void AddJob_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string source = txtSource.Text.Trim();
            string target = txtTarget.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Champs manquants", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BackupType type = (cmbType.SelectedIndex == 1) ? BackupType.Differential : BackupType.Full;
            _viewModel.AddJob(name, source, target, type);

            txtName.Clear();
            txtSource.Clear();
            txtTarget.Clear();
            cmbType.SelectedIndex = 0;
        }

        private void ExecuteJob_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                _viewModel.ExecuteJob(id);
        }

        private void DeleteJob_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                _viewModel.RemoveJob(id);
        }

        private void PauseJob_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                _viewModel.PauseJob(id);
        }

        private void ResumeJob_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                _viewModel.ResumeJob(id);
        }

        private void StopJob_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                _viewModel.StopJob(id);
        }

        private void ExecuteAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExecuteAll();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
