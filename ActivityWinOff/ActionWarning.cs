using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ActivityWinOff;

namespace ActivityWinOff
{
    public partial class ActionWarning : Form
    {
        static BackgroundWorker workerProgressbar = null;
        AutoResetEvent _resetEvent = new AutoResetEvent(false);
        Form ActionWarningForm;

        public ActionWarning()
        {
            InitializeComponent();
            ActionWarningForm = this;
            Cancelbutton.Visible = true;
            this.Shown += new EventHandler(this.ActionWarningForm_Shown);
        }

        private void ActionWarningForm_Shown(object sender, EventArgs e)
        {
            StartProgressbar();
        }

        private void StartProgressbar()
        {
            progressBar.Minimum = 0;
            progressBar.Maximum = Interface.WarningTime;
            progressBar.Value = 1;
            progressBar.Step = 1;

            workerProgressbar = new BackgroundWorker();
            workerProgressbar.DoWork += ProgressProgressbar;
            workerProgressbar.WorkerReportsProgress = true;
            workerProgressbar.WorkerSupportsCancellation = true;
            workerProgressbar.RunWorkerAsync();
            workerProgressbar.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerProgressbar_RunWorkerCompleted);
        }

        private void workerProgressbar_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Warninglabel.Text = "Executing " + Interface.ShutdownType;
            Cancelbutton.Visible = false;
            this.Close();
        }

        private void UpdateWarningLabel(int time)
        {
            ActionWarningForm.BeginInvoke((MethodInvoker)delegate ()
            {
                Warninglabel.Visible = true;
                Warninglabel.Text = Interface.ShutdownType + " in " + time.ToString() + " seconds";
            });
        }

        private void UpdateProgressBar()
        {
            ActionWarningForm.BeginInvoke((MethodInvoker)delegate ()
            {
                progressBar.PerformStep();
                double calc = (((double)progressBar.Value / (double)progressBar.Maximum) * 100);
                if (calc <= 33)
                    progressBar.SetState(1);
                else if (calc <= 66)
                    progressBar.SetState(3);
                else
                    progressBar.SetState(2);
            });
        }

        private void ProgressProgressbar(object sender, DoWorkEventArgs e)
        {
            while (progressBar.Value < progressBar.Maximum && !workerProgressbar.CancellationPending)
            {
                UpdateWarningLabel(progressBar.Maximum - progressBar.Value);
                Thread.Sleep(1000);
                UpdateProgressBar();
            }
            _resetEvent.Set();
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            workerProgressbar.CancelAsync();
            _resetEvent.WaitOne();
            Interface.WarningCancel = true;
            this.Close();
        }
    }

    public static class ModifyProgressBarColor
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
        public static void SetState(this System.Windows.Forms.ProgressBar pBar, int state)
        {
            SendMessage(pBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
        }
    }
}
