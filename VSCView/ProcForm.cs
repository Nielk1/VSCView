using HidLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSCView
{
    public partial class ProcForm : Form
    {
        SteamController[] Controllers;
        TextBox[] txtTemp;

        public ProcForm()
        {
            InitializeComponent();

            SetDoubleBuffered(txtTemp1);
            SetDoubleBuffered(txtTemp2);
            SetDoubleBuffered(txtTemp3);
            SetDoubleBuffered(txtTemp4);
            txtTemp = new TextBox[] { txtTemp1, txtTemp2, txtTemp3, txtTemp4 }; ;

            Controllers = SteamController.GetControllers();

            //Controllers[0].StateUpdated += MainForm_StateUpdated;
            if (Controllers.Length > 0) Controllers[0].StateUpdated += (object sender, SteamController.SteamControllerState e) => MainForm_StateUpdated(sender, e, 0);
            if (Controllers.Length > 1) Controllers[1].StateUpdated += (object sender, SteamController.SteamControllerState e) => MainForm_StateUpdated(sender, e, 1);
            if (Controllers.Length > 2) Controllers[2].StateUpdated += (object sender, SteamController.SteamControllerState e) => MainForm_StateUpdated(sender, e, 2);
            if (Controllers.Length > 3) Controllers[3].StateUpdated += (object sender, SteamController.SteamControllerState e) => MainForm_StateUpdated(sender, e, 3);
        }

        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }

        private void MainForm_StateUpdated(object sender, SteamController.SteamControllerState e, int index)
        {
            txtTemp1.Invoke((MethodInvoker)delegate
            {
                // Running on the UI thread
                txtTemp[index].Text =
                    $"Buttons.A = {e.Buttons.A}\r\n" +
                    $"Buttons.B = {e.Buttons.B}\r\n" +
                    $"Buttons.X = {e.Buttons.X}\r\n" +
                    $"Buttons.Y = {e.Buttons.Y}\r\n" +
                    "\r\n" +
                    $"Buttons.LeftBumper = {e.Buttons.LeftBumper}\r\n" +
                    $"Buttons.LeftTrigger = {e.Buttons.LeftTrigger}\r\n" +
                    $"Buttons.LeftGrip = {e.Buttons.LeftGrip}\r\n" +
                    "\r\n" +
                    $"Buttons.RightBumper = {e.Buttons.RightBumper}\r\n" +
                    $"Buttons.RightTrigger = {e.Buttons.RightTrigger}\r\n" +
                    $"Buttons.RightGrip = {e.Buttons.RightGrip}\r\n" +
                    "\r\n" +
                    $"Buttons.CenterRight = {e.Buttons.Start}\r\n" +
                    $"Buttons.Center = {e.Buttons.Steam}\r\n" +
                    $"Buttons.CenterLeft = {e.Buttons.Select}\r\n" +
                    "\r\n" +
                    $"Buttons.Up = {e.Buttons.Up}\r\n" +
                    $"Buttons.Down = {e.Buttons.Down}\r\n" +
                    $"Buttons.Left = {e.Buttons.Left}\r\n" +
                    $"Buttons.Right = {e.Buttons.Right}\r\n" +
                    "\r\n" +
                    $"Buttons.ThumbStick = {e.Buttons.StickClick}\r\n" +
                    $"Buttons.LeftPadTouch = {e.Buttons.LeftPadTouch}\r\n" +
                    $"Buttons.LeftPadPress = {e.Buttons.LeftPadClick}\r\n" +
                    $"Buttons.RightPadTouch = {e.Buttons.RightPadTouch}\r\n" +
                    $"Buttons.RightPadPress = {e.Buttons.RightPadClick}\r\n" +
                    "\r\n" +
                    $"LeftTrigger = {e.LeftTrigger}\r\n" +
                    $"RightTrigger = {e.RightTrigger}\r\n" +
                    "\r\n" +
                    $"LeftStickX = {e.LeftStickX}\r\n" +
                    $"LeftStickY = {e.LeftStickY}\r\n" +
                    $"LeftPadX = {e.LeftPadX}\r\n" +
                    $"LeftPadY = {e.LeftPadY}\r\n" +
                    $"RightPadX = {e.RightPadX}\r\n" +
                    $"RightPadY = {e.RightPadY}\r\n" +
                    "\r\n" +
                    $"AngularVelocityX = {e.AngularVelocityX}\r\n" +
                    $"AngularVelocityY = {e.AngularVelocityY}\r\n" +
                    $"AngularVelocityZ = {e.AngularVelocityZ}\r\n" +
                    $"OrientationW = {e.OrientationW}\r\n" +
                    $"OrientationX = {e.OrientationX}\r\n" +
                    $"OrientationY = {e.OrientationY}\r\n" +
                    $"OrientationZ = {e.OrientationZ}\r\n";
            });
        }
    }
}
