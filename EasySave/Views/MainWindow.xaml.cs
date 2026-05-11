using System.Windows;
using EasySave.ViewModels;
using EasySave.Services;
using EasyLog;

namespace EasySaveGUI
{
    public partial class MainWindow : Window
    {
        private readonly BackupManagerViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // 1. Initialisation des services (comme dans Program.cs)
            Logger logger = new Logger();
            BusinessAppService businessService = new BusinessAppService();
            CryptoService cryptoService = new CryptoService();

            BackupService backupService = new BackupService(logger, businessService, cryptoService);
            ConfigService configService = new ConfigService();
            LanguageService langService = new LanguageService();

            // 2. Création du ViewModel
            _viewModel = new BackupManagerViewModel(backupService, configService, langService);

            // 3. Liaison avec l'interface (DataBinding)
            this.DataContext = _viewModel;
        }

        // Exemple d'action sur un bouton "Lancer"
        private void ExecuteAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExecuteAll();
        }
    }
}