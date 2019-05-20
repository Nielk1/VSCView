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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 0 || args[0] == @"-1")
            {
                Application.Run(new MainForm());
            }
            else if (args[0] == @"-2")
            {
                Application.Run(new ProcForm());
            }
            else if (args[0] == @"-3")
            {
                Application.Run(new RawForm());
            }
            else
            {
                Application.Run(new MainForm());
            }
        }
    }
}
