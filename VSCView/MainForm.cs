using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSCView
{
    public partial class MainForm : Form
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        private static extern uint TimeBeginPeriod(uint delay);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        private static extern uint TimeEndPeriod(uint delay);

        public static SteamController.SteamControllerState state;
        public static SensorCollector sensorData;

        ControllerData ControllerData;
        SteamController ActiveController;
        bool exited = false;
        int renderUsageEx = 0;

        UI ui;

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();

            ControllerData = new ControllerData();
            state = new SteamController.SteamControllerState();
            // 5s lookback smoothing on sensors
            sensorData = new SensorCollector(5, true);

            LoadThemes();
            LoadControllers();
        }

        #region Render Loop
        private void RenderLoop()
        {
            int fpsLimit = 60, fpsTracker = 0;
            double frameCap = 1.0f * 1000 / fpsLimit;
            double prevTime = msTime();
            double lag = 0.0f, timer = 0.0f;
            // loop must run faster than rendering rate!
            int tickDelay = (int)(frameCap * 0.75f);
            TimeBeginPeriod((uint)tickDelay);

            while (!exited)
            {
                double curTime = msTime();
                double elapsed = (curTime - prevTime);
                prevTime = curTime;
                lag += elapsed;
#if DEBUG
                timer += elapsed;
#endif

                if (lag >= frameCap)
                {// variable render rate on fixed timestep
                    Render();
                    lag -= frameCap;
#if DEBUG
                    fpsTracker++;
                    Debug.WriteLine($"Render delta: {lag}ms");
#endif
                }
#if DEBUG
                if (timer >= 1000.0f)
                {
                    Debug.WriteLine($"FPS: {fpsTracker}");
                    fpsTracker = 0;
                    timer = 0;
                }
#endif

                Tick(tickDelay);
            }
            TimeEndPeriod((uint)tickDelay);
        }

        private void Tick(int delay)
        {// high res timer (CAUTION: PInvoke dragons!)
            try
            {
                Thread.Sleep(delay);
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        private double msTime()
        {
            return Stopwatch.GetTimestamp() * 1.0f / 10000.0f;
        }

        private void Render()
        {
            var state = new SteamController.SteamControllerState();
            if (ActiveController != null)
            {
                state = ActiveController.GetState();
                sensorData.Update(state);
            }

            if (!this.IsDisposed && this.InvokeRequired && sensorData != null)
            {
                try
                {
                    this.Invoke(new Action(() =>
                    {// force a full repaint
                        this.Invalidate();
                    }));
                }
                catch (ObjectDisposedException e) { /* eat the Disposed exception when exiting*/ }
            }
        }
        #endregion

        private void LoadThemes()
        {
            tsmiTheme.DropDownItems.Clear();

            if (!Directory.Exists("themes")) Directory.CreateDirectory("themes");
            string[] themeParents = Directory.GetDirectories("themes");

            foreach (string themeParent in themeParents)
            {
                string ThemeName = Path.GetFileName(themeParent);

                string[] themeFiles = Directory.GetFiles(themeParent, "*.json", SearchOption.TopDirectoryOnly);
                foreach (string themeFile in themeFiles)
                {
                    string ThemeFileName = Path.GetFileNameWithoutExtension(themeFile);

                    string DisplayName = ThemeName + "/" + ThemeFileName;

                    ToolStripItem itm = tsmiTheme.DropDownItems.Add(DisplayName, null, LoadTheme);
                }
            }
        }

        private void LoadControllers()
        {
            tsmiController.DropDownItems.Clear();
            SteamController[] Controllers = SteamController.GetControllers();

            for (int i = 0; i < Controllers.Count(); i++)
            {
                ToolStripItem itm = tsmiController.DropDownItems.Add(Controllers[i].GetDevicePath(), null, LoadController);
                itm.Tag = Controllers[i];
                switch (Controllers[i].ConnectionType)
                {
                    case SteamController.EConnectionType.Wireless:
                        itm.Image = Properties.Resources.icon_wireless;
                        break;
                    case SteamController.EConnectionType.USB:
                        itm.Image = Properties.Resources.icon_usb;
                        break;
                    case SteamController.EConnectionType.BT:
                        itm.Image = Properties.Resources.icon_bt;
                        break;
                }

                // load the first controller in the list if it exists
                if (i == 0 && Controllers[i] != null)
                    LoadController(Controllers[i], null);
            }
        }

        private void LoadController(object sender, EventArgs e)
        {
            if (ActiveController != null)
                ActiveController.DeInitalize();

            // differentiate between context selection and startup
            if (sender is ToolStripItem)
            {
                ToolStripItem item = (ToolStripItem)sender;
                ActiveController = (SteamController)item.Tag;
            }
            else
                ActiveController = (SteamController)sender;

            ControllerData.SetController(ActiveController);
            ActiveController.Initalize();
        }

        private async void LoadTheme(object sender, EventArgs e)
        {
            lblHint1.Hide();
            lblHint2.Hide();

            ToolStripItem item = (ToolStripItem)sender;
            string displayText = item.Text;

            string[] displayTextParts = displayText.Split(new string[] { "/" }, 2, StringSplitOptions.None);

            string skinJson = File.ReadAllText(Path.Combine("themes", displayTextParts[0], displayTextParts[1] + @".json"));

            ui = new UI(ControllerData, displayTextParts[0], skinJson);
            this.Width = ui.Width;
            this.Height = ui.Height;

            if (0 == Interlocked.Exchange(ref renderUsageEx, 1))
            {
                await Task.Run(() => RenderLoop());
                Interlocked.Exchange(ref renderUsageEx, 0);
            }
        }

        #region Drag Anywhere
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void MainForm_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion Drag Anywhere

        protected override void OnPaint(PaintEventArgs e)
        {
            //DateTime now = DateTime.UtcNow;

            // Call the OnPaint method of the base class.  
            base.OnPaint(e);

            ui?.Paint(e.Graphics);

            //Console.WriteLine((DateTime.UtcNow - now).TotalMilliseconds);
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            exited = true;
            this.Close();
        }

        private void tsmiReloadThemes_Click(object sender, EventArgs e)
        {
            LoadThemes();
        }

        private void tsmiReloadControllers_Click(object sender, EventArgs e)
        {
            LoadControllers();
        }

        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            new About().ShowDialog();
        }

        private void tsmiSetBackgroundColor_Click(object sender, EventArgs e)
        {
            if(colorDialog1.ShowDialog() == DialogResult.OK)
            {
                this.BackColor = colorDialog1.Color;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            exited = true;
        }
    }
}
