using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasySave.Services
{
    public class CryptoService
    {
        private List<string> _extensions = new();

        public void Configure(List<string> extensions) => _extensions = extensions;

        public bool MustEncrypt(string filePath)
        {
            return _extensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        public long Encrypt(string src, string dst)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var p = System.Diagnostics.Process.Start("CryptoSoft.exe", $"\"{src}\" \"{dst}\"");
            p?.WaitForExit();
            sw.Stop();
            return p?.ExitCode == 0 ? sw.ElapsedMilliseconds : -1;
        }
    }
}
