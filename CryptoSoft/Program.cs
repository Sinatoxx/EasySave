using System;
using System.IO;
using System.Threading;

namespace CryptoSoft
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2) return -1;

            // Mutex global pour assurer le mono-instance
            using Mutex mutex = new Mutex(false, "Global\\CryptoSoftMonoInstance", out bool createdNew);

            try
            {
                // Attente jusqu'à 30 secondes pour acquérir le mutex
                if (!mutex.WaitOne(TimeSpan.FromSeconds(30)))
                {
                    return -2; // Code erreur : impossible d'acquérir le mutex
                }

                try
                {
                    string source = args[0];
                    string dest = args[1];

                    byte[] content = File.ReadAllBytes(source);
                    byte[] key = { 0x41, 0x42, 0x43 };

                    for (int i = 0; i < content.Length; i++)
                    {
                        content[i] = (byte)(content[i] ^ key[i % key.Length]);
                    }

                    File.WriteAllBytes(dest, content);
                    return 0;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            catch (AbandonedMutexException)
            {
                return -3;
            }
            catch
            {
                return -1;
            }
        }
    }
}