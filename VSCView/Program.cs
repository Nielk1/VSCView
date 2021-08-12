// Useful find targets
// "DEBUG_GYRO"

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace VSCView
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(StandardHandle nStdHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetStdHandle(StandardHandle nStdHandle, IntPtr handle);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern FileType GetFileType(IntPtr handle);

        private enum StandardHandle : uint
        {
            Input = unchecked((uint)-10),
            Output = unchecked((uint)-11),
            Error = unchecked((uint)-12)
        }

        private enum FileType : uint
        {
            Unknown = 0x0000,
            Disk = 0x0001,
            Char = 0x0002,
            Pipe = 0x0003
        }

        private static bool IsRedirected(IntPtr handle)
        {
            FileType fileType = GetFileType(handle);

            return (fileType == FileType.Disk) || (fileType == FileType.Pipe);
        }






        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if(args.Length == 0)
                ApplicationStart();
            if (args.Length == 1 && args[0].TrimStart(new char[] { '-', '/' }) == "console")
            {
                ApplicationStart(true);
            }
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

        static void ApplicationStart(bool showCon = false)
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
                bool AllocedCon = false;
                if (showCon)
                {
                    bool outRedirected = IsRedirected(GetStdHandle(StandardHandle.Output));
                    if (outRedirected)
                    {
                        var initialiseOut = Console.Out;
                    }

                    bool errorRedirected = IsRedirected(GetStdHandle(StandardHandle.Error));
                    if (errorRedirected)
                    {
                        var initialiseError = Console.Error;
                    }

                    if (!AttachConsole(-1))
                    {
                        if (!outRedirected || !errorRedirected)
                        {
                            AllocConsole();
                            AllocedCon = true;
                        }
                    }

                    if (!errorRedirected)
                        SetStdHandle(StandardHandle.Error, GetStdHandle(StandardHandle.Output));
                    
                    Console.WriteLine($"Starting {System.AppDomain.CurrentDomain.FriendlyName}, please wait.");
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(programInstance));

                if (AllocedCon)
                    FreeConsole();
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
