using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace EasySave.Services
{
    public class BusinessAppService
    {
        
        public bool IsBusinessAppRunning(string name)
        {
            
            return Process.GetProcessesByName(name).Any();
        }
    }
}
