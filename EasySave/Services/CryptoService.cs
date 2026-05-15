using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasySave.Services
{
    public class CryptoService
    {
        private List<string> _extensions = new();

        // Permet de définir quelles extensions on veut crypter (ex: .txt, .docx)
        public void Configure(List<string> extensions) => _extensions = extensions;

        // Vérifie si le fichier actuel doit passer par CryptoSoft
        public bool MustEncrypt(string filePath)
        {
            return _extensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        private List<string> _priorityExtensions = new();
        public void ConfigurePriorities(List<string> extensions) => _priorityExtensions = extensions;

        public bool IsPriority(string filePath)
        {
            return _priorityExtensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        public long Encrypt(string src, string dst)
        {
            // 1. On cherche l'exécutable CryptoSoft dans le dossier du logiciel
            string cryptoSoftPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");

            // Si le logiciel de cryptage est introuvable, on renvoie un code erreur
            if (!File.Exists(cryptoSoftPath)) return -1;

            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                // 2. Configuration du lancement de CryptoSoft.exe
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = cryptoSoftPath,
                    // Utilisation des bons noms de variables : src et dst
                    Arguments = $"\"{src}\" \"{dst}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                // 3. Exécution du processus de cryptage
                using (Process? process = Process.Start(startInfo))
                {
                    process?.WaitForExit(); // On attend la fin du cryptage
                    sw.Stop();

                    if (process?.ExitCode == 0)
                    {
                        return sw.ElapsedMilliseconds; // Succès : on renvoie le temps > 0
                    }
                    return -1; // Erreur : CryptoSoft a renvoyé un code d'échec
                }
            }
            catch
            {
                return -1; // Erreur : Problème d'accès au fichier ou crash
            }
        }
    }
}