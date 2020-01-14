using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        Dictionary<string, ToolStripMenuItem> ThemeMenuItems = new Dictionary<string, ToolStripMenuItem>();

        ControllerData ControllerData;
        IController ActiveController;
        List<IController> Controllers = new List<IController>();
        System.Threading.Timer ttimer;

        List<IControllerFactory> Factory;

        UI ui;

        Settings settings;

        public MainForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None; // no borders
            this.SetStyle(ControlStyles.ResizeRedraw, true); // this is to avoid visual artifacts

            RegistryKey winLogonKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\HidGuardian", false);
            if (winLogonKey != null)
            {
                hIDGuardianWhitelistToolStripMenuItem.Visible = true;
                RegistryKey whitelistKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters\Whitelist\" + Process.GetCurrentProcess().Id, false);
                hIDGuardianWhitelistToolStripMenuItem.Checked = whitelistKey != null;
            }

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
            if (File.Exists("ctrl.last"))
            {
                settings.Theme = File.ReadAllText("ctrl.last");
                File.Delete("ctrl.last");
            }
            if(settings.CustomSize)
            {
                this.Width = settings.Width ?? this.Width;
                this.Height = settings.Height ?? this.Height;
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

        #region Resizeing
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        const int cornerSize = 10; // you can rename this variable if you like

        Rectangle TopRect { get { return new Rectangle(0, 0, this.ClientSize.Width, cornerSize); } }
        Rectangle LeftRect { get { return new Rectangle(0, 0, cornerSize, this.ClientSize.Height); } }
        Rectangle BottomRect { get { return new Rectangle(0, this.ClientSize.Height - cornerSize, this.ClientSize.Width, cornerSize); } }
        Rectangle RightRect { get { return new Rectangle(this.ClientSize.Width - cornerSize, 0, cornerSize, this.ClientSize.Height); } }

        Rectangle TopLeftRect { get { return new Rectangle(0, 0, cornerSize, cornerSize); } }
        Rectangle TopRightRect { get { return new Rectangle(this.ClientSize.Width - cornerSize, 0, cornerSize, cornerSize); } }
        Rectangle BottomLeftRect { get { return new Rectangle(0, this.ClientSize.Height - cornerSize, cornerSize, cornerSize); } }
        Rectangle BottomRightRect { get { return new Rectangle(this.ClientSize.Width - cornerSize, this.ClientSize.Height - cornerSize, cornerSize, cornerSize); } }

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);

            if (message.Msg == 0x84) // WM_NCHITTEST
            {
                var cursor = this.PointToClient(Cursor.Position);

                if (TopLeftRect.Contains(cursor)) message.Result = (IntPtr)HTTOPLEFT;
                else if (TopRightRect.Contains(cursor)) message.Result = (IntPtr)HTTOPRIGHT;
                else if (BottomLeftRect.Contains(cursor)) message.Result = (IntPtr)HTBOTTOMLEFT;
                else if (BottomRightRect.Contains(cursor)) message.Result = (IntPtr)HTBOTTOMRIGHT;

                else if (TopRect.Contains(cursor)) message.Result = (IntPtr)HTTOP;
                else if (LeftRect.Contains(cursor)) message.Result = (IntPtr)HTLEFT;
                else if (RightRect.Contains(cursor)) message.Result = (IntPtr)HTRIGHT;
                else if (BottomRect.Contains(cursor)) message.Result = (IntPtr)HTBOTTOM;
            }
        }
        #endregion Resizeing

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
            ThemeMenuItems.Clear();

            if (!Directory.Exists("themes")) Directory.CreateDirectory("themes");
            string[] themeParents = Directory.GetFiles("themes", "theme.json", SearchOption.AllDirectories);
            themeParents = themeParents.Select(dr => Path.GetDirectoryName(dr)).Distinct().ToArray();

            foreach (string themeParent in themeParents)
            {
                string[] PathMiddleParts = themeParent.Split(Path.DirectorySeparatorChar).Reverse().Skip(1).Reverse().Skip(1).ToArray();

                ToolStripMenuItem parentMenuItem = tsmiTheme;
                for (int j = 0; j < PathMiddleParts.Length; j++)
                {
                    string dir_key = string.Join(Path.DirectorySeparatorChar.ToString(), PathMiddleParts.Take(j + 1).ToArray());
                    if (!ThemeMenuItems.ContainsKey(dir_key))
                    {
                        string nameFile = Path.Combine("themes", dir_key, "name.txt");
                        string icon_file = Path.Combine("themes", dir_key, "icon.png");
                        string name = File.Exists(nameFile) ? File.ReadAllText(nameFile).Trim() : null;
                        if (string.IsNullOrWhiteSpace(name))
                            name = PathMiddleParts.Skip(j).First();
                        ThemeMenuItems[dir_key] = new ToolStripMenuItem(name);
                        if (File.Exists(icon_file))
                        {
                            ThemeMenuItems[dir_key].Image = Image.FromFile(icon_file);
                            ThemeMenuItems[dir_key].ImageScaling = ToolStripItemImageScaling.None;
                        }
                        parentMenuItem.DropDownItems.Add(ThemeMenuItems[dir_key]);
                    }
                    parentMenuItem = ThemeMenuItems[dir_key];
                }

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
                string themeIconFile = Path.Combine(themeParent, "icon.png");
                if (File.Exists(themeIconFile))
                {
                    itmTop.Image = Image.FromFile(themeIconFile);
                    itmTop.ImageScaling = ToolStripItemImageScaling.None;
                }
                parentMenuItem.DropDownItems.Add(itmTop);

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
                Controllers[i].ControllerNameUpdated += () =>
                {
                    try
                    {
                        if (this.Created && !this.Disposing && !this.IsDisposed)
                            this.Invoke(new Action(() =>
                            {
                                itm.Text = c.GetName();
                            }));
                    }
                    catch (ObjectDisposedException e) { /* eat the Disposed exception when exiting */ }
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

            new Thread(() =>
            {
                ActiveController.Identify();
            }).Start();

            ui?.InitalizeController();
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
            if (!settings.CustomSize)
            {
                this.Width = ui.Width;
                this.Height = ui.Height;
            }

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

            if (ui != null)
            {
                float ratioWidth = 1.0f * this.Width / ui.Width;
                float ratioHeight = 1.0f * this.Height / ui.Height;
                float ratio = Math.Min(ratioWidth, ratioHeight);

                //Matrix preserve = e.Graphics.Transform;

                e.Graphics.TranslateTransform(ui.Width * (ratioWidth - ratio) / 2, ui.Height * (ratioHeight - ratio) / 2);
                e.Graphics.ScaleTransform(ratio, ratio);

                ui.Paint(e.Graphics);

                //e.Graphics.Transform = preserve;
            }
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
            if (colorDialog1.ShowDialog() == DialogResult.OK)
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

        private void MinimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void HIDGuardianWhitelistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = System.Reflection.Assembly.GetEntryAssembly().Location,
                Arguments = $"admin {settings.PreviousPid} {Process.GetCurrentProcess().Id}",
                UseShellExecute = true,
                Verb = "runas",
            }).WaitForExit();
            settings.PreviousPid = Process.GetCurrentProcess().Id;
            SaveSettings();
            RegistryKey whitelistKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters\Whitelist\" + Process.GetCurrentProcess().Id, false);
            hIDGuardianWhitelistToolStripMenuItem.Checked = whitelistKey != null;
        }

        private void TsmiResetWindowSize_Click(object sender, EventArgs e)
        {
            if (ui != null)
            {
                this.Width = ui.Width;
                this.Height = ui.Height;
                settings.CustomSize = false;
                settings.Width = null;
                settings.Height = null;
                SaveSettings();
            }
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            if (ui != null)
                if (ui.Width != this.Width || ui.Height != this.Height)
                {
                    settings.CustomSize = true;
                    settings.Width = this.Width;
                    settings.Height = this.Height;
                }
                else
                {
                    this.Width = ui.Width;
                    this.Height = ui.Height;
                    settings.CustomSize = false;
                    settings.Width = null;
                    settings.Height = null;
                }
            SaveSettings();
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
