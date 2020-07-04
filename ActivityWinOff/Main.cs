using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace ActivityWinOff
{
    public partial class Main : Form
    {
        public static Form MainForm;
        public static BackgroundWorker workerWatchdogTrigger = null;
        BackgroundWorker watchShellExplorer = null;
        BackgroundWorker workerGUIUpdate = null;
        KeyboardInput keyboard;
        MouseInput mouse;
        string[] ApplicationArguments;
        public static Logger logger;
        Thread LoggerThread;

        public Main(string[] args)
        {
            InitializeComponent();

            // set system paths
            Interface.PathLog = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath) + "\\ActivityWinOff.log";
            Interface.PathConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;

            //start logging
            logger = new Logger(Interface.PathLog);

            //Loading information
            InitGuiElements();

            Logger.add(1, "---------------------------------------------------------------------------------------------------------------");
            Logger.add(1, "ActivityWinOff " + Application.ProductVersion.ToString());
            Logger.add(1, "Settings: Loaded " + Interface.PathConfig);
            Logger.add(1, "LogLevel: " + LogLevelcomboBox.SelectedItem.ToString());

            //fix resizing
            this.ResizeBegin += (s, e) => { this.SuspendLayout(); };
            this.ResizeEnd += (s, e) => { this.ResumeLayout(true); };

            MainForm = this;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Start", (s, e) => { StartTrigger(); }),
                new MenuItem("Exit", (s, e) =>  { Application.Exit(); }),
            });

            ApplicationArguments = args;

            HandleLogger(Interface.EnableLogger);
        }

        public void HandleLogger(bool t)
        {
            if (t)
            {
                LoggerThread = new Thread(() => SaveWork());
                LoggerThread.IsBackground = true;
                if (!LoggerThread.ThreadState.Equals(System.Threading.ThreadState.Running))
                    LoggerThread.Start();
            }
            else
            {
                if (LoggerThread != null)
                     LoggerThread.Abort();
            }
        }

        public static void SaveWork()
        {
            while (true)
            {
                logger.saveNow();
                Thread.Sleep(50);
            }
        }

        public void InitGuiElements()
        {
            // Upgrade config
            Interface.UpdateRequired();

            // Load settings
            Interface.LoadSettings();

            // Set information to GUI
            ShutdownSequencedataGridView.Rows.Clear();
            ShutdownSequencedataGridView.Refresh();
            DataGridViewHelpers.JSONToDataGridview(ShutdownSequencedataGridView, (string)Properties.Settings.Default["ShutdownSequence"]);
            StartupSequencedataGridView.Rows.Clear();
            StartupSequencedataGridView.Refresh();
            DataGridViewHelpers.JSONToDataGridview(StartupSequencedataGridView, (string)Properties.Settings.Default["StartupSequence"]);

            LogPathtextBox.Text = Interface.PathLog;
            ConfigLocationtextBox.Text = Interface.PathConfig;

            NetworkAdapterscomboBox.SelectedIndexChanged -= new EventHandler(NetworkAdapterscomboBox_SelectedIndexChanged);
            NetworkAdapterscomboBox.DataSource = GetNetworkInterfaces();
            NetworkAdapterscomboBox.SelectedIndexChanged += new EventHandler(NetworkAdapterscomboBox_SelectedIndexChanged);
            NetworkAdapterscomboBox.SelectedItem = Interface.NetworkAdapter;
            ActionProgramcheckBox.Checked = Interface.ActionProgramEnabled;
            SpeedtextBox.Text = Interface.NetworkTriggerSpeed.ToString();
            NetworkTriggerTimetextBox.Text = Interface.NetworkTriggerTime.ToString();
            NetworkTriggerTimeFormatcomboBox.SelectedItem = Interface.NetworkTriggerTimeFormat;
            TrafficTypecomboBox.SelectedItem = Interface.NetworkAdapterTriggerType;
            TrafficcheckBox.Checked = Interface.NetworkTrafficEnabled;
            NetworkAveragecheckBox.Checked = Interface.NetworkAverage;
            UserActivityMousecheckBox.Checked = Interface.UserActivityMouse;
            UserActivityKeyboardcheckBox.Checked = Interface.UserActivityKeyboard;
            UserActivityEnabledcheckBox.Checked = Interface.UserActivityEnabled;
            UserActivityTriggerTimeFormatcomboBox.SelectedItem = Interface.UserActivityTriggerTimeFormat;
            UserActivityTriggerTimetextBox.Text = Interface.UserActivityTriggerTime.ToString();
            LoadOnWindowsStartcheckBox.Checked = Interface.RunAtWindowsStart;
            StartActivatedcheckBox.Checked = Interface.StartActivated;

            TimercheckBox.Checked = Interface.TimerEnabled;
            TimerDayscomboBox.SelectedItem = Interface.TimerDays.ToString();
            TimerHourscomboBox.SelectedItem = Interface.TimerHours.ToString();
            TimerMinscomboBox.SelectedItem = Interface.TimerMins.ToString();
            TimerSecondscomboBox.SelectedItem = Interface.TimerSeconds.ToString();

            CPUUtilcheckBox.Checked = Interface.CPUUtilEnabled;
            CPUUtilAveragecheckBox.Checked = Interface.CPUUtilAverage;
            CPUUtilcomboBox.SelectedItem = Interface.CPUUtilLoadBelow.ToString();
            CPUUtilTriggerTimeFormatcomboBox.SelectedItem = Interface.CPUUtilTriggerTimeFormat;
            CPUUtilTriggerTimetextBox.Text = Interface.CPUUtilTriggerTime.ToString();

            WaitForProgramcheckBox.Checked = Interface.WaitForProgramEnabled;
            WaitForProgramtextBox.Text = Interface.WaitForProgram;
            WaitForProgramTriggerTimetextBox.Text = Interface.WaitForProgramTriggerTime.ToString();
            WaitForProgramTimeFormatcomboBox.SelectedItem = Interface.WaitForProgramTriggerTimeFormat;

            WarningTimecomboBox.SelectedItem = Interface.WarningTime.ToString();
            ShowWarningcheckBox.Checked = Interface.WarningShowEnabled;

            FocusProgramtextBox.Text = Interface.FocusProgram;
            FocusProgramcheckBox.Checked = Interface.FocusProgramEnabled;
            FocusProgramPoolIntervaltextBox.Text = Interface.FocusProgramPoolInterval.ToString();
            EnableLoggercheckBox.Checked = Interface.EnableLogger;

            ForceShutdowncheckBox.Checked = Interface.ShutdownForced;
            StartupProgramscheckBox.Checked = Interface.StartupProgramsEnabled;

            ShellStartProgramEnabledcheckBox.Checked = Interface.ShellStartProgramEnabled;
            ShellStartProgramtextBox.Text = Interface.ShellStartProgramPath;

            SetBootMethod();

            // set shutdown type
            switch (Interface.ShutdownType)
            {
                case "PowerOff":
                    ShutdownTypePoweroffradioButton.Checked = true;
                    break;
                case "Restart":
                    ShutdownTypeRestartradioButton.Checked = true;
                    break;
                case "Sleep":
                    ShutdownTypeSleepradioButton.Checked = true;
                    break;
                case "Hibernate":
                    ShutdownTypeHibernateradioButton.Checked = true;
                    break;
            }

            // set condition
            switch (Interface.ConditionBehavior)
            {
                case "All":
                    ConditionAllradioButton.Checked = true;
                    break;
                case "One":
                    ConditionOneradioButton.Checked = true;
                    break;
            }

            // set loglevel
            switch (Interface.LogLevel)
            {
                case 1:
                    LogLevelcomboBox.SelectedItem = "Normal";
                    break;
                case 2:
                    LogLevelcomboBox.SelectedItem = "Debug";
                    break;
            }

            // set Shell
            Interface.CurrentShell = Helper.GetCurrentShell();
            if (Interface.CurrentShell.Contains("explorer.exe"))
                    ShellExplorerradioButton.Checked = true;
            else if (Interface.CurrentShell.Contains("ActivityWinOff"))
                ShellActivityWinOffradioButton.Checked = true;
        }

        public void InitWorkers()
        {
            keyboard = new KeyboardInput();
            keyboard.KeyBoardKeyPressed += keyboard_KeyBoardKeyPressed;

            mouse = new MouseInput();
            mouse.MouseMoved += mouse_MouseMoved;

            workerWatchdogTrigger = new BackgroundWorker();
            workerWatchdogTrigger.DoWork += BackgroundWorkers.StartWatchTriggers;
            workerWatchdogTrigger.WorkerReportsProgress = false;
            workerWatchdogTrigger.WorkerSupportsCancellation = true;
            workerWatchdogTrigger.RunWorkerCompleted += OnWorkerCompleted;

            workerGUIUpdate = new BackgroundWorker();
            workerGUIUpdate.DoWork += BackgroundWorkerGUIUpdate;
            workerGUIUpdate.WorkerReportsProgress = false;
            workerGUIUpdate.WorkerSupportsCancellation = true;
            workerGUIUpdate.RunWorkerAsync();
        }

        public void RestartShellExplorer(object sender, DoWorkEventArgs e)
        {
            string ProcessName = Path.GetFileNameWithoutExtension(Interface.ShellStartProgramPath);
            while (!watchShellExplorer.CancellationPending)
            {
                try
                {
                    var t = Process.GetProcesses();
                    Process process = Process.GetProcessesByName(ProcessName)[0];
                    if (process == null)
                        Logger.add(1, "ShellExplorer: Program is stopped running. Starting Windows explorer");
                    else
                        Logger.add(2, "ShellExplorer: Program is still running");
                }
                catch (Exception)
                {
                    Logger.add(1, "ShellExplorer: Program is stopped running. Starting Windows explorer");
                }
                Thread.Sleep(1000);
            }

            //start explorer
            // reset shell 
            Helper.SetCurrentShell("\"explorer.exe\"");

            Process.Start("explorer.exe");

            //reset shell to what is selected in program
            if (Interface.StartupProgramsEnabled)
                Helper.SetCurrentShell("\"" + Process.GetCurrentProcess().MainModule.FileName + "\"" + " /autostart");
            else
                Helper.SetCurrentShell("\"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
        }


        private void OnWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (runWorkerCompletedEventArgs.Result != null && (bool)runWorkerCompletedEventArgs.Result)
                PerformShutdownAction();
        }

        private void BackgroundWorkerGUIUpdate(object sender, DoWorkEventArgs e)
        {
            while(!workerGUIUpdate.CancellationPending)
            {
                if (WindowState != FormWindowState.Minimized)
                {
                    if (Interface.TimerEnabled)
                    {
                        UpdateTimerNextAction(Interface.TimerNextTrigger.ToString());
                    }
                    if (Interface.CPUUtilEnabled)
                    {

                    }
                    if (Interface.UserActivityEnabled)
                    {

                    }
                    if (Interface.NetworkTrafficEnabled)
                    {
                        
                    }
                    if (Interface.WaitForProgramEnabled)
                    {
                        UpdateWaitForProgramNextAction(Helper.CurrentTime().ToString());
                    }
                    UpdateInputTraffic("In: " + (Interface.NetworkDownSpeed).ToString() + " kbit/s");
                    UpdateOutputTraffic("Out: " + (Interface.NetworkUpSpeed).ToString() + " kbit/s");
                    UpdateCPUUtilisation(Interface.CPUUtil.ToString() + " %");

                    // Update mouse & keyboard info
                    UpdateKeyboardtime("Keyboard: " + Helper.FormatDateTime(Interface.UserActivityLastActivityKeyboard));
                    UpdateMousetime("Mouse: " + Helper.FormatDateTime(Interface.UserActivityLastActivityMouse));
                }
                Thread.Sleep(500);
            }
        }

        public void StartTrigger()
        {
            Logger.add(2, "Starting watchdogs");
            Helper.DisableScreensaver(Interface.DisableScreensaver);
            SetStatusInGui();
            LockSettingsInGui(true);
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Stop", (s, e) => { StopTrigger(); }),
                new MenuItem("Exit", (s, e) => { Application.Exit(); }),
            });
            workerWatchdogTrigger.RunWorkerAsync();
            notifyIcon.BalloonTipTitle = "ActivityWinOff";
            notifyIcon.BalloonTipText = "Watchdog running";
            notifyIcon.ShowBalloonTip(1000);
        }

        public void StopTrigger()
        {
            Logger.add(2, "Stopping watchdogs");
            Helper.DisableScreensaver(false);
            SetStatusInGui();
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Start", (s, e) => { StartTrigger(); }),
                new MenuItem("Exit", (s, e) => { Application.Exit(); }),
            });
            BackgroundWorkers.StopWatchTriggers();
            workerWatchdogTrigger.CancelAsync();

            // Hide gui elements
            UpdateNextActionAtlabel("");
            UpdateWaitForProgramNextAction("");
            UpdateTimerNextAction("");

            // renable settings
            LockSettingsInGui(false);
        }

        public void UpdateNextActionAtlabel(string t)
        {
            MainForm.BeginInvoke((MethodInvoker)delegate ()
            {
                FinalActionlabel.Text = t;
            }
            );
        }

        public void SetStatusInGui()
        {
            if (Interface.Enabled)
            {
                Statusbutton.Text = "Active";
                Statusbutton.BackColor = Color.Green;
            }
            else
            {
                Statusbutton.Text = "Inactive";
                Statusbutton.BackColor = Color.White;
            }
        }

        public void UpdateInputTraffic(string t)
        {
            MainForm.BeginInvoke((MethodInvoker)delegate ()
            {
                InputTrafficlabel.Text = t;
            }
            );
        }

        public void UpdateCPUUtilisation(string t)
        {
            MainForm.BeginInvoke((MethodInvoker)delegate ()
            {
                CPUUtilisationlabel.Text = t;
            }
            );
        }

        public void UpdateMousetime(string t)
        {
            MainForm.BeginInvoke((MethodInvoker)delegate ()
            {
                Mouselabel.Text = t;
            }
            );
        }

        public void UpdateKeyboardtime(string t)
        {
            MainForm.BeginInvoke((MethodInvoker)delegate ()
            {
                Keyboardlabel.Text = t;
            }
            );
        }

        public void UpdateOutputTraffic(string t)
        {
            MainForm.BeginInvoke((MethodInvoker)delegate ()
            {
                OutputTrafficlabel.Text = t;
            }
            );
        }

        public void mouse_MouseMoved(object sender, EventArgs e)
        {
            Interface.UserActivityLastActivityMouse = DateTime.Now;
        }

        public void keyboard_KeyBoardKeyPressed(object sender, EventArgs e)
        {
            Interface.UserActivityLastActivityKeyboard = DateTime.Now;
        }

        public List<string> GetNetworkInterfaces()
        {
            List<string> Interfaces = new List<string>();
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
                Interfaces.Add(adapter.Description);
            return Interfaces;
        }

        private void NetworkAdapterscomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.NetworkAdapter = senderComboBox.SelectedItem.ToString();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                if (workerGUIUpdate != null && !workerGUIUpdate.IsBusy)
                    workerGUIUpdate.RunWorkerAsync();
            }
            else
            {
                workerGUIUpdate.CancelAsync();
            }
        }

        private void TrafficTypecomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.NetworkAdapterTriggerType = senderComboBox.SelectedItem.ToString();
        }

        private void SpeedtextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox senderComboBox = (TextBox)sender;
            if (senderComboBox.Text != "")
            {
                Interface.NetworkTriggerSpeed = Convert.ToInt32(senderComboBox.Text);
            }
            else
            {
                Interface.NetworkTriggerSpeed = 0;
            }
        }

        private void Savebutton_Click(object sender, EventArgs e)
        {
            Logger.add(2, "Settings: Saved");
            Properties.Settings.Default["ShutdownSequence"] = DataGridViewHelpers.DataGridviewToJSON(ShutdownSequencedataGridView);
            Properties.Settings.Default["StartupSequence"] = DataGridViewHelpers.DataGridviewToJSON(StartupSequencedataGridView);
            Interface.SaveSettings();
        }

        private void NetworkTriggerTimetextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox senderTextBox = (TextBox)sender;
            if (senderTextBox.Text != "")
            {
                Interface.NetworkTriggerTime = Convert.ToInt32(senderTextBox.Text);
            }
            else
            {
                Interface.NetworkTriggerTime = 0;
            }
        }

        private void TrafficTimeFormatcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.NetworkTriggerTimeFormat = senderComboBox.SelectedItem.ToString();
        }

        private void TrafficcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.NetworkTrafficEnabled = senderCheckBox.Checked;
        }

        private void MinimizeToTraybutton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Hide();
            notifyIcon.Text = "ActivityWinOff" + System.Environment.NewLine + "Running: " + Interface.Enabled;
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Text = "ActivityWinOff";
        }

        private void Condition_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton button = ConditiongroupBox.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked);
            switch (button.Name)
            {
                case "ConditionOneradioButton":
                    Interface.ConditionBehavior = "One";
                    break;
                case "ConditionAllradioButton":
                    Interface.ConditionBehavior = "All";
                    break;
            }
        }

        private void ShutdownType_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton button = ShutdownTypegroupBox.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked);
            switch (button.Name)
            {
                case "ShutdownTypePoweroffradioButton":
                    Interface.ShutdownType = "PowerOff";
                    break;
                case "ShutdownTypeRestartradioButton":
                    Interface.ShutdownType = "Restart";
                    break;
                case "ShutdownTypeSleepradioButton":
                    Interface.ShutdownType = "Sleep";
                    break;
                case "ShutdownTypeHibernateradioButton":
                    Interface.ShutdownType = "Hibernate";
                    break;
            }
        }

        private void ActivateDeactivatebutton_Click(object sender, EventArgs e)
        {
            Interface.Enabled = !Interface.Enabled;
            if (Interface.Enabled)
                StartTrigger();
            else
                StopTrigger();
        }

        private void NetworkAveragecheckBox_CheckChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.NetworkAverage = senderCheckBox.Checked;
        }

        private void LoadOnWindowsStartcheckBox_CheckChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.RunAtWindowsStart = senderCheckBox.Checked;

            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (Interface.RunAtWindowsStart)
            {
                // Add the value in the registry so that the application runs at startup
                rkApp.SetValue("ActivityWinOff", "\"" + Application.ExecutablePath + "\"" + " /autostart");
            }
            else
            {
                // Remove the value from the registry so that the application doesn't start
                rkApp.DeleteValue("ActivityWinOff", false);
            }
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Version: " + Application.ProductVersion.ToString() + Environment.NewLine + Environment.NewLine + "MIT Licence 2020, Meliox", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UserActivityKeyboardcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.UserActivityKeyboard = senderCheckBox.Checked;
        }

        private void UserActivityMousecheckBox_CheckChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.UserActivityMouse = senderCheckBox.Checked;
        }

        private void UserActivityEnabledcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.UserActivityEnabled = senderCheckBox.Checked;
        }

        private void UserActivityTriggerTimetextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox senderTextBox = (TextBox)sender;
            if (senderTextBox.Text != "")
                Interface.UserActivityTriggerTime = Convert.ToInt32(senderTextBox.Text);
            else
                Interface.UserActivityTriggerTime = 0;
        }

        private void UserActivityTriggerTimeFormatcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.UserActivityTriggerTimeFormat = senderComboBox.SelectedItem.ToString();
        }

        private void StartActivatedcheckBox_CheckChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.StartActivated = senderCheckBox.Checked;
        }

        public void UpdateTimerNextAction(string t)
        {
            MainForm.BeginInvoke((MethodInvoker)delegate ()
            {
                if (t != "")
                {
                    TimerNextActionlabel.Visible = true;
                    TimerNextActionlabel.Text = "Next action: " + Environment.NewLine + "  " + Helper.FormatDate(t);
                }
                else
                {
                    TimerNextActionlabel.Visible = false;
                }
            });
        }

        public void UpdateWaitForProgramNextAction(string t)
        {
            MainForm.BeginInvoke((MethodInvoker)delegate ()
            {
                if (t != "")
                {
                    WaitForProgramActionlabel.Visible = true;
                    WaitForProgramActionlabel.Text = "Next action: " + Environment.NewLine + "  Running " + Helper.FormatDate(t);
                }
                else
                {
                    WaitForProgramActionlabel.Visible = false;
                }
            });
        }

        public void LockSettingsInGui(bool lockGUI)
        {
            if (lockGUI)
            {
                IEnumerable<Control> controls = flowLayoutPanel1.Controls.Cast<Control>();
                foreach (Control childControl in controls)
                {
                    if (childControl.GetType() == typeof(GroupBox))
                        childControl.Enabled = false;
                }
                ReloadSettingsbutton.Enabled = false;
                reloadSettingsToolStripMenuItem.Enabled = false;
                FocusgroupBox.Enabled = false;
            }
            else
            {
                IEnumerable<Control> controls = flowLayoutPanel1.Controls.Cast<Control>();
                foreach (Control childControl in controls)
                {
                    if (childControl.GetType() == typeof(GroupBox))
                        childControl.Enabled = true;
                }
                ReloadSettingsbutton.Enabled = true;
                reloadSettingsToolStripMenuItem.Enabled = true;
                FocusgroupBox.Enabled = true;
            }
        }

        public void PerformShutdownAction()
        {
            Interface.WarningCancel = false;
            if (Interface.WarningShowEnabled)
            {
                Form f1 = new ActionWarning();
                f1.ShowDialog();
            }
            if (!Interface.WarningCancel)
            {
                // Make a windows notification popup
                notifyIcon.BalloonTipTitle = "ActivityWinOff";
                if (Interface.ActionProgramEnabled)
                    notifyIcon.BalloonTipText = "Executing shutdown action(s) follow by " + Interface.ShutdownType;
                else
                    notifyIcon.BalloonTipText = "Executing " + Interface.ShutdownType;
                notifyIcon.ShowBalloonTip(1000);

                // Start external program if selected
                if (Interface.ActionProgramEnabled)
                {
                    Logger.add(1, "Shutdown: Starting action(s)");
                    DataGridViewHelpers.DataGridViewExecutor(ShutdownSequencedataGridView);
                }

                string Action = Interface.ShutdownType;
                Logger.add(1, "Shutdown type: " + Interface.ShutdownType + ", Forced=" + Interface.ShutdownForced.ToString());
                string shutdownCmd = "";
                switch (Action)
                {
                    case "PowerOff":
                        shutdownCmd += "/s /t 0 /c \"ActivityWinOff\"";
                        break;
                    case "Restart":
                        shutdownCmd += "/r /t 0 /c \"ActivityWinOff\"";
                        break;
                    case "Sleep":
                        shutdownCmd += "/d /hybrid 0 /c \"ActivityWinOff\"";
                        break;
                    case "Hibernate":
                        shutdownCmd += "/h /t 0 /c \"ActivityWinOff\"";
                        break;
                }
                if (Interface.ShutdownForced)
                    shutdownCmd += "/f";
                Process.Start("psshutdown", shutdownCmd);
            }
        }

        private void TimercheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.TimerEnabled = senderCheckBox.Checked;
        }

        private void TimerDayscomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.TimerDays = Convert.ToInt16(senderComboBox.SelectedItem.ToString());
        }

        private void TimerhourscomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.TimerHours = Convert.ToInt16(senderComboBox.SelectedItem.ToString());
        }

        private void TimerMinscomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.TimerMins = Convert.ToInt16(senderComboBox.SelectedItem.ToString());
        }

        private void TimerSecondscomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.TimerSeconds = Convert.ToInt16(senderComboBox.SelectedItem.ToString());
        }

        private void CPUUtilcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.CPUUtilEnabled = senderCheckBox.Checked;
        }

        private void CPUUtilcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.CPUUtilLoadBelow = Convert.ToInt16(senderComboBox.SelectedItem.ToString());
        }

        private void CPUUtilAveragecheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.CPUUtilAverage = senderCheckBox.Checked;
        }

        private void CPUUtilTriggerTimetextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox senderTextBox = (TextBox)sender;
            if (senderTextBox.Text != "")
                Interface.CPUUtilTriggerTime = Convert.ToInt32(senderTextBox.Text);
            else
                Interface.CPUUtilTriggerTime = 0;
        }

        private void CPUUtilTriggerTimeFormatcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.CPUUtilTriggerTimeFormat = senderComboBox.SelectedItem.ToString();
        }

        private void WaitForProgramcheckBox_CheckChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.WaitForProgramEnabled = senderCheckBox.Checked;
        }

        private void WaitForProgramCleanbutton_Click(object sender, EventArgs e)
        {
            WaitForProgramtextBox.Text = "";
        }

        private void WaitForProgrambutton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                WaitForProgramtextBox.Text = openFileDialog1.FileName;
                Interface.WaitForProgram = openFileDialog1.FileName;
            }
        }

        private void WaitForProgramTimeFormatcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.WaitForProgramTriggerTimeFormat = senderComboBox.SelectedItem.ToString();
        }

        private void WaitForProgramTriggerTimetextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox senderTextBox = (TextBox)sender;
            Interface.WaitForProgramTriggerTime = Convert.ToInt16(senderTextBox.Text);
        }

        private void OnlyAllowNumericNumbers(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void ReloadSettingsbutton_Click(object sender, EventArgs e)
        {
            InitGuiElements();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void saveSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Savebutton_Click(null, null);
        }

        private void reloadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadSettingsbutton_Click(null, null);
        }

        private void minimizeToTrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MinimizeToTraybutton_Click(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Logger.add(1, "Testing shutdown action(s) - start");
            PerformShutdownAction();
            Logger.add(1, "Testing shutdown action(s) - end");
        }

        private void WarningTimecomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            Interface.WarningTime = Convert.ToInt32(senderComboBox.SelectedItem);
        }

        private void ShowWarningcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.WarningShowEnabled = senderCheckBox.Checked;
        }

        private void ActionProgramcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.ActionProgramEnabled = senderCheckBox.Checked;
        }

        private void OpenConfigbutton_Click(object sender, EventArgs e)
        {
            bool h = File.Exists(@Interface.PathConfig);
            if (File.Exists(@Interface.PathConfig))
                Process.Start(@Interface.PathConfig);
        }

        private void OpenLogbutton_Click(object sender, EventArgs e)
        {
            if (File.Exists(@Interface.PathLog))
                Process.Start(@Interface.PathLog);
        }

        private void SpecificTimer_ValueChanged(object sender, EventArgs e)
        {
            DateTime t = SpecificTimerDate.Value.Date + SpecificTimerTime.Value.TimeOfDay;
            Interface.SpecificTimerTime = t;
        }

        private void SpecificTimercheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.SpecificTimerEnabled = senderCheckBox.Checked;
        }

        private void ShellgroupBox_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton button = ShellgroupBox.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked);
            switch (button.Name)
            {
                case "ShellActivityWinOffradioButton":
                    if (Interface.StartupProgramsEnabled)
                        Helper.SetCurrentShell("\"" + Process.GetCurrentProcess().MainModule.FileName + "\"" + " /autostart");
                    else
                        Helper.SetCurrentShell("\"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
                    break;
                case "ShellExplorerradioButton":
                    Helper.SetCurrentShell("\"explorer.exe\"");
                    break;
            }
            SetBootMethod();
        }

        private void ShutdownAddbutton_Click(object sender, EventArgs e)
        {
            ShutdownSequencedataGridView.Rows.Add("", "", "", "Normal", 0, 0, false);
            DataGridViewHelpers.DataGridViewAddOrder(ShutdownSequencedataGridView);
        }

        private void ShutdownSequencedataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewHelpers.InputHandler(sender, e);
        }

        private void ShutdownRemovebutton_Click(object sender, EventArgs e)
        {
            if (ShutdownSequencedataGridView.CurrentRow != null)
            {
                ShutdownSequencedataGridView.Rows.RemoveAt(ShutdownSequencedataGridView.CurrentRow.Index);
                DataGridViewHelpers.DataGridViewAddOrder(ShutdownSequencedataGridView);
            }
        }

        private void ShutdownSequencedataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            ShutdownRemovebutton_Click(null, null);
        }

        private void SequencedataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridViewHelpers.DataGridViewHandleKeyPress(sender, e);
        }

        private void DisableScreensaver_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.DisableScreensaver = senderCheckBox.Checked;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            InitWorkers();
            BackgroundWorkers.BackgroundDataReaders();

            foreach (string argument in ApplicationArguments.Skip(1))
            {
                switch (argument)
                {
                    case "/autostart":
                        if (Interface.StartupProgramsEnabled)
                        {
                            MinimizeToTraybutton_Click(null, null);
                            Logger.add(2, "Console args: /autostart");
                            DataGridViewHelpers.DataGridViewExecutor(StartupSequencedataGridView);
                            if (Interface.CurrentShell != "explorer.exe" && Interface.ShellStartProgramEnabled)
                            {
                                watchShellExplorer = new BackgroundWorker();
                                watchShellExplorer.DoWork += RestartShellExplorer;
                                watchShellExplorer.WorkerReportsProgress = false;
                                watchShellExplorer.WorkerSupportsCancellation = true;
                                watchShellExplorer.RunWorkerAsync();
                            }
                        }
                        break;
                    case "/min":
                        Logger.add(2, "Console args: /min");
                        MinimizeToTraybutton_Click(null, null);
                        break;
                    case "/active":
                        Logger.add(2, "Console args: /active");
                        StartTrigger();
                        break;
                }
            }
        }

        private void StartAddbutton_Click(object sender, EventArgs e)
        {
            StartupSequencedataGridView.Rows.Add("", "", "", "Normal", 0, 0, false);
            DataGridViewHelpers.DataGridViewAddOrder(StartupSequencedataGridView);
        }

        private void StartupRemovebutton_Click(object sender, EventArgs e)
        {
            if (StartupSequencedataGridView.CurrentRow != null)
            {
                StartupSequencedataGridView.Rows.RemoveAt(StartupSequencedataGridView.CurrentRow.Index);
                DataGridViewHelpers.DataGridViewAddOrder(StartupSequencedataGridView);
            }
        }

        private void StartupSequencedataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewHelpers.InputHandler(sender, e);
        }

        private void SelectFocusProgrambutton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "exe (*.exe)|*.exe";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FocusProgramtextBox.Text = dialog.FileName;
                Interface.FocusProgram = dialog.FileName;
            }
        }

        private void FocusProgramClearbutton_Click(object sender, EventArgs e)
        {
            FocusProgramtextBox.Text = "";
            Interface.FocusProgram = "";
        }

        private void FocusProgramcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.FocusProgramEnabled = senderCheckBox.Checked;
        }

        private void FocusProgramPoolIntervaltextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox senderTextBox = (TextBox)sender;
            if (senderTextBox.Text != "")
                Interface.FocusProgramPoolInterval = Convert.ToInt32(senderTextBox.Text);
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f1 = new Help();
            f1.Show();
        }

        private void EnableLoggercheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.EnableLogger = senderCheckBox.Checked;
            HandleLogger(Interface.EnableLogger);
        }

        private void ForceShutdowncheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.ShutdownForced = senderCheckBox.Checked;
        }

        private void SetBootMethod()
        {
            // add to startup
            if (Interface.StartupProgramsEnabled && !Helper.GetCurrentShell().Contains(Process.GetCurrentProcess().MainModule.FileName))
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key.SetValue("ActivityWinOff", "\"" + Application.ExecutablePath + "\"" + " /autostart");
            }
            else
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key.DeleteValue("ActivityWinOff", false);
            }
        }

        private void StartupProgramscheckBox_CheckChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.StartupProgramsEnabled = senderCheckBox.Checked;
            SetBootMethod();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.add(1, "Closing ActivityWinOff");
            //close async threads
            if (workerWatchdogTrigger.IsBusy)
                workerWatchdogTrigger.CancelAsync();
            if (workerGUIUpdate.IsBusy)
                workerGUIUpdate.CancelAsync();
            if (watchShellExplorer != null && watchShellExplorer.IsBusy)
                watchShellExplorer.CancelAsync();
            Thread.Sleep(100);
        }

        private void LogLevelcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            switch(senderComboBox.SelectedItem.ToString())
            {
                case "Normal":
                    Interface.LogLevel = 1;
                    break;
                case "Debug":
                    Interface.LogLevel = 2;
                    break;
            }
            Logger.add(1, "LogLevel: " + senderComboBox.SelectedItem.ToString());
        }

        private void StartupUpbutton_Click(object sender, EventArgs e)
        {
            DataGridViewHelpers.DataGridViewMoveUp(StartupSequencedataGridView, StartupSequencedataGridView.CurrentRow);
        }

        private void StartupDownbutton_Click(object sender, EventArgs e)
        {
            DataGridViewHelpers.DataGridViewMoveDown(StartupSequencedataGridView, StartupSequencedataGridView.CurrentRow);
        }

        private void ShutdownUpbutton_Click(object sender, EventArgs e)
        {
            DataGridViewHelpers.DataGridViewMoveUp(ShutdownSequencedataGridView, ShutdownSequencedataGridView.CurrentRow);
        }

        private void ShutdownDownbutton_Click(object sender, EventArgs e)
        {
            DataGridViewHelpers.DataGridViewMoveDown(ShutdownSequencedataGridView, ShutdownSequencedataGridView.CurrentRow);
        }

        private void ShellStartProgramcheckBoxEnabled_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderCheckBox = (CheckBox)sender;
            Interface.ShellStartProgramEnabled = senderCheckBox.Checked;
        }

        private void ShellStartProgramSelectbutton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "exe (*.exe)|*.exe";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ShellStartProgramtextBox.Text = dialog.FileName;
                Interface.ShellStartProgramPath = dialog.FileName;
            }
        }

        private void ShellStartProgramClearbutton_Click(object sender, EventArgs e)
        {
            ShellStartProgramtextBox.Text = "";
        }
    }

    public class Logger
    {
        // Queue import: 
        // using System.Collections
        public static Queue logs = new Queue();
        public static string path = "";

        public Logger(string p)
        {
            path = p;
        }

        public static void add(int DebugLevel, string LogString)
        {
            if (Interface.EnableLogger)
            {
                if (DebugLevel <= Interface.LogLevel)
                    logs.Enqueue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "|" + LogString);
                else if (DebugLevel <= Interface.LogLevel)
                    logs.Enqueue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "|" + LogString);
            }
        }

        public void saveNow()
        {
            logs.Enqueue("");
            if (logs.Count > 0)
            {
                string t;
                try
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        while (logs.Count > 0)
                        {
                            //remove null entries
                            if ((string)logs.Peek() == null || (string)logs.Peek() == "")
                            {
                                logs.Dequeue();
                                continue;
                            }
                            // get first entry
                            t = (string)logs.Peek();
                            t = t.Replace("\n", "");
                            // write first entry
                            Console.WriteLine(t);
                            sw.WriteLine(t);
                            // remove first entry
                            logs.Dequeue();
                        }
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }

    public class WindowsHookHelper
    {
        public delegate IntPtr HookDelegate(
            Int32 Code, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern IntPtr CallNextHookEx(
            IntPtr hHook, Int32 nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern IntPtr UnhookWindowsHookEx(IntPtr hHook);


        [DllImport("User32.dll")]
        public static extern IntPtr SetWindowsHookEx(
            Int32 idHook, HookDelegate lpfn, IntPtr hmod,
            Int32 dwThreadId);
    }

    public class KeyboardInput : IDisposable
    {
        public event EventHandler<EventArgs> KeyBoardKeyPressed;

        private WindowsHookHelper.HookDelegate keyBoardDelegate;
        private IntPtr keyBoardHandle;
        private const Int32 WH_KEYBOARD_LL = 13;
        private bool disposed;

        public KeyboardInput()
        {
            keyBoardDelegate = KeyboardHookDelegate;
            keyBoardHandle = WindowsHookHelper.SetWindowsHookEx(
                WH_KEYBOARD_LL, keyBoardDelegate, IntPtr.Zero, 0);
        }

        private IntPtr KeyboardHookDelegate(
            Int32 Code, IntPtr wParam, IntPtr lParam)
        {
            if (Code < 0)
            {
                return WindowsHookHelper.CallNextHookEx(
                    keyBoardHandle, Code, wParam, lParam);
            }

            if (KeyBoardKeyPressed != null)
                KeyBoardKeyPressed(this, new EventArgs());

            return WindowsHookHelper.CallNextHookEx(
                keyBoardHandle, Code, wParam, lParam);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (keyBoardHandle != IntPtr.Zero)
                {
                    WindowsHookHelper.UnhookWindowsHookEx(
                        keyBoardHandle);
                }

                disposed = true;
            }
        }

        ~KeyboardInput()
        {
            Dispose(false);
        }
    }

    public class MouseInput : IDisposable
    {
        public event EventHandler<EventArgs> MouseMoved;

        private WindowsHookHelper.HookDelegate mouseDelegate;
        private IntPtr mouseHandle;
        private const Int32 WH_MOUSE_LL = 14;

        private bool disposed;

        public MouseInput()
        {
            mouseDelegate = MouseHookDelegate;
            mouseHandle = WindowsHookHelper.SetWindowsHookEx(WH_MOUSE_LL, mouseDelegate, IntPtr.Zero, 0);
        }

        private IntPtr MouseHookDelegate(Int32 Code, IntPtr wParam, IntPtr lParam)
        {
            if (Code < 0)
                return WindowsHookHelper.CallNextHookEx(mouseHandle, Code, wParam, lParam);

            if (MouseMoved != null)
                MouseMoved(this, new EventArgs());

            return WindowsHookHelper.CallNextHookEx(mouseHandle, Code, wParam, lParam);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (mouseHandle != IntPtr.Zero)
                    WindowsHookHelper.UnhookWindowsHookEx(mouseHandle);

                disposed = true;
            }
        }

        ~MouseInput()
        {
            Dispose(false);
        }
    }

    public static class Extensions
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            StringBuilder fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;

            try
            {
                return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
            }
            catch
            {
                return null;
            }
        }
    }
}
