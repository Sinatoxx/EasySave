using System.Diagnostics;
using System.Linq;

namespace EasySave.Services
{
    public class BusinessAppService
    {
        private string _name = "";

        public void Configure(string name) => _name = name;

        public bool IsBusinessAppRunning()
        {
            if (string.IsNullOrEmpty(_name)) return false;
            return Process.GetProcessesByName(_name).Any();
        }
    }
}
