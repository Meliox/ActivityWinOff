using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActivityWinOff
{
    class BackgroundWorkers
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        static BackgroundWorker workerTimer = null;
        static BackgroundWorker watchWaitForProgram = null;
        static BackgroundWorker watchWaitNetworkTraffic = null;
        static BackgroundWorker watchWaitCPULoad = null;
        static BackgroundWorker watchUserActivity = null;
        static BackgroundWorker watchWaitSpecificTimer = null;
        static BackgroundWorker readCPULoad = null;
        static BackgroundWorker ReadNetworkLoad = null;
        static BackgroundWorker watchFocusProgram = null;

        public static void BackgroundDataReaders()
        {
            Logger.add(2, "Backgroundworker: Starting CPU reader");
            readCPULoad = new BackgroundWorker();
            readCPULoad.DoWork += ReadCPUUtilisation;
            readCPULoad.WorkerReportsProgress = false;
            readCPULoad.WorkerSupportsCancellation = true;
            readCPULoad.RunWorkerAsync();

            Logger.add(2, "Backgroundworker: Starting network utilisation reader");
            ReadNetworkLoad = new BackgroundWorker();
            ReadNetworkLoad.DoWork += ReadNetworkActivity;
            ReadNetworkLoad.WorkerReportsProgress = false;
            ReadNetworkLoad.WorkerSupportsCancellation = true;
            ReadNetworkLoad.RunWorkerAsync();
        }

        public static void SetFocusToProgram(object sender, DoWorkEventArgs e)
        {
            string ProcessName = Path.GetFileNameWithoutExtension(Interface.FocusProgram);
            while (!watchFocusProgram.CancellationPending)
            {
                try
                {
                    var t = Process.GetProcesses();
                    Process process = Process.GetProcessesByName(ProcessName)[0];
                    if (process != null)
                    {
                        // only set focus if not already focussed
                        if (GetForegroundWindow() != process.Handle)
                        {
                            process.WaitForInputIdle();
                            IntPtr hWnd = process.MainWindowHandle;
                            if (hWnd != IntPtr.Zero)
                            {
                                SetForegroundWindow(hWnd);
                                ShowWindow(hWnd, 3);
                                Logger.add(2, "Watchdog: SetFocus " + ProcessName + " has focus set");
                            }
                        }
                        else
                        {
                            Logger.add(2, "Watchdog: SetFocus " + ProcessName + " is already in focus");
                        }
                    }
                    else
                    {
                        Logger.add(2, "Watchdog: SetFocus " + ProcessName + " is not running");
                    }
                }
                catch (Exception)
                {
                    Logger.add(2, "Watchdog: SetFocus " + ProcessName + " is not running");
                }
                Thread.Sleep(Interface.FocusProgramPoolInterval * 1000);
            }
        }

        public static void ReadCPUUtilisation(object sender, DoWorkEventArgs e)
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
            while (true)
            {
                List<(object Name, object Usage)> cpuTimes = searcher.Get().Cast<ManagementObject>().Select(mo => (
                    Name: mo["Name"],
                    Usage: mo["PercentProcessorTime"]
                )
                )
                .ToList();
                IEnumerable<object> query = cpuTimes.Where(x => x.Name.ToString() == "_Total").Select(x => x.Usage);
                object cpuUsage = query.SingleOrDefault();
                Interface.CPUUtil = Convert.ToInt32(cpuUsage);

                Thread.Sleep(1000);
            }
        }

        public static void ReadNetworkActivity(object sender, DoWorkEventArgs e)
        {
            DateTime firstReceivedMesurement;
            long firstBytesRecevied;
            long firstBytesSend;
            long lastBytesRecevied;
            long lastBytesSend;
            DateTime lastReceivedMesurement;
            double ReceivedSpeed = 0;
            double SendSpeed = 0;
            bool ActivateAdapter;

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            while (true)
            {
                ActivateAdapter = false;
                if (Interface.NetworkAdapter != "")
                {
                    foreach (NetworkInterface adapter in adapters)
                    {
                        if (adapter.Description == Interface.NetworkAdapter && adapter.OperationalStatus.ToString() == "Up")
                        {
                            firstReceivedMesurement = DateTime.UtcNow;
                            firstBytesRecevied = adapter.GetIPv4Statistics().BytesReceived;
                            firstBytesSend = adapter.GetIPv4Statistics().BytesSent;
                            Thread.Sleep(1000);
                            lastBytesRecevied = adapter.GetIPv4Statistics().BytesReceived;
                            lastBytesSend = adapter.GetIPv4Statistics().BytesSent;
                            lastReceivedMesurement = DateTime.UtcNow;
                            ReceivedSpeed = (lastBytesRecevied - firstBytesRecevied) / (lastReceivedMesurement - firstReceivedMesurement).TotalSeconds;
                            SendSpeed = (lastBytesSend - firstBytesSend) / (lastReceivedMesurement - firstReceivedMesurement).TotalSeconds;
                            ReceivedSpeed = Math.Round(ReceivedSpeed / 1024, 1);
                            SendSpeed = Math.Round(SendSpeed / 1024, 1);
                            ActivateAdapter = true;
                            break;
                        }
                    }
                }
                if (!ActivateAdapter)
                {
                    Thread.Sleep(1000);
                    ReceivedSpeed = -1;
                    SendSpeed = -1;
                }

                Interface.NetworkDownSpeed = ReceivedSpeed;
                Interface.NetworkUpSpeed = SendSpeed;
            }
        }

        public static void StopWatchTriggers()
        {
            Logger.add(2, "Stopping all watchdogs");
            // Stop all triggers that are running
            if (workerTimer != null)
                workerTimer.CancelAsync();
            if (watchWaitForProgram != null)
                watchWaitForProgram.CancelAsync();
            if (watchUserActivity != null)
                watchUserActivity.CancelAsync();
            if (watchWaitNetworkTraffic != null)
                watchWaitNetworkTraffic.CancelAsync();
            if (watchWaitCPULoad != null)
                watchWaitCPULoad.CancelAsync();
            if (watchWaitSpecificTimer != null)
                watchWaitSpecificTimer.CancelAsync();
            if (watchFocusProgram != null)
                watchFocusProgram.CancelAsync();
        }

        public static void StartWatchTriggers(object sender, DoWorkEventArgs e)
        {
            List<bool> Triggerbool = new List<bool>();

            // Start timer worker
            if (Interface.TimerEnabled)
            {
                workerTimer = new BackgroundWorker();
                workerTimer.DoWork += WatchTimer;
                workerTimer.WorkerReportsProgress = false;
                workerTimer.WorkerSupportsCancellation = true;
                workerTimer.RunWorkerAsync();
            }

            // Start for wait for program worker
            if (Interface.WaitForProgramEnabled)
            {
                watchWaitForProgram = new BackgroundWorker();
                watchWaitForProgram.DoWork += WatchWaitForProgram;
                watchWaitForProgram.WorkerReportsProgress = false;
                watchWaitForProgram.WorkerSupportsCancellation = true;
                watchWaitForProgram.RunWorkerAsync();
            }

            // Start for wait for user activity worker
            if (Interface.UserActivityEnabled)
            {
                watchUserActivity = new BackgroundWorker();
                watchUserActivity.DoWork += WatchUserActivity;
                watchUserActivity.WorkerReportsProgress = false;
                watchUserActivity.WorkerSupportsCancellation = true;
                watchUserActivity.RunWorkerAsync();
            }

            // Start for wait for network traffic worker
            if (Interface.NetworkTrafficEnabled)
            {
                watchWaitNetworkTraffic = new BackgroundWorker();
                watchWaitNetworkTraffic.DoWork += WatchNetworkTraffic;
                watchWaitNetworkTraffic.WorkerReportsProgress = false;
                watchWaitNetworkTraffic.WorkerSupportsCancellation = true;
                watchWaitNetworkTraffic.RunWorkerAsync();
            }

            // Start for wait for CPU load worker
            if (Interface.CPUUtilEnabled)
            {
                watchWaitCPULoad = new BackgroundWorker();
                watchWaitCPULoad.DoWork += WatchCPULoad;
                watchWaitCPULoad.WorkerReportsProgress = false;
                watchWaitCPULoad.WorkerSupportsCancellation = true;
                watchWaitCPULoad.RunWorkerAsync();
            }

            if (Interface.SpecificTimerEnabled)
            {
                watchWaitSpecificTimer = new BackgroundWorker();
                watchWaitSpecificTimer.DoWork += WatchSpecificTime;
                watchWaitSpecificTimer.WorkerReportsProgress = false;
                watchWaitSpecificTimer.WorkerSupportsCancellation = true;
                watchWaitSpecificTimer.RunWorkerAsync();
            }

            if (Interface.FocusProgramEnabled)
            {
                watchFocusProgram = new BackgroundWorker();
                watchFocusProgram.DoWork += SetFocusToProgram;
                watchFocusProgram.WorkerReportsProgress = false;
                watchFocusProgram.WorkerSupportsCancellation = true;
                watchFocusProgram.RunWorkerAsync();
            }

            // Wait for workers to get settled
            Thread.Sleep(5000);
            while (!Main.workerWatchdogTrigger.CancellationPending)
            {
                Triggerbool = new List<bool>();
                if (Interface.TimerEnabled)
                    Triggerbool.Add(Interface.TimerShutdown);
                if (Interface.WaitForProgramEnabled)
                    Triggerbool.Add(Interface.WaitForProgramShutdown);
                if (Interface.UserActivityEnabled)
                    Triggerbool.Add(Interface.UserActivityShutdown);
                if (Interface.NetworkTrafficEnabled)
                    Triggerbool.Add(Interface.NetworkTriggered);
                if (Interface.CPUUtilEnabled)
                    Triggerbool.Add(Interface.CPULoadShutdown);
                if (Interface.SpecificTimerEnabled)
                    Triggerbool.Add(Interface.SpecificShutdown);

                Logger.add(2, "Triger condition=" + Interface.ConditionBehavior + ". Current status of watchdogs " + Triggerbool.FindAll(x => x == true).Count() + "/" + Triggerbool.Count() + " are true");

                if (Interface.ConditionBehavior == "All")
                {
                    if (Triggerbool.All(x => x == true))
                    {
                        StopWatchTriggers();
                        Logger.add(1, "Trigger condition(s) is true. Starting actions");
                        e.Result = true;
                        break;
                    }
                }
                else if (Interface.ConditionBehavior == "One")
                {
                    if (Triggerbool.Contains(true))
                    {
                        StopWatchTriggers();
                        Logger.add(1, "Trigger condition(s) is true. Starting actions");
                        e.Result = true;
                        break;
                    }
                }   
                Thread.Sleep(1000);
                e.Result = false;
            }
            Thread.Sleep(200);
        }

        public static void WatchSpecificTime(object sender, DoWorkEventArgs e)
        {
            Interface.SpecificShutdown = false;

            Interface.TimerNextTrigger = Helper.CurrentTime() + Interface.TimerDays * 24 * 60 * 60 + Interface.TimerHours * 60 * 60 + Interface.TimerMins * 60 + Interface.TimerSeconds;
            while (!watchWaitSpecificTimer.CancellationPending)
            {
                if (Helper.CurrentTime() > Interface.TimerNextTrigger)
                {
                    Interface.SpecificShutdown = true;
                    Logger.add(2, "Watchdog: specific time - true");
                }
                else
                {
                    Interface.SpecificShutdown = false;
                    Logger.add(2, "Watchdog: specific time - false");
                }
                Thread.Sleep(1000);
            }
        }

        public static void WatchTimer(object sender, DoWorkEventArgs e)
        {
            Interface.TimerShutdown = false;

            Interface.TimerNextTrigger = Helper.CurrentTime() + Interface.TimerDays * 24 * 60 * 60 + Interface.TimerHours * 60 * 60 + Interface.TimerMins * 60 + Interface.TimerSeconds;
            while (!workerTimer.CancellationPending)
            {
                if (Helper.CurrentTime() > Interface.TimerNextTrigger)
                {
                    Logger.add(2, "Watchdog: timer - true");
                    Interface.TimerShutdown = true;
                    break;
                }
                else
                {
                    Logger.add(2, "Watchdog: timer - false");
                    Interface.TimerShutdown = false;
                }
                Thread.Sleep(1000);
            }
        }

        public static void WatchWaitForProgram(object sender, DoWorkEventArgs e)
        {
            Interface.WaitForProgramShutdown = false;

            Interface.WaitForProgramNextTrigger = Helper.CalculateNext(DateTime.Now, Interface.CPUUtilTriggerTime, Interface.CPUUtilTriggerTimeFormat);
            while (!workerTimer.CancellationPending)
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.GetMainModuleFileName() != @Interface.WaitForProgram && DateTime.Now > Interface.WaitForProgramNextTrigger)
                    {
                        Interface.WaitForProgramShutdown = true;
                        Logger.add(2, "Watchdog: program - true");
                        break;
                    }
                    else
                    {
                        Interface.WaitForProgramNextTrigger = Helper.CalculateNext(DateTime.Now, Interface.CPUUtilTriggerTime, Interface.CPUUtilTriggerTimeFormat);
                        Logger.add(2, "Watchdog: program - false");
                        Interface.WaitForProgramShutdown = false;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public static void WatchNetworkTraffic(object sender, DoWorkEventArgs e)
        {
            Interface.NetworkTriggered = false;

            // Set next timer
            Interface.NetworkTriggerNext = Helper.CalculateNext(DateTime.Now, Interface.NetworkTriggerTime, Interface.NetworkTriggerTimeFormat);

            // Create array for average values
            int size = Helper.SecondsFromTime(Interface.NetworkTriggerTime, Interface.NetworkTriggerTimeFormat);
            double[] SpeedArray = new double[size];
            for (int j = 0; j < SpeedArray.Length; j++)
                SpeedArray[j] = -1;

            double average;
            int i = 0;


            double Speed = 0;
            while (!watchWaitNetworkTraffic.CancellationPending)
            {
                // get speed
                switch (Interface.NetworkAdapterTriggerType)
                {
                    case "Input":
                        Speed = Interface.NetworkDownSpeed;
                        break;
                    case "Output":
                        Speed = Interface.NetworkUpSpeed;
                        break;
                    case "Combined":
                        Speed = Interface.NetworkUpSpeed + Interface.NetworkDownSpeed;
                        break;
                }

                if (Interface.NetworkAverage)
                {
                    SpeedArray[i % size] = Speed;

                    average = SpeedArray.Where(x => x != -1).Average();
                    //assume average when size > i, as then the average will always be over size measurements, which is the time to wait
                    if (i > size && average < Interface.NetworkTriggerSpeed)
                    {
                        Interface.NetworkTriggered = true;
                        Logger.add(2, "Watchdog: network activity (current = " + Math.Round(Speed, 1) + ", average = " + Math.Round(average, 1) + ") - true");
                    }
                    else
                    {
                        Interface.NetworkTriggered = false;
                        Logger.add(2, "Watchdog: network activity (current = " + Math.Round(Speed, 1) + ", average = " + Math.Round(average, 1) + ") - false");
                    }
                    i++;
                }
                else
                {
                    if (Speed < Interface.NetworkTriggerSpeed && DateTime.Now > Interface.NetworkTriggerNext)
                    {
                        Interface.NetworkTriggered = true;
                        Logger.add(2, "Watchdog: network activity (speed = " + Math.Round(Speed,1) + ") - true");
                    }
                    else
                    {
                        // Reset timer
                        Interface.NetworkTriggered = false;
                        Logger.add(2, "Watchdog: network activity (speed = " + Math.Round(Speed,1) + ") - false");
                        Interface.NetworkTriggerNext = Helper.CalculateNext(DateTime.Now, Interface.NetworkTriggerTime, Interface.NetworkTriggerTimeFormat);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public static void WatchCPULoad(object sender, DoWorkEventArgs e)
        {
            Interface.CPULoadShutdown = false;

            // Set next timer
            Interface.CPUUtilNextTrigger = Helper.CalculateNext(DateTime.Now, Interface.CPUUtilTriggerTime, Interface.CPUUtilTriggerTimeFormat);

            // Create array for average values
            int size = Helper.SecondsFromTime(Interface.CPUUtilTriggerTime, Interface.CPUUtilTriggerTimeFormat);
            int[] CPUUtilArray = new int[size];
            for (int j = 0; j < CPUUtilArray.Length; j++)
                CPUUtilArray[j] = -1;
            double average;
            int i = 0;
            while (!watchWaitCPULoad.CancellationPending)
            {
                if (Interface.CPUUtilAverage)
                {
                    CPUUtilArray[i % size] = Interface.CPUUtil;

                    average = CPUUtilArray.Where(x => x != -1).Average();
                    if (i > size && average < Interface.CPUUtilLoadBelow)
                    {
                        Interface.CPULoadShutdown = true;
                        Logger.add(2, "Watchdog: CPU activity " + Interface.CPUUtil + " - true");
                    }
                    else
                    {
                        Interface.CPULoadShutdown = false;
                        Logger.add(2, "Watchdog: CPU activity " + Interface.CPUUtil + " - false");
                    }
                    i++;
                }
                else
                {
                    if (Interface.CPUUtil < Interface.CPUUtilLoadBelow && DateTime.Now > Interface.CPUUtilNextTrigger)
                    {
                        Interface.CPULoadShutdown = true;
                        Logger.add(2, "Watchdog: CPU activity " + Interface.CPUUtil + " - true");
                    }
                    else
                    {
                        // Reset timer
                        Interface.CPULoadShutdown = false;
                        Logger.add(2, "Watchdog: CPU activity " + Interface.CPUUtil + " - false");
                        Interface.CPUUtilNextTrigger = Helper.CalculateNext(DateTime.Now, Interface.CPUUtilTriggerTime, Interface.CPUUtilTriggerTimeFormat);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public static void WatchUserActivity(object sender, DoWorkEventArgs e)
        {
            Interface.UserActivityShutdown = false;

            while (!watchUserActivity.CancellationPending)
            {
                if (Interface.UserActivityMouse && Interface.UserActivityLastActivityMouse > Interface.UserActivityNextTrigger)
                {
                    Interface.UserActivityNextTrigger = Helper.CalculateNext(Interface.UserActivityLastActivityMouse,Interface.UserActivityTriggerTime, Interface.UserActivityTriggerTimeFormat);
                    Interface.UserActivityShutdown = false;
                    Logger.add(2, "Watchdog: user activity - false");
                }
                if (Interface.UserActivityKeyboard && Interface.UserActivityLastActivityKeyboard > Interface.UserActivityNextTrigger)
                {
                    Interface.UserActivityNextTrigger = Helper.CalculateNext(Interface.UserActivityLastActivityKeyboard, Interface.UserActivityTriggerTime, Interface.UserActivityTriggerTimeFormat);
                    Interface.UserActivityShutdown = false;
                    Logger.add(2, "Watchdog: user activity - false");
                }
                
                if (DateTime.Now > Interface.UserActivityNextTrigger)
                {
                    Interface.UserActivityShutdown = true;
                    Logger.add(2, "Watchdog: user activity - true");
                }
                Thread.Sleep(1000);
            }
        }
    }
}
