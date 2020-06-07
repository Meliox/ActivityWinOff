using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ActivityWinOff
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary savedState)
        {
            Process application = null;
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName.ToLower().Contains("activitywinOff"))
                {
                    application = process;
                    break;
                }
            }
            if (application != null && application.Responding)
            {
                application.Kill();
                base.Install(savedState);
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            Process application = null;
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName.ToLower().Contains("activitywinOff"))
                {
                    application = process;
                    break;
                }
            }
            if (application != null && application.Responding)
            {
                application.Kill();
                base.Uninstall(savedState);
            }
        }
    }
}
