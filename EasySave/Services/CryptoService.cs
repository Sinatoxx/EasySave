using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EasySave.Services
{
    internal class CryptoService
    {
    
        public long Encrypt(string src, string dst)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Process p = Process.Start("CryptoSoft.exe", $"\"{src}\" \"{dst}\"");
            p.WaitForExit();
            sw.Stop();
            return p.ExitCode == 0 ? sw.ElapsedMilliseconds : -1;
        }
    }
}
