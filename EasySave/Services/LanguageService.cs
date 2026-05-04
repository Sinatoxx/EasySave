using System.Text.Json;

namespace EasySave.Services
{
    public class LanguageService
    {
        private string _currentLanguage = "en";
        private Dictionary<string, string> _translations = new();
#modification hereejfejfnjenfjenfjebnfjdbezjfnejb
        public LanguageService()
        {
            SetLanguage("english");
        }

        public void SetLanguage(string lang)
        {
            _currentLanguage = lang;
            LoadFromJson(lang);
        }

        public string Translate(string key)
        {
            return _translations.TryGetValue(key, out string? value) ? value : key;
        }

        private void LoadFromJson(string lang)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{lang}.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                                ?? new Dictionary<string, string>();
            }
        }
    }
}