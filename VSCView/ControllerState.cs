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
    public interface IControl : ICloneable
    {
        T Value<T>(string key);
    }
    public class ControlCollection /*: ICloneable*/// where T : IControl
    {
        private Dictionary<string, IControl> Data = new Dictionary<string, IControl>();

        /*public string[] Keys
        {
            get
            {
                return Data.Keys.ToArray();
            }
        }*/
        public IControl this[string key]
        {
            get
            {
                if (Data.ContainsKey(key))
                    return Data[key];
                return default;
            }
            set
            {
                Data[key] = value;
            }
        }

        public object Clone()
        {
            ControlCollection newData = new ControlCollection();

            foreach (var key in Data.Keys)
            {
                newData[key] = (IControl)Data[key].Clone();
            }

            return newData;
        }
    }

    public enum EOrientation
    {
        Diamond,
        Square
    }
    public class ControlTriggerPair : IControl
    {
        public T Value<T>(string key)
        {
            return default;
        }

        public object Clone()
        {
            ControlTriggerPair newData = new ControlTriggerPair();
           
            return newData;
        }
    }
    public class ControlTrigger : IControl
    {
        public T Value<T>(string key)
        {
            return default;
        }

        public object Clone()
        {
            ControlTrigger newData = new ControlTrigger();

            return newData;
        }
    }
    public class ControlButtonQuad : IControl
    {
        public EOrientation Orientation { get; private set; }

        public bool Button0 { get; set; }
        public bool Button1 { get; set; }
        public bool Button2 { get; set; }
        public bool Button3 { get; set; }

        public ControlButtonQuad(EOrientation Orientation)
        {
            this.Orientation = Orientation;
        }

        public T Value<T>(string key)
        {
            switch(key)
            {
                case "0":
                    return (T)Convert.ChangeType(Button0, typeof(T));
                case "1":
                    return (T)Convert.ChangeType(Button1, typeof(T));
                case "2":
                    return (T)Convert.ChangeType(Button2, typeof(T));
                case "3":
                    return (T)Convert.ChangeType(Button3, typeof(T));
                default:
                    return default;
            }
        }

        public object Clone()
        {
            ControlButtonQuad newData = new ControlButtonQuad(this.Orientation);

            newData.Button0 = this.Button0;
            newData.Button1 = this.Button1;
            newData.Button2 = this.Button2;
            newData.Button3 = this.Button3;

            return newData;
        }
    }
    public class ControlButtonPair : IControl
    {
        public bool Button0 { get; set; }
        public bool Button1 { get; set; }

        public ControlButtonPair()
        {
        }

        public T Value<T>(string key)
        {
            switch (key)
            {
                case "0":
                    return (T)Convert.ChangeType(Button0, typeof(T));
                case "1":
                    return (T)Convert.ChangeType(Button1, typeof(T));
                default:
                    return default;
            }
        }

        public object Clone()
        {
            ControlButtonPair newData = new ControlButtonPair();

            newData.Button0 = this.Button0;
            newData.Button1 = this.Button1;

            return newData;
        }
    }
    public class ControlButton : IControl
    {
        public T Value<T>(string key)
        {
            return default;
        }

        public object Clone()
        {
            ControlButton newData = new ControlButton();

            return newData;
        }
    }
    public class ControlStick : IControl
    {
        public bool HasClick { get; private set; }

        public ControlStick(bool HasClick)
        {
            this.HasClick = HasClick;
        }

        public T Value<T>(string key)
        {
            return default;
        }

        public object Clone()
        {
            ControlStick newData = new ControlStick(this.HasClick);

            return newData;
        }
    }
    public class ControlTouch : IControl
    {
        public bool HasClick { get; private set; }
        public int TouchCount { get; private set; }

        public ControlTouch(int TouchCount, bool HasClick)
        {
            this.TouchCount = TouchCount;
            this.HasClick = HasClick;
        }

        public T Value<T>(string key)
        {
            return default;
        }

        public object Clone()
        {
            ControlTouch newData = new ControlTouch(this.TouchCount, this.HasClick);

            return newData;
        }
    }

    public class ControllerState : ICloneable
    {
        /*public ControlCollection<ControlTriggerPair> TriggerPairs { get; private set; }
        public ControlCollection<ControlTrigger> Triggers { get; private set; }
        public ControlCollection<ControlButtonQuad> ButtonQuads { get; private set; }
        public ControlCollection<ControlButtonPair> ButtonPairs { get; private set; }
        public ControlCollection<ControlButton> Buttons { get; private set; }
        public ControlCollection<ControlStick> Sticks { get; private set; }
        public ControlCollection<ControlTouch> Touch { get; private set; }*/
        public ControlCollection Controls { get; private set; }

        public ControllerState()
        {
            /*TriggerPairs = new ControlCollection<ControlTriggerPair>();
            Triggers = new ControlCollection<ControlTrigger>();
            ButtonQuads = new ControlCollection<ControlButtonQuad>();
            ButtonPairs = new ControlCollection<ControlButtonPair>();
            Buttons = new ControlCollection<ControlButton>();*/
            Controls = new ControlCollection();
        }

        public SteamControllerButtons ButtonsOld { get; set; }

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
        
        public object Clone()
        {
            ControllerState newState = new ControllerState();

            newState.Controls = (ControlCollection)this.Controls.Clone();

            newState.ButtonsOld = (SteamControllerButtons)this.ButtonsOld.Clone();

            newState.LeftTrigger = this.LeftTrigger;
            newState.RightTrigger = this.RightTrigger;

            newState.LeftStickX = this.LeftStickX;
            newState.LeftStickY = this.LeftStickY;
            newState.LeftPadX = this.LeftPadX;
            newState.LeftPadY = this.LeftPadY;
            newState.RightPadX = this.RightPadX;
            newState.RightPadY = this.RightPadY;

            newState.AccelerometerX = this.AccelerometerX;
            newState.AccelerometerY = this.AccelerometerY;
            newState.AccelerometerZ = this.AccelerometerZ;
            newState.AngularVelocityX = this.AngularVelocityX;
            newState.AngularVelocityY = this.AngularVelocityY;
            newState.AngularVelocityZ = this.AngularVelocityZ;
            newState.OrientationW = this.OrientationW;
            newState.OrientationX = this.OrientationX;
            newState.OrientationY = this.OrientationY;
            newState.OrientationZ = this.OrientationZ;

            return newState;
        }
    }
}
