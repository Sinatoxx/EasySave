using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
#modification
namespace EasySave.Services
{
    internal class BusinessAppService
    {
        
        public bool IsBusinessAppRunning(string name)
        {
            
            return Process.GetProcessesByName(name).Any();
        }
    }
}
