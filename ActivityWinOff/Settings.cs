using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityWinOff
{
    class Settings
    {
        public static string PathConfig;
        public static string PathLog;

        public static bool NetworkTrafficEnabled;
        public static string NetworkAdapter;
        public static string NetworkTriggerType;
        public static int NetworkTriggerSpeed;
        public static int NetworkTriggerTime;
        public static string NetworkTriggerTimeFormat;
        public static bool NetworkAverage;
        public static double NetworkUpSpeed;
        public static double NetworkDownSpeed;
        public static bool NetworkTriggered;
        public static DateTime NetworkTriggerNext;

        public static string ShutdownType;
        public static bool ShutdownForced;
        public static bool Enabled;
        public static bool RunAtWindowsStart;
        public static bool StartActivated;

        public static bool ActionProgramEnabled;

        public static bool DisableScreensaver;

        public static string ConditionBehavior;

        public static bool UserActivityEnabled;
        public static bool UserActivityMouse;
        public static bool UserActivityKeyboard;
        public static string UserActivityTriggerTimeFormat;
        public static int UserActivityTriggerTime;
        public static DateTime UserActivityNextTrigger;
        public static bool UserActivityShutdown;
        public static DateTime UserActivityLastActivityKeyboard;
        public static DateTime UserActivityLastActivityMouse;

        public static bool TimerEnabled;
        public static int TimerDays;
        public static int TimerHours;
        public static int TimerMins;
        public static int TimerSeconds;
        public static int TimerNextTrigger;
        public static bool TimerShutdown;

        public static bool SpecificTimerEnabled;
        public static DateTime SpecificTimerTime;
        public static bool SpecificShutdown;

        public static bool CPUUtilEnabled;
        public static int CPUUtilLoadBelow;
        public static bool CPUUtilAverage;
        public static int CPUUtilTriggerTime;
        public static string CPUUtilTriggerTimeFormat;
        public static bool CPULoadShutdown;
        public static int CPUUtil;
        public static DateTime CPUUtilNextTrigger;

        public static bool WaitForProgramEnabled;
        public static string WaitForProgram;
        public static int WaitForProgramTriggerTime;
        public static string WaitForProgramTriggerTimeFormat;
        public static DateTime WaitForProgramNextTrigger;
        public static bool WaitForProgramShutdown;

        public static int WarningTime;
        public static bool WarningShowEnabled;
        public static bool WarningCancel;

        public static string CurrentShell;
        public static bool FocusProgramEnabled;
        public static string FocusProgram;
        public static int FocusProgramPoolInterval;
        public static bool EnableLogger;
        public static bool StartupProgramsEnabled;
        public static int LogLevel;
    }
}
