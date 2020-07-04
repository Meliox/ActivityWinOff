using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ActivityWinOff
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();
        private const int ATTACH_PARENT_PROCESS = -1;
        private static Mutex mutex = null;

        [STAThread]
        static void Main()
        {
            const string appName = "ActivityWinOff";
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application  
                MessageBox.Show("Application is already running. Exiting");
                return;
            }

            // redirect console output to parent process;
            // must be before any calls to Console.WriteLine()
            AttachConsole(ATTACH_PARENT_PROCESS);

            string[] args = Environment.GetCommandLineArgs();
            foreach (string argument in args.Skip(1))
            {
                switch (argument)
                {
                    case "/autostart":
                        continue;
                    case "/min":
                        continue;
                    case "/activate":
                        continue;
                    case "/?":
                        ShowHelp();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid argument: " + argument);
                        ShowHelp();
                        Environment.Exit(0);
                        break;
                }
            }
            // Detach console
            FreeConsole();

            // Start application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main(args));
        }

        static void ShowHelp()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Format: ActivityWinOff [/args]" + Environment.NewLine
                + "     /?     Show help" + Environment.NewLine
                + "     /activate   Start application in an activated state" + Environment.NewLine
                + "     /autostart   Start startup application at windows startup" + Environment.NewLine
                + "     /min   Start minimized" + Environment.NewLine
                );
            SendKeys.SendWait("{ENTER}");
        }
    }
}
