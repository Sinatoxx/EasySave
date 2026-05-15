using System.Windows;
using System.Windows.Controls;
using EasySave.Models;
using EasySave.Services;

namespace EasySaveGUI
{
    public partial class SettingsWindow : Window
    {
        private readonly ConfigService _configService;
        private AppSettings _settings;

        public SettingsWindow(ConfigService configService)
        {
            InitializeComponent();
            _configService = configService;
            _settings = _configService.LoadSettings();
            LoadSettings();
        }

        private void LoadSettings()
        {
            cmbLanguage.SelectedIndex = _settings.Language == "fr" ? 1 : 0;
            cmbLogFormat.SelectedIndex = _settings.LogFormat == LogFormat.Xml ? 1 : 0;
            txtBusinessSoftware.Text = _settings.BusinessSoftwareName;
            txtEncryptionExt.Text = string.Join(",", _settings.EncryptionExtensions);
            txtCryptoKey.Text = _settings.CryptoKey;
            txtPriorityExt.Text = string.Join(",", _settings.PriorityExtensions);
            txtMaxFileSize.Text = _settings.MaxParallelFileSizeKB.ToString();
            cmbLogStorage.SelectedIndex = (int)_settings.LogStorageMode;
            txtDockerUrl.Text = _settings.DockerServerUrl;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _settings.Language = ((ComboBoxItem)cmbLanguage.SelectedItem).Tag.ToString()!;
            _settings.LogFormat = ((ComboBoxItem)cmbLogFormat.SelectedItem).Tag.ToString() == "Xml" ? LogFormat.Xml : LogFormat.Json;
            _settings.BusinessSoftwareName = txtBusinessSoftware.Text.Trim();
            _settings.EncryptionExtensions = ParseExtensions(txtEncryptionExt.Text);
            _settings.CryptoKey = string.IsNullOrWhiteSpace(txtCryptoKey.Text) ? "ABC" : txtCryptoKey.Text.Trim();
            _settings.PriorityExtensions = ParseExtensions(txtPriorityExt.Text);

            if (long.TryParse(txtMaxFileSize.Text, out long size) && size > 0)
                _settings.MaxParallelFileSizeKB = size;

            string mode = ((ComboBoxItem)cmbLogStorage.SelectedItem).Tag.ToString()!;
            _settings.LogStorageMode = mode switch
            {
                "Docker" => LogStorageMode.Docker,
                "Both" => LogStorageMode.Both,
                _ => LogStorageMode.Local
            };

            _settings.DockerServerUrl = txtDockerUrl.Text.Trim();

            _configService.SaveSettings(_settings);

            MessageBox.Show("Paramètres enregistrés ! Redémarrez l'application pour appliquer les changements.",
                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private List<string> ParseExtensions(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new List<string>();
            return input.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s.StartsWith(".") ? s : "." + s)
                .ToList();
        }
    }
}