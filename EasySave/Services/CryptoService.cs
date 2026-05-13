using System;
using System.Diagnostics;
using System.IO;

namespace EasySave.Services
{
    public class CryptoService
    {
        public long Encrypt(string source, string dest)
        {
            // On vérifie si l'exécutable CryptoSoft existe
            // Il doit être dans le même dossier que EasySave.exe
            string cryptoSoftPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");

            if (!File.Exists(cryptoSoftPath)) return -1;

            Stopwatch sw = Stopwatch.StartNew();

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