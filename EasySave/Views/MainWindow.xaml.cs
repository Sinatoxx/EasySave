using System.Windows;
using EasySave.ViewModels;
using EasySave.Services;
using EasySave.Models;
using EasyLog;

namespace EasySaveGUI
{
    public partial class MainWindow : Window
    {
        private readonly BackupManagerViewModel _viewModel;

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

            BackupService backupService = new BackupService(logger, businessService, cryptoService);
            _viewModel = new BackupManagerViewModel(backupService, configService, langService, stateService);

            this.DataContext = _viewModel;
        }

        private void ExecuteAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExecuteAll();
        }
    }
}
