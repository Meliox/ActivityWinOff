using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActivityWinOff;
namespace ActivityWinOff
{
    class Interface
    {
        public static void UpdateRequired()
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Logger.add(2, "Upgrading config");
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
        }

        public static void LoadSettings()
        {
            Properties.Settings.Default.Reload();
            NetworkAdapter = (string)Properties.Settings.Default["NetworkAdapter"];
            NetworkAdapterTriggerType = (string)Properties.Settings.Default["NetworkAdapterTriggerType"];
            NetworkTriggerSpeed = (int)Properties.Settings.Default["NetworkTriggerSpeed"];
            NetworkTriggerTime = (int)Properties.Settings.Default["NetworkTriggerTime"];
            NetworkTriggerTimeFormat = (string)Properties.Settings.Default["NetworkTriggerTimeFormat"];
            NetworkTrafficEnabled = (bool)Properties.Settings.Default["NetworkTrafficEnabled"];
            ShutdownType = (string)Properties.Settings.Default["ShutdownType"];
            NetworkAverage = (bool)Properties.Settings.Default["NetworkAverage"];
            RunAtWindowsStart = (bool)Properties.Settings.Default["RunAtWindowsStart"];
            ActionProgramEnabled = (bool)Properties.Settings.Default["ActionProgramEnabled"];
            UserActivityMouse = (bool)Properties.Settings.Default["UserActivityMouse"];
            UserActivityKeyboard = (bool)Properties.Settings.Default["UserActivityKeyboard"];
            UserActivityEnabled = (bool)Properties.Settings.Default["UserActivityEnabled"];
            UserActivityTriggerTime = (int)Properties.Settings.Default["UserActivityTriggerTime"];
            UserActivityTriggerTimeFormat = (string)Properties.Settings.Default["UserActivityTriggerTimeFormat"];
            StartActivated = (bool)Properties.Settings.Default["StartActivated"];
            ConditionBehavior = (string)Properties.Settings.Default["ConditionBehavior"];
            TimerEnabled = (bool)Properties.Settings.Default["TimerEnabled"];
            TimerDays = (int)Properties.Settings.Default["TimerDays"];
            TimerHours = (int)Properties.Settings.Default["TimerHours"];
            TimerMins = (int)Properties.Settings.Default["TimerMins"];
            TimerSeconds = (int)Properties.Settings.Default["TimerSeconds"];
            CPUUtilEnabled = (bool)Properties.Settings.Default["CPUUtilEnabled"];
            CPUUtilLoadBelow = (int)Properties.Settings.Default["CPUUtilLoadBelow"];
            CPUUtilAverage = (bool)Properties.Settings.Default["CPUUtilAverage"];
            CPUUtilTriggerTime = (int)Properties.Settings.Default["CPUUtilTriggerTime"];
            CPUUtilTriggerTimeFormat = (string)Properties.Settings.Default["CPUUtilTriggerTimeFormat"];
            WaitForProgramEnabled = (bool)Properties.Settings.Default["WaitForProgramEnabled"];
            WaitForProgram = (string)Properties.Settings.Default["WaitForProgram"];
            WaitForProgramTriggerTime = (int)Properties.Settings.Default["WaitForProgramTriggerTime"];
            WaitForProgramTriggerTimeFormat = (string)Properties.Settings.Default["WaitForProgramTriggerTimeFormat"];
            WarningTime = (int)Properties.Settings.Default["WarningTime"];
            WarningShowEnabled = (bool)Properties.Settings.Default["WarningShowEnabled"];
            SpecificTimerEnabled = (bool)Properties.Settings.Default["SpecificTimerEnabled"];
            //SpecificTimerTime = (DateTime)Properties.Settings.Default["SpecificTimerTime"];
            DisableScreensaver = (bool)Properties.Settings.Default["DisableScreensaver"];
            FocusProgramEnabled = (bool)Properties.Settings.Default["FocusProgramEnabled"];
            FocusProgram = (string)Properties.Settings.Default["FocusProgram"];
            FocusProgramPoolInterval = (int)Properties.Settings.Default["FocusProgramPoolInterval"];
            EnableLogger = (bool)Properties.Settings.Default["EnableLogger"];
            ShutdownForced = (bool)Properties.Settings.Default["ShutdownForced"];
            StartupProgramsEnabled = (bool)Properties.Settings.Default["StartupProgramsEnabled"];
            LogLevel = (int)Properties.Settings.Default["LogLevel"];
            ShellStartProgramEnabled = (bool)Properties.Settings.Default["ShellStartProgramEnabled"];
            ShellStartProgramPath = (string)Properties.Settings.Default["ShellStartProgramPath"];
        }

