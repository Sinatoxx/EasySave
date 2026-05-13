
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System;
using System.Diagnostics;
using System.IO;


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
            // On vérifie si l'exécutable CryptoSoft existe
            // Il doit être dans le même dossier que EasySave.exe
            string cryptoSoftPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");

            if (!File.Exists(cryptoSoftPath)) return -1;

            Stopwatch sw = Stopwatch.StartNew();

            var p = System.Diagnostics.Process.Start("CryptoSoft.exe", $"\"{src}\" \"{dst}\"");
            p?.WaitForExit();
            sw.Stop();
            return p?.ExitCode == 0 ? sw.ElapsedMilliseconds : -1;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = cryptoSoftPath,
                    Arguments = $"\"{source}\" \"{dest}\"", // On met des guillemets pour les espaces
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit(); // On attend que CryptoSoft finisse
                    sw.Stop();

                    if (process.ExitCode == 0)
                    {
                        return sw.ElapsedMilliseconds; // On renvoie le temps > 0
                    }
                    return -1; // Code erreur si CryptoSoft a échoué
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
<<<<<<< HEAD
=======
//modification
>>>>>>> feature/crypto-service
