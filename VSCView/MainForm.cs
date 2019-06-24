using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public static ControllerState state;
        public static SensorCollector sensorData;
        public static int frameTime = (int)(1000 / 60); // 16ms interval => ~62fps

        ControllerData ControllerData;
        IController ActiveController;
        List<IController> Controllers = new List<IController>();
        System.Threading.Timer ttimer;

        List<IControllerFactory> Factory;

        UI ui;

        Settings settings;

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();

            Factory = new List<IControllerFactory>() {
                new SteamControllerFactory(),
                new DS4ControllerFactory(),
            };

            ControllerData = new ControllerData();
            state = new ControllerState();
            // 5s lookback for smoothing
            sensorData = new SensorCollector(5, true);

            LoadThemes();
            LoadSettings();
            if(File.Exists("ctrl.last"))
            {
                settings.Theme = File.ReadAllText("ctrl.last");
                File.Delete("ctrl.last");
            }
            if (!string.IsNullOrWhiteSpace(settings.Theme) && File.Exists(settings.Theme))
            {
                LoadTheme(settings.Theme, false); // we'll just let this task spool off on its own and pray
            }
            if (!string.IsNullOrWhiteSpace(settings.Background))
            {
                try
                {
                    this.BackColor = ColorTranslator.FromHtml(settings.Background);
                }
                catch { }
            }
            LoadControllers(true);
        }

        private void Render(object stateInfo)
        {
            var state = new ControllerState();
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
                catch (ObjectDisposedException e) { /* eat the Disposed exception when exiting */ }
            }
        }

        private void LoadSettings()
        {
            if (!File.Exists("settings.json")) File.Create("settings.json").Close();
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json")) ?? new Settings();
        }
        private void SaveSettings()
        {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings));
        }
        private void LoadThemes()
        {
            tsmiTheme.DropDownItems.Clear();

            if (!Directory.Exists("themes")) Directory.CreateDirectory("themes");
            string[] themeParents = Directory.GetDirectories("themes");

            foreach (string themeParent in themeParents)
            {
                string ThemeName = Path.GetFileName(themeParent);
                try
                {
                    string themeMetaFile = Path.Combine(themeParent, "theme.json");
                    if (File.Exists(themeMetaFile))
                    {
                        ThemeDesc desc = JsonConvert.DeserializeObject<ThemeDesc>(File.ReadAllText(themeMetaFile));
                        if (!string.IsNullOrWhiteSpace(desc.name))
                            ThemeName = desc.name;
                    }
                }
                catch { }

                ToolStripMenuItem itmTop = new ToolStripMenuItem(ThemeName);
                tsmiTheme.DropDownItems.Add(itmTop);

                string[] themeFiles = Directory.GetFiles(themeParent, "*.json", SearchOption.TopDirectoryOnly);
                foreach (string themeFile in themeFiles)
                {
                    string ThemeFileName = Path.GetFileNameWithoutExtension(themeFile);

                    if (ThemeFileName.ToLowerInvariant() == "theme")
                        continue;

                    string DisplayName = ThemeFileName;

                    try
                    {
                        ThemeSubDesc desc = JsonConvert.DeserializeObject<ThemeSubDesc>(File.ReadAllText(themeFile));
                        if (!string.IsNullOrWhiteSpace(desc.name))
                            DisplayName = desc.name;
                    }
                    catch { }

                    ToolStripItem itm = new ToolStripMenuItem(DisplayName, null, LoadTheme);
                    itm.Tag = themeFile;
                    itmTop.DropDownItems.Add(itm);
                }
            }
        }

        private void LoadControllers(bool firstload)
        {
            tsmiController.DropDownItems.Clear();
            Controllers.ForEach(dr => dr.DeInitalize());
            Controllers.Clear();
            Controllers.AddRange(Factory.SelectMany(dr => dr.GetControllers()));

            for (int i = 0; i < Controllers.Count(); i++)
            {
                ToolStripItem itm = tsmiController.DropDownItems.Add(Controllers[i].GetName(), null, LoadController);
                IController c = Controllers[i];
                Controllers[i].ControllerNameUpdated += () => {
                    this.Invoke(new Action(() =>
                    {
                        itm.Text = c.GetName();
                    }));
                };
                itm.Text = Controllers[i].GetName();

                itm.ImageScaling = ToolStripItemImageScaling.None;
                itm.Tag = Controllers[i];
                itm.Image = Controllers[i].GetIcon();

                // load the first controller in the list if it exists
                if (firstload && i == 0 && Controllers[i] != null)
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
                ActiveController = (IController)item.Tag;
            }
            else
                ActiveController = (IController)sender;

            ControllerData.SetController(ActiveController);
            ActiveController.Initalize();

            ActiveController.Identify();

            ui.InitalizeController();
        }

        private async void LoadTheme(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;

            await LoadTheme((string)item.Tag);
        }

        private async Task LoadTheme(string path, bool recordLast = true)
        {
            string skinJson = File.ReadAllText(path);

            {
                ThemeFixer.UI ui = JsonConvert.DeserializeObject<ThemeFixer.UI>(skinJson);
                ui.Update();
                skinJson = JsonConvert.SerializeObject(ui, Formatting.Indented);
            }

            ui = new UI(ControllerData, Path.GetDirectoryName(path), skinJson);
            this.Width = ui.Width;
            this.Height = ui.Height;

            if (recordLast)
            {
                settings.Theme = path;
                SaveSettings();
            }

            lblHint1.Hide();
            lblHint2.Hide();

            ttimer = new System.Threading.Timer(new TimerCallback(Render), null, 0, frameTime);
        }

        #region Drag Anywhere
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.NativeReleaseCapture();
                NativeMethods.NativeSendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, (IntPtr)0);
            }
        }
        #endregion Drag Anywhere

        protected override void OnPaint(PaintEventArgs e)
        {
            // Call the OnPaint method of the base class.  
            base.OnPaint(e);

            ui?.Paint(e.Graphics);
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            ttimer?.Change(Timeout.Infinite, Timeout.Infinite);
            this.Close();
        }

        private void tsmiReloadThemes_Click(object sender, EventArgs e)
        {
            LoadThemes();
        }

        private void tsmiReloadControllers_Click(object sender, EventArgs e)
        {
            LoadControllers(false);
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
                settings.Background = ColorTranslator.ToHtml(this.BackColor);
                SaveSettings();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ttimer?.Change(Timeout.Infinite, Timeout.Infinite);
            Controllers?.ForEach(dr => dr.DeInitalize());
        }
    }

    public class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        private static extern uint TimeBeginPeriod(uint delay);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        private static extern uint TimeEndPeriod(uint delay);

        public static IntPtr NativeSendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam)
        {
            return SendMessage(hWnd, Msg, wParam, lParam);
        }

        public static bool NativeReleaseCapture()
        {
            return ReleaseCapture();
        }

        public static uint NativeTimeBeginPeriod(int delay)
        {
            return TimeBeginPeriod((uint)delay);
        }

        public static uint NativeTimeEndPeriod(int delay)
        {
            return TimeEndPeriod((uint)delay);
        }
    }
}
