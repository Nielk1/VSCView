using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSCView
{
    public partial class MainForm : Form
    {
        //SteamController[] Controllers;
        //SteamController.SteamControllerState State;

        ControllerData ControllerData;
        SteamController ActiveController;

        //short AngularVelocityXMax = 5000;
        //short AngularVelocityYMax = 5000;
        //short AngularVelocityZMax = 5000;

        UI ui;

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();

            ControllerData = new ControllerData();
            //ActiveController.SetController(Controllers[0]);

            //ui = new UI(ActiveController, "default",);

            //this.Width = ui.Width;
            //this.Height = ui.Height;

            //Controllers = SteamController.GetControllers();
            //ActiveController.SetController(Controllers[0]);
            //if (Controllers.Length > 0) Controllers[0].StateUpdated += (object sender, SteamController.SteamControllerState e) => MainForm_StateUpdated(sender, e, 0);
            //if (Controllers.Length > 1) Controllers[1].StateUpdated += (object sender, SteamController.SteamControllerState e) => MainForm_StateUpdated(sender, e, 1);
            //if (Controllers.Length > 2) Controllers[2].StateUpdated += (object sender, SteamController.SteamControllerState e) => MainForm_StateUpdated(sender, e, 2);
            //if (Controllers.Length > 3) Controllers[3].StateUpdated += (object sender, SteamController.SteamControllerState e) => MainForm_StateUpdated(sender, e, 3);

            LoadThemes();
            LoadControllers();
        }

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

        private void LoadTheme(object sender, EventArgs e)
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

        private void tmrPaint_Tick(object sender, EventArgs e)
        {
            this.Invalidate(); // we can invalidate specific zones
            this.Update();
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
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
    }
}
