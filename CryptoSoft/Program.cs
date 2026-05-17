gusing System;
using System.IO;

namespace CryptoSoft
{
    class Program
    {
        static int Main(string[] args)
        {
            // L'exécutable attend deux arguments : source et destination
            if (args.Length < 2) return -1;

            string source = args[0];
            string dest = args[1];

            try
            {
                byte[] content = File.ReadAllBytes(source);
                byte[] key = { 0x41, 0x42, 0x43 }; // Clé de cryptage simple (ABC)

                // Logique de cryptage XOR (très rapide)
                for (int i = 0; i < content.Length; i++)
                {
                    content[i] = (byte)(content[i] ^ key[i % key.Length]);
                }

                File.WriteAllBytes(dest, content);
                return 0; // Succès !
            }
            catch
            {
                return -1; // Erreur (ex: fichier utilisé)
            }
        }
    }
}