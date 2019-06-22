using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    public class SteamControllerButtons : ICloneable
    {
        public bool A { get; set; }
        public bool B { get; set; }
        public bool X { get; set; }
        public bool Y { get; set; }

        public bool LeftBumper { get; set; }
        public bool LeftTrigger { get; set; }

        public bool RightBumper { get; set; }
        public bool RightTrigger { get; set; }

        public bool LeftGrip { get; set; }
        public bool RightGrip { get; set; }

        public bool Start { get; set; }
        public bool Home { get; set; }
        public bool Select { get; set; }
        public bool DS4PadClick { get; set; }

        public bool Down { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }
        public bool Up { get; set; }

        public bool LeftStickClick { get; set; }
        public bool LeftPadTouch { get; set; }
        public bool LeftPadClick { get; set; }
        public bool RightStickClick { get; set; }
        public bool RightPadTouch { get; set; }
        public bool RightPadClick { get; set; }

        public bool Touch0 { get; set; }
        public bool Touch1 { get; set; }
        public bool Touch2 { get; set; }
        public bool Touch3 { get; set; }

        public virtual object Clone()
        {
            SteamControllerButtons buttons = (SteamControllerButtons)base.MemberwiseClone();

            buttons.A = A;
            buttons.B = B;
            buttons.X = X;
            buttons.Y = Y;

            buttons.LeftBumper = LeftBumper;
            buttons.LeftTrigger = LeftTrigger;

            buttons.RightBumper = RightBumper;
            buttons.RightTrigger = RightTrigger;

            buttons.LeftGrip = LeftGrip;
            buttons.RightGrip = RightGrip;

            buttons.Start = Start;
            buttons.Home = Home;
            buttons.Select = Select;

            buttons.Down = Down;
            buttons.Left = Left;
            buttons.Right = Right;
            buttons.Up = Up;

            buttons.LeftStickClick = LeftStickClick;
            buttons.LeftPadTouch = LeftPadTouch;
            buttons.LeftPadClick = LeftPadClick;
            buttons.RightStickClick = RightStickClick;
            buttons.RightPadTouch = RightPadTouch;
            buttons.RightPadClick = RightPadClick;

            buttons.Touch0 = Touch0;
            buttons.Touch1 = Touch1;
            buttons.Touch2 = Touch2;
            buttons.Touch3 = Touch3;

            buttons.DS4PadClick = DS4PadClick;

            return buttons;
        }
    }

    public class ControllerState
    {
        public SteamControllerButtons Buttons { get; set; }

        public float LeftTrigger { get; set; }
        public float RightTrigger { get; set; }

        public float LeftStickX { get; set; }
        public float LeftStickY { get; set; }
        public float LeftPadX { get; set; }
        public float LeftPadY { get; set; }
        public float RightStickX { get; set; }
        public float RightStickY { get; set; }
        public float RightPadX { get; set; }
        public float RightPadY { get; set; }

        public Int16 AccelerometerX { get; set; }
        public Int16 AccelerometerY { get; set; }
        public Int16 AccelerometerZ { get; set; }
        public Int16 AngularVelocityX { get; set; }
        public Int16 AngularVelocityY { get; set; }
        public Int16 AngularVelocityZ { get; set; }
        public Int16 OrientationW { get; set; }
        public Int16 OrientationX { get; set; }
        public Int16 OrientationY { get; set; }
        public Int16 OrientationZ { get; set; }

        public bool DataStuck { get; set; }
    }
}