        public static void SaveSettings()
        {
            Properties.Settings.Default["NetworkAdapter"] = NetworkAdapter;
            Properties.Settings.Default["NetworkAdapterTriggerType"] = NetworkAdapterTriggerType;
            Properties.Settings.Default["NetworkTriggerSpeed"] = NetworkTriggerSpeed;
            Properties.Settings.Default["NetworkTriggerTime"] = NetworkTriggerTime;
            Properties.Settings.Default["NetworkTriggerTimeFormat"] = NetworkTriggerTimeFormat;
            Properties.Settings.Default["NetworkTrafficEnabled"] = NetworkTrafficEnabled;
            Properties.Settings.Default["ShutdownType"] = ShutdownType;
            Properties.Settings.Default["NetworkAverage"] = NetworkAverage;
            Properties.Settings.Default["RunAtWindowsStart"] = RunAtWindowsStart;
            Properties.Settings.Default["ActionProgramEnabled"] = ActionProgramEnabled;
            Properties.Settings.Default["WaitForProgramEnabled"] = WaitForProgramEnabled;
            Properties.Settings.Default["UserActivityMouse"] = UserActivityMouse;
            Properties.Settings.Default["UserActivityKeyboard"] = UserActivityKeyboard;
            Properties.Settings.Default["UserActivityEnabled"] = UserActivityEnabled;
            Properties.Settings.Default["UserActivityTriggerTimeFormat"] = UserActivityTriggerTimeFormat;
            Properties.Settings.Default["UserActivityTriggerTime"] = UserActivityTriggerTime;
            Properties.Settings.Default["StartActivated"] = StartActivated;
            Properties.Settings.Default["ConditionBehavior"] = ConditionBehavior;
            Properties.Settings.Default["TimerEnabled"] = TimerEnabled;
            Properties.Settings.Default["TimerDays"] = TimerDays;
            Properties.Settings.Default["TimerHours"] = TimerHours;
            Properties.Settings.Default["TimerMins"] = TimerMins;
            Properties.Settings.Default["TimerSeconds"] = TimerSeconds;
            Properties.Settings.Default["CPUUtilEnabled"] = CPUUtilEnabled;
            Properties.Settings.Default["CPUUtilLoadBelow"] = CPUUtilLoadBelow;
            Properties.Settings.Default["CPUUtilAverage"] = CPUUtilAverage;
            Properties.Settings.Default["CPUUtilTriggerTime"] = CPUUtilTriggerTime;
            Properties.Settings.Default["CPUUtilTriggerTimeFormat"] = CPUUtilTriggerTimeFormat;
            Properties.Settings.Default["WaitForProgramEnabled"] = WaitForProgramEnabled;
            Properties.Settings.Default["WaitForProgram"] = WaitForProgram;
            Properties.Settings.Default["WaitForProgramTriggerTime"] = WaitForProgramTriggerTime;
            Properties.Settings.Default["WaitForProgramTriggerTimeFormat"] = WaitForProgramTriggerTimeFormat;
            Properties.Settings.Default["WarningTime"] = WarningTime;
            Properties.Settings.Default["WarningShowEnabled"] = WarningShowEnabled;
            Properties.Settings.Default["SpecificTimerEnabled"] = SpecificTimerEnabled;
            Properties.Settings.Default["SpecificTimerTime"] = SpecificTimerTime;
            Properties.Settings.Default["DisableScreensaver"] = DisableScreensaver;
            Properties.Settings.Default["FocusProgramEnabled"] = FocusProgramEnabled;
            Properties.Settings.Default["FocusProgram"] = FocusProgram;
            Properties.Settings.Default["FocusProgramPoolInterval"] = FocusProgramPoolInterval;
            Properties.Settings.Default["EnableLogger"] = EnableLogger;
            Properties.Settings.Default["ShutdownForced"] = ShutdownForced;
            Properties.Settings.Default["StartupProgramsEnabled"] = StartupProgramsEnabled;
            Properties.Settings.Default["LogLevel"] = LogLevel;
            Properties.Settings.Default["ShellStartProgramEnabled"] = ShellStartProgramEnabled;
            Properties.Settings.Default["ShellStartProgramPath"] = ShellStartProgramPath;

            Properties.Settings.Default.Save();
        }

