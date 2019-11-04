// Useful find targets
// "DEBUG_GYRO"

using System;
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
                    Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters\Whitelist\" + oldPid);

                Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters\Whitelist\" + newPid);
            }
        }

        static void ApplicationStart()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
