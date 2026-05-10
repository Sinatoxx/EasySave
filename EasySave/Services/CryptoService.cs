using System.Diagnostics;
namespace EasySaveApp.Services
{
    public class CryptoService
    {
        public long Encrypt(string source, string dest)
        {
            Stopwatch sw = Stopwatch.StartNew();
            // Appel de l'exécutable CryptoSoft
            Process p = Process.Start("CryptoSoft.exe", $"\"{source}\" \"{dest}\"");
            p.WaitForExit();
            sw.Stop();
            return p.ExitCode == 0 ? sw.ElapsedMilliseconds : -1;
        }
    }
}