        public static bool NetworkTrafficEnabled { get => Settings.NetworkTrafficEnabled; set => Settings.NetworkTrafficEnabled = value; }
        public static string NetworkAdapter { get => Settings.NetworkAdapter; set => Settings.NetworkAdapter = value; }
        public static string NetworkAdapterTriggerType { get => Settings.NetworkTriggerType; set => Settings.NetworkTriggerType = value; }
        public static int NetworkTriggerSpeed { get => Settings.NetworkTriggerSpeed; set => Settings.NetworkTriggerSpeed = value; }
        public static int NetworkTriggerTime { get => Settings.NetworkTriggerTime; set => Settings.NetworkTriggerTime = value; }
        public static bool NetworkAverage { get => Settings.NetworkAverage; set => Settings.NetworkAverage = value; }
        public static bool NetworkTriggered { get => Settings.NetworkTriggered; set => Settings.NetworkTriggered = value; }
        public static DateTime NetworkTriggerNext { get => Settings.NetworkTriggerNext; set => Settings.NetworkTriggerNext = value; }
        public static string NetworkTriggerTimeFormat { get => Settings.NetworkTriggerTimeFormat; set => Settings.NetworkTriggerTimeFormat = value; }
        public static string ShutdownType { get => Settings.ShutdownType; set => Settings.ShutdownType = value; }
        public static bool Enabled { get => Settings.Enabled; set => Settings.Enabled = value; }
        public static bool RunAtWindowsStart { get => Settings.RunAtWindowsStart; set => Settings.RunAtWindowsStart = value; }
        public static bool ActionProgramEnabled { get => Settings.ActionProgramEnabled; set => Settings.ActionProgramEnabled = value; }
        public static bool UserActivityMouse { get => Settings.UserActivityMouse; set => Settings.UserActivityMouse = value; }
        public static bool UserActivityKeyboard { get => Settings.UserActivityKeyboard; set => Settings.UserActivityKeyboard = value; }
        public static bool UserActivityEnabled { get => Settings.UserActivityEnabled; set => Settings.UserActivityEnabled = value; }
        public static string UserActivityTriggerTimeFormat { get => Settings.UserActivityTriggerTimeFormat; set => Settings.UserActivityTriggerTimeFormat = value; }
        public static int UserActivityTriggerTime { get => Settings.UserActivityTriggerTime; set => Settings.UserActivityTriggerTime = value; }
        public static bool StartActivated { get => Settings.StartActivated; set => Settings.StartActivated = value; }
        public static string ConditionBehavior { get => Settings.ConditionBehavior; set => Settings.ConditionBehavior = value; }
        public static bool TimerEnabled { get => Settings.TimerEnabled; set => Settings.TimerEnabled = value; }
        public static int TimerDays { get => Settings.TimerDays; set => Settings.TimerDays = value; }
        public static int TimerHours { get => Settings.TimerHours; set => Settings.TimerHours = value; }
        public static int TimerMins { get => Settings.TimerMins; set => Settings.TimerMins = value; }
        public static int TimerSeconds { get => Settings.TimerSeconds; set => Settings.TimerSeconds = value; }
        public static bool CPUUtilEnabled { get => Settings.CPUUtilEnabled; set => Settings.CPUUtilEnabled = value; }
        public static int CPUUtilLoadBelow { get => Settings.CPUUtilLoadBelow; set => Settings.CPUUtilLoadBelow = value; }
        public static bool CPUUtilAverage { get => Settings.CPUUtilAverage; set => Settings.CPUUtilAverage = value; }
        public static int CPUUtilTriggerTime { get => Settings.CPUUtilTriggerTime; set => Settings.CPUUtilTriggerTime = value; }
        public static string CPUUtilTriggerTimeFormat { get => Settings.CPUUtilTriggerTimeFormat; set => Settings.CPUUtilTriggerTimeFormat = value; }
        public static bool WaitForProgramEnabled { get => Settings.WaitForProgramEnabled; set => Settings.WaitForProgramEnabled = value; }
        public static string WaitForProgram { get => Settings.WaitForProgram; set => Settings.WaitForProgram = value; }
        public static int WaitForProgramTriggerTime { get => Settings.WaitForProgramTriggerTime; set => Settings.WaitForProgramTriggerTime = value; }
        public static string WaitForProgramTriggerTimeFormat { get => Settings.WaitForProgramTriggerTimeFormat; set => Settings.WaitForProgramTriggerTimeFormat = value; }
        public static int TimerNextTrigger { get => Settings.TimerNextTrigger; set => Settings.TimerNextTrigger = value; }
        public static DateTime UserActivityNextTrigger { get => Settings.UserActivityNextTrigger; set => Settings.UserActivityNextTrigger = Helper.CalculateNext(value, Settings.UserActivityTriggerTime, Settings.UserActivityTriggerTimeFormat); }
        public static bool UserActivityShutdown { get => Settings.UserActivityShutdown; set => Settings.UserActivityShutdown = value; }
        public static DateTime UserActivityLastActivityKeyboard { get => Settings.UserActivityLastActivityKeyboard; set => Settings.UserActivityLastActivityKeyboard = value; }
        public static DateTime UserActivityLastActivityMouse { get => Settings.UserActivityLastActivityMouse; set => Settings.UserActivityLastActivityMouse = value; }
        public static DateTime WaitForProgramNextTrigger { get => Settings.WaitForProgramNextTrigger; set => Settings.WaitForProgramNextTrigger = Helper.CalculateNext(value, Settings.WaitForProgramTriggerTime, Settings.WaitForProgramTriggerTimeFormat); }
        public static bool WaitForProgramShutdown { get => Settings.WaitForProgramShutdown; set => Settings.WaitForProgramShutdown = value; }
        public static bool TimerShutdown { get => Settings.TimerShutdown; set => Settings.TimerShutdown = value; }
        public static bool CPULoadShutdown { get => Settings.CPULoadShutdown; set => Settings.CPULoadShutdown = value; }
        public static int CPUUtil { get => Settings.CPUUtil; set => Settings.CPUUtil = value; }
        public static DateTime CPUUtilNextTrigger { get => Settings.CPUUtilNextTrigger; set => Settings.CPUUtilNextTrigger = Helper.CalculateNext(value, Settings.CPUUtilTriggerTime, Settings.CPUUtilTriggerTimeFormat); }
        public static int WarningTime { get => Settings.WarningTime; set => Settings.WarningTime = value; }
        public static bool WarningShowEnabled { get => Settings.WarningShowEnabled; set => Settings.WarningShowEnabled = value; }
        public static bool WarningCancel { get => Settings.WarningCancel; set => Settings.WarningCancel = value; }
        public static bool SpecificTimerEnabled { get => Settings.SpecificTimerEnabled; set => Settings.SpecificTimerEnabled = value; }
        public static DateTime SpecificTimerTime { get => Settings.SpecificTimerTime; set => Settings.SpecificTimerTime = value; }

