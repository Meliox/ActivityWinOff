using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections;
using ActivityWinOff;

namespace ActivityWinOff
{
    class Helper
    {
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;

        public static string TempLoggerText = "";

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SetThreadExecutionState([In] uint esFlags);

        public static int CurrentTime()
        {
            return (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string FormatDate(string t)
        {
            DateTime time = new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(t));
            return time.ToString("yyyy-MM-dd" + " - " + "HH:mm:ss");
        }

        public static string FormatDateTime(DateTime t)
        {
            return t.ToString("HH:mm:ss fff", CultureInfo.CurrentUICulture);
        }

        internal static int SecondsFromTime(int userActivityTriggerTime, string userActivityTriggerTimeFormat)
        {
            int t = 0;
            switch (userActivityTriggerTimeFormat)
            {
                case "sec":
                    t = userActivityTriggerTime;
                    break;
                case "min":
                    t = userActivityTriggerTime * 60;
                    break;
                case "hour":
                    t = userActivityTriggerTime * 60 * 60;
                    break;
            }
            return t;
        }

        internal static DateTime CalculateNext(DateTime time, int userActivityTriggerTime, string userActivityTriggerTimeFormat)
        {
            switch (userActivityTriggerTimeFormat)
            {
                case "sec":
                    time.AddSeconds(userActivityTriggerTime);
                    break;
                case "min":
                    time.AddMinutes(userActivityTriggerTime);
                    break;
                case "hour":
                    time.AddHours(userActivityTriggerTime);
                    break;
            }
            return time;
        }

        public static string GetCurrentShell()
        {
            try
            {
                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", false);
                string ShellValue = (string)rkApp.GetValue("Shell");

                // if nothing is found, assume std. shell
                if (ShellValue == null)
                    ShellValue = "explorer.exe";
                return ShellValue;
            }
            catch
            {
                MessageBox.Show("You don't have permission to read registry");
                return "explorer.exe";
            }
        }

        public static void SetCurrentShell(string s)
        {
            try
            {
                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", true);
                // Set key
                rkApp.SetValue("Shell", s);
            }
            catch
            {
                MessageBox.Show("You don't have permission to edit registry");
            }
        }

        public static void DisableScreensaver(bool disable)
        {
            if (disable)
            {
                Logger.add(2, "Disabling screensaver");
                SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED);
            }
            else
            {
                Logger.add(2, "Enable screensaver");
                SetThreadExecutionState(ES_CONTINUOUS);

            }
        }

    }
}
