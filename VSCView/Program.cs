// Useful find targets
// "DEBUG_GYRO"

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VSCView
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if(args.Length == 0)
                ApplicationStart();
            if (args.Length == 3 && args[0] == "admin")
            {
                int oldPid = int.Parse(args[1]);
                int newPid = int.Parse(args[2]);


                if (oldPid != 0)
                    try
                    {
                        Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters\Whitelist\" + oldPid);
                    }
                    catch { }
                Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters\Whitelist\" + newPid);
            }
        }

        static void ApplicationStart()
        {
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;

            bool FoundOpenSlot = false;
            int programInstance = 0;
            Mutex mutex = null;
            for (; programInstance < 100; programInstance++)
            {
                mutex?.Dispose(); // dispose prior loop's mutex
                mutex = new Mutex(false, $"{appGuid}:{programInstance}");
                bool isAnotherInstanceOpen = !mutex.WaitOne(TimeSpan.Zero);
                if (!isAnotherInstanceOpen)
                {
                    FoundOpenSlot = true;
                    break;
                }
            }

            if (FoundOpenSlot)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(programInstance));
            }
            else
            {
                mutex?.Dispose(); // dispose prior loop's mutex
                MessageBox.Show("Failed to start program, do you really have 100 instances open or did something go wrong?", "100 Instance Limit", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            mutex.ReleaseMutex();
            mutex.Dispose();
        }
    }
}
