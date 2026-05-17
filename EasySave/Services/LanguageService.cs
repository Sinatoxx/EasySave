using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.ComponentModel;

namespace EasySave.Services
{
    public class LanguageService : INotifyPropertyChanged
    {
        private string _currentLanguage = "en";
        private Dictionary<string, string> _translations = new();

        public LanguageService()
        {
            LoadLanguage();
        }

        // Cette méthode permet au XAML de faire : {Binding Lang[menu.title]}
        public string this[string key] => Get(key);

        public void LoadLanguage()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{_currentLanguage}.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
                    OnPropertyChanged("Item[]"); // Notifie le WPF que toutes les clés ont changé
                }
            }
            catch (Exception ex) { Console.WriteLine("Language Load Error: " + ex.Message); }
        }

        public void SetLanguage(string lang)
        {
            _currentLanguage = lang.ToLower();
            LoadLanguage();
        }

        public string Get(string key)
        {
            return _translations.ContainsKey(key) ? _translations[key] : key;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}