        public static double NetworkUpSpeed { get => Settings.NetworkUpSpeed; set => Settings.NetworkUpSpeed = value; }
        public static double NetworkDownSpeed { get => Settings.NetworkDownSpeed; set => Settings.NetworkDownSpeed = value; }

        public static string CurrentShell { get => Settings.CurrentShell; set => Settings.CurrentShell = value; }

        public static string PathConfig { get => Settings.PathConfig; set => Settings.PathConfig = value; }
        public static string PathLog { get => Settings.PathLog; set => Settings.PathLog = value; }

        public static bool DisableScreensaver { get => Settings.DisableScreensaver; set => Settings.DisableScreensaver = value; }
        public static bool SpecificShutdown { get => Settings.SpecificShutdown; set => Settings.SpecificShutdown = value; }
        public static bool FocusProgramEnabled { get => Settings.FocusProgramEnabled; set => Settings.FocusProgramEnabled = value; }
        public static string FocusProgram { get => Settings.FocusProgram; set => Settings.FocusProgram = value; }
        public static int FocusProgramPoolInterval { get => Settings.FocusProgramPoolInterval; set => Settings.FocusProgramPoolInterval = value; }
        public static bool EnableLogger { get => Settings.EnableLogger; set => Settings.EnableLogger = value; }
        public static bool ShutdownForced { get => Settings.ShutdownForced; set => Settings.ShutdownForced = value; }
        public static bool StartupProgramsEnabled { get => Settings.StartupProgramsEnabled; set => Settings.StartupProgramsEnabled = value; }
        public static int LogLevel { get => Settings.LogLevel; set => Settings.LogLevel = value; }
        public static bool ShellStartProgramEnabled { get => Settings.ShellStartProgramEnabled; set => Settings.ShellStartProgramEnabled = value; }
        public static string ShellStartProgramPath { get => Settings.ShellStartProgramPath; set => Settings.ShellStartProgramPath = value; }
    }
